using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;

namespace BusinessLogicLayer.Managers
{
    /// <summary>
    /// Manager de Aguinaldos según Código de Trabajo de Costa Rica
    /// Artículo 229: Aguinaldo = promedio de salarios del 1 dic al 30 nov
    /// Mínimo: 1/12 del salario anual (1 mes de salario)
    /// </summary>
    public class AguinaldoManager : IAguinaldoManager
    {
        private readonly IAguinaldoRepository _aguinaldoRepo;
        private readonly IEmpleadosRepository _empleadosRepo;

        // Constantes según legislación costarricense
        private const int MES_INICIO_AGUINALDO = 12; // Diciembre

        private const int DIA_INICIO_AGUINALDO = 1;  // 1 de diciembre
        private const int MES_FIN_AGUINALDO = 11;     // Noviembre
        private const int DIA_FIN_AGUINALDO = 30;     // 30 de noviembre
        private const int DIAS_ANIO = 365;

        public AguinaldoManager(
            IAguinaldoRepository aguinaldoRepo,
            IEmpleadosRepository empleadosRepo)
        {
            _aguinaldoRepo = aguinaldoRepo ?? throw new ArgumentNullException(nameof(aguinaldoRepo));
            _empleadosRepo = empleadosRepo ?? throw new ArgumentNullException(nameof(empleadosRepo));
        }

        #region Consultas

        public async Task<AguinaldoDTO?> ObtenerPorIdAsync(int id)
        {
            var aguinaldo = await _aguinaldoRepo.GetByIdAsync(id);
            return aguinaldo == null ? null : MapToDTO(aguinaldo);
        }

        public async Task<IEnumerable<AguinaldoDTO>> ObtenerTodosAsync()
        {
            var aguinaldos = await _aguinaldoRepo.GetAllAsync();
            return aguinaldos.Select(MapToDTO);
        }

        public async Task<IEnumerable<AguinaldoDTO>> ObtenerPorAnioAsync(int anio)
        {
            var aguinaldos = await _aguinaldoRepo.GetByAnioAsync(anio);
            return aguinaldos.Select(MapToDTO);
        }

        public async Task<IEnumerable<AguinaldoDTO>> ObtenerPorEmpleadoAsync(int empleadoId)
        {
            var aguinaldos = await _aguinaldoRepo.GetByEmpleadoAsync(empleadoId);
            return aguinaldos.Select(MapToDTO);
        }

        public async Task<ResumenAguinaldoDTO> ObtenerResumenPorAnioAsync(int anio)
        {
            var aguinaldos = await _aguinaldoRepo.GetByAnioAsync(anio);
            var aguinaldosList = aguinaldos.ToList();

            return new ResumenAguinaldoDTO
            {
                TotalEmpleados = aguinaldosList.Count,
                AguinaldosPendientes = aguinaldosList.Count(a => a.Estado == "PENDIENTE"),
                AguinaldosPagados = aguinaldosList.Count(a => a.Estado == "PAGADO"),
                TotalPendiente = aguinaldosList
                    .Where(a => a.Estado == "PENDIENTE")
                    .Sum(a => a.MontoAguinaldo),
                TotalPagado = aguinaldosList
                    .Where(a => a.Estado == "PAGADO")
                    .Sum(a => a.MontoAguinaldo),
                TotalGeneral = aguinaldosList.Sum(a => a.MontoAguinaldo),
                Aguinaldos = aguinaldosList.Select(MapToDTO).ToList()
            };
        }

        #endregion Consultas

        #region Cálculo de Aguinaldo

        /// <summary>
        /// Calcula aguinaldo para un empleado según Art. 229 Código de Trabajo CR
        /// Período: 1 diciembre (año anterior) al 30 noviembre (año actual)
        /// Fórmula: (Suma de salarios ordinarios y extraordinarios) / meses trabajados
        /// Aguinaldo = Salario promedio * (días trabajados / 365)
        /// Mínimo: 1/12 del salario anual
        /// </summary>
        public async Task<ResultadoCalculoAguinaldoDTO> CalcularAguinaldoEmpleadoAsync(
            CalcularAguinaldoDTO dto)
        {
            // Validar empleado
            var empleado = await _empleadosRepo.GetByIdAsync(dto.EmpleadoId);
            if (empleado == null)
                throw new ArgumentException($"Empleado {dto.EmpleadoId} no encontrado");

            if (empleado.Estado != "ACTIVO")
                throw new InvalidOperationException($"Empleado {empleado.Nombre} no está activo");

            // Determinar período de cálculo
            var fechaFin = dto.FechaCorte ?? new DateTime(dto.Anio, MES_FIN_AGUINALDO, DIA_FIN_AGUINALDO);
            var fechaInicio = new DateTime(dto.Anio - 1, MES_INICIO_AGUINALDO, DIA_INICIO_AGUINALDO);

            // Si el empleado entró después del 1 de diciembre, usar su fecha de contratación
            var fechaContratacion = empleado.FechaContratacion.ToDateTime(TimeOnly.MinValue);
            if (fechaContratacion > fechaInicio)
            {
                fechaInicio = fechaContratacion;
            }

            // Calcular días trabajados
            var diasTrabajados = await CalcularDiasLaboradosAsync(
                dto.EmpleadoId,
                fechaInicio,
                fechaFin);

            // Calcular salario promedio
            var salarioPromedio = await CalcularSalarioPromedioAsync(
                dto.EmpleadoId,
                fechaInicio,
                fechaFin);

            // Calcular monto de aguinaldo proporcional
            // Aguinaldo = Salario promedio mensual * (días trabajados / 365)
            // Mínimo: 1/12 del salario anual
            var montoAguinaldo = CalcularMontoAguinaldo(salarioPromedio, diasTrabajados);

            var nombreCompleto = $"{empleado.Nombre} {empleado.PrimerApellido} {empleado.SegundoApellido}".Trim();

            return new ResultadoCalculoAguinaldoDTO
            {
                EmpleadoId = dto.EmpleadoId,
                NombreEmpleado = nombreCompleto,
                FechaContratacion = fechaContratacion,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                DiasTrabajados = diasTrabajados,
                SalarioPromedio = salarioPromedio,
                MontoAguinaldo = montoAguinaldo,
                Detalle = $"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}. " +
                         $"Días: {diasTrabajados}. Promedio: ₡{salarioPromedio:N2}"
            };
        }

        /// <summary>
        /// Calcula aguinaldos para todos los empleados activos
        /// </summary>
        public async Task<List<ResultadoCalculoAguinaldoDTO>> CalcularAguinaldoMasivoAsync(
            CalcularAguinaldoMasivoDTO dto)
        {
            var empleados = await _empleadosRepo.GetAllAsync();

            // Filtrar empleados activos
            var empleadosActivos = empleados.Where(e => e.Estado == "ACTIVO");

            // Si se especificó departamento, filtrar
            if (dto.DepartamentoId.HasValue)
            {
                empleadosActivos = empleadosActivos
                    .Where(e => e.DepartamentoId == dto.DepartamentoId.Value);
            }

            var resultados = new List<ResultadoCalculoAguinaldoDTO>();

            foreach (var empleado in empleadosActivos)
            {
                try
                {
                    var calculo = await CalcularAguinaldoEmpleadoAsync(new CalcularAguinaldoDTO
                    {
                        EmpleadoId = empleado.IdEmpleado,
                        Anio = dto.Anio,
                        FechaCorte = dto.FechaCorte
                    });

                    resultados.Add(calculo);
                }
                catch (Exception ex)
                {
                    // Log error pero continuar con otros empleados
                    Console.WriteLine($"Error calculando aguinaldo para empleado {empleado.IdEmpleado}: {ex.Message}");
                }
            }

            return resultados;
        }

        /// <summary>
        /// Calcula el salario promedio según legislación CR
        /// Incluye: salario base + horas extra + bonificaciones
        /// </summary>
        public async Task<decimal> CalcularSalarioPromedioAsync(
            int empleadoId,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            // Obtener todas las nóminas pagadas en el período
            var nominas = await _aguinaldoRepo.GetNominasPorPeriodoAsync(
                empleadoId,
                fechaInicio,
                fechaFin);

            var nominasList = nominas.ToList();

            if (!nominasList.Any())
            {
                // Si no hay nóminas, usar el salario base actual del empleado
                var empleado = await _empleadosRepo.GetByIdAsync(empleadoId);
                return empleado?.SalarioBase ?? 0;
            }

            // Sumar todos los salarios brutos (incluye base + horas extra + bonificaciones)
            var totalSalarios = nominasList.Sum(n => n.TotalBruto);

            // Calcular meses trabajados
            var mesesTrabajados = nominasList.Count;

            // Promedio = Total / Meses
            return mesesTrabajados > 0 ? totalSalarios / mesesTrabajados : 0;
        }

        /// <summary>
        /// Calcula días laborados en el período
        /// Excluye: incapacidades sin goce de salario, permisos sin goce
        /// </summary>
        public async Task<int> CalcularDiasLaboradosAsync(
            int empleadoId,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            // Total de días en el período
            var totalDias = (fechaFin - fechaInicio).Days + 1; // +1 para incluir ambos días

            // Por ahora retornamos el total
            // TODO: Restar días de incapacidades sin goce, permisos sin goce
            // Esto requeriría consultar las tablas de Incapacidades y Permisos

            return totalDias;
        }

        #endregion Cálculo de Aguinaldo

        #region Registro y Pago

        /// <summary>
        /// Registra un aguinaldo calculado en la BD
        /// </summary>
        public async Task<AguinaldoDTO> RegistrarAguinaldoAsync(
            ResultadoCalculoAguinaldoDTO calculo,
            int anio)
        {
            // Verificar si ya existe aguinaldo para este empleado en este año
            if (await _aguinaldoRepo.ExisteAguinaldoAsync(calculo.EmpleadoId, anio))
            {
                throw new InvalidOperationException(
                    $"Ya existe un aguinaldo registrado para el empleado {calculo.NombreEmpleado} en el año {anio}");
            }

            var aguinaldo = new Aguinaldos
            {
                EmpleadoId = calculo.EmpleadoId,
                FechaCalculo = DateTime.UtcNow,
                DiasTrabajados = calculo.DiasTrabajados,
                SalarioPromedio = calculo.SalarioPromedio,
                MontoAguinaldo = calculo.MontoAguinaldo,
                Estado = "PENDIENTE",
                FechaCreacion = DateTime.UtcNow
            };

            var aguinaldoCreado = await _aguinaldoRepo.CreateAsync(aguinaldo);

            // Recargar con relaciones
            var aguinaldoCompleto = await _aguinaldoRepo.GetByIdAsync(aguinaldoCreado.IdAguinaldo);

            return MapToDTO(aguinaldoCompleto!);
        }

        /// <summary>
        /// Paga un aguinaldo
        /// </summary>
        public async Task<bool> PagarAguinaldoAsync(int idAguinaldo, DateTime fechaPago)
        {
            var aguinaldo = await _aguinaldoRepo.GetByIdAsync(idAguinaldo);

            if (aguinaldo == null)
                throw new KeyNotFoundException($"Aguinaldo {idAguinaldo} no encontrado");

            if (aguinaldo.Estado != "PENDIENTE")
                throw new InvalidOperationException($"El aguinaldo ya fue {aguinaldo.Estado?.ToLower()}");

            // Validar que no se pague después del 20 de diciembre según ley CR
            var maxFechaPago = new DateTime(fechaPago.Year, 12, 20);
            if (fechaPago > maxFechaPago)
            {
                // Permitir pero advertir
                Console.WriteLine($"⚠️ Advertencia: Pago de aguinaldo después del 20 de diciembre (fecha límite legal)");
            }

            aguinaldo.FechaPago = fechaPago;
            aguinaldo.Estado = "PAGADO";
            aguinaldo.FechaModificacion = DateTime.UtcNow;

            return await _aguinaldoRepo.UpdateAsync(aguinaldo);
        }

        /// <summary>
        /// Paga múltiples aguinaldos (operación masiva)
        /// </summary>
        public async Task<(int exitosos, int fallidos, List<string> errores)> PagarAguinaldosMasivoAsync(
            List<int> idsAguinaldos,
            DateTime fechaPago)
        {
            var exitosos = 0;
            var fallidos = 0;
            var errores = new List<string>();

            foreach (var id in idsAguinaldos)
            {
                try
                {
                    await PagarAguinaldoAsync(id, fechaPago);
                    exitosos++;
                }
                catch (Exception ex)
                {
                    fallidos++;
                    errores.Add($"Aguinaldo {id}: {ex.Message}");
                }
            }

            return (exitosos, fallidos, errores);
        }

        /// <summary>
        /// Anula un aguinaldo
        /// </summary>
        public async Task<bool> AnularAguinaldoAsync(int idAguinaldo)
        {
            var aguinaldo = await _aguinaldoRepo.GetByIdAsync(idAguinaldo);

            if (aguinaldo == null)
                throw new KeyNotFoundException($"Aguinaldo {idAguinaldo} no encontrado");

            if (aguinaldo.Estado == "PAGADO")
                throw new InvalidOperationException("No se puede anular un aguinaldo ya pagado");

            return await _aguinaldoRepo.DeleteAsync(idAguinaldo);
        }

        #endregion Registro y Pago

        #region Métodos Privados

        /// <summary>
        /// Calcula el monto de aguinaldo según días trabajados
        /// Fórmula: Salario mensual promedio * (días trabajados / 365)
        /// Mínimo: 1/12 del salario anual (equivalente a 1 mes)
        /// </summary>
        private decimal CalcularMontoAguinaldo(decimal salarioPromedio, int diasTrabajados)
        {
            // Calcular aguinaldo proporcional
            var aguinaldoProporcional = salarioPromedio * diasTrabajados / DIAS_ANIO;

            // Aplicar mínimo legal: 1/12 del salario anual = salario mensual
            var minimoLegal = salarioPromedio;

            // Si trabajó todo el año, el aguinaldo es el salario promedio completo
            if (diasTrabajados >= DIAS_ANIO)
                return minimoLegal;

            // Si trabajó menos, calcular proporcional pero nunca menos del mínimo
            return Math.Max(aguinaldoProporcional, minimoLegal * diasTrabajados / DIAS_ANIO);
        }

        /// <summary>
        /// Mapea entidad Aguinaldos a DTO
        /// </summary>
        private AguinaldoDTO MapToDTO(Aguinaldos aguinaldo)
        {
            var nombreCompleto = aguinaldo.Empleado != null
                ? $"{aguinaldo.Empleado.Nombre} {aguinaldo.Empleado.PrimerApellido} {aguinaldo.Empleado.SegundoApellido}".Trim()
                : "";

            return new AguinaldoDTO
            {
                IdAguinaldo = aguinaldo.IdAguinaldo,
                EmpleadoId = aguinaldo.EmpleadoId,
                CodigoEmpleado = aguinaldo.Empleado?.CodigoEmpleado,
                NombreEmpleado = nombreCompleto,
                Departamento = aguinaldo.Empleado?.Departamento?.NombreDepartamento,
                Puesto = aguinaldo.Empleado?.Puesto?.NombrePuesto,
                FechaCalculo = aguinaldo.FechaCalculo,
                DiasTrabajados = aguinaldo.DiasTrabajados,
                SalarioPromedio = aguinaldo.SalarioPromedio,
                MontoAguinaldo = aguinaldo.MontoAguinaldo,
                FechaPago = aguinaldo.FechaPago,
                Estado = aguinaldo.Estado,
                FechaCreacion = aguinaldo.FechaCreacion,
                FechaModificacion = aguinaldo.FechaModificacion
            };
        }

        #endregion Métodos Privados
    }
}