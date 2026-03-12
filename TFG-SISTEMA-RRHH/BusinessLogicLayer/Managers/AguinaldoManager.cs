using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogicLayer.Managers
{
    public class AguinaldoManager : IAguinaldoManager
    {
        private const int DIA_FIN_AGUINALDO = 30;
        private const int DIA_INICIO_AGUINALDO = 1;
        private const int DIAS_ANIO = 365;
        private const int MES_FIN_AGUINALDO = 11;
        private const int MES_INICIO_AGUINALDO = 12;

        private readonly IAguinaldoRepository _aguinaldoRepo;
        private readonly IEmpleadosRepository _empleadosRepo;
        private readonly ILogger<AguinaldoManager> _logger;

        public AguinaldoManager(
            IAguinaldoRepository aguinaldoRepo,
            IEmpleadosRepository empleadosRepo,
            ILogger<AguinaldoManager> logger)
        {
            _aguinaldoRepo = aguinaldoRepo ?? throw new ArgumentNullException(nameof(aguinaldoRepo));
            _empleadosRepo = empleadosRepo ?? throw new ArgumentNullException(nameof(empleadosRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Consultas

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

        public async Task<AguinaldoDTO?> ObtenerPorIdAsync(int id)
        {
            var aguinaldo = await _aguinaldoRepo.GetByIdAsync(id);
            return aguinaldo == null ? null : MapToDTO(aguinaldo);
        }

        public async Task<ResumenAguinaldoDTO> ObtenerResumenPorAnioAsync(int anio)
        {
            var aguinaldos = await _aguinaldoRepo.GetByAnioAsync(anio);
            var lista = aguinaldos.ToList();

            return new ResumenAguinaldoDTO
            {
                TotalEmpleados = lista.Count,
                AguinaldosPendientes = lista.Count(a => a.Estado == "PENDIENTE"),
                AguinaldosPagados = lista.Count(a => a.Estado == "PAGADO"),
                TotalPendiente = lista.Where(a => a.Estado == "PENDIENTE")
                                           .Sum(a => a.MontoAguinaldo),
                TotalPagado = lista.Where(a => a.Estado == "PAGADO")
                                           .Sum(a => a.MontoAguinaldo),
                Aguinaldos = lista.Select(MapToDTO).ToList()
            };
        }

        public async Task<IEnumerable<AguinaldoDTO>> ObtenerTodosAsync()
        {
            var aguinaldos = await _aguinaldoRepo.GetAllAsync();
            return aguinaldos.Select(MapToDTO);
        }

        #endregion Consultas

        #region Cálculo y Registro

        public async Task<AguinaldoDTO> CalcularAguinaldoEmpleadoAsync(CalcularAguinaldoDTO dto)
        {
            var empleado = await _empleadosRepo.GetByIdAsync(dto.EmpleadoId);

            if (empleado == null)
                throw new ArgumentException($"Empleado {dto.EmpleadoId} no encontrado");

            if (empleado.Estado != "ACTIVO")
                throw new InvalidOperationException(
                    $"Empleado {empleado.Nombre} no está activo");

            // FIX #7: validar FechaCorte antes de usarla
            var fechaLimiteLegal = new DateTime(dto.Anio, MES_FIN_AGUINALDO, DIA_FIN_AGUINALDO);
            var fechaFin = dto.FechaCorte.HasValue
                ? dto.FechaCorte.Value <= fechaLimiteLegal
                    ? dto.FechaCorte.Value
                    : throw new ArgumentException(
                        $"La fecha de corte no puede ser posterior al {fechaLimiteLegal:dd/MM/yyyy}")
                : fechaLimiteLegal;

            var fechaInicio = new DateTime(dto.Anio - 1, MES_INICIO_AGUINALDO, DIA_INICIO_AGUINALDO);
            var fechaContratacion = empleado.FechaContratacion.ToDateTime(TimeOnly.MinValue);

            if (fechaContratacion > fechaFin)
                throw new InvalidOperationException(
                    $"Empleado {empleado.Nombre} fue contratado después del período de cálculo");

            if (fechaContratacion > fechaInicio)
                fechaInicio = fechaContratacion;

            if (await _aguinaldoRepo.ExisteAguinaldoAsync(dto.EmpleadoId, dto.Anio))
                throw new InvalidOperationException(
                    $"Ya existe un aguinaldo registrado para {empleado.Nombre} en el año {dto.Anio}");

            var diasTrabajados = CalcularDiasLaborados(fechaInicio, fechaFin);
            var salarioPromedio = await CalcularSalarioPromedioAsync(dto.EmpleadoId, fechaInicio, fechaFin, empleado.SalarioBase);
            var montoAguinaldo = CalcularMontoAguinaldo(salarioPromedio, diasTrabajados);

            var entidad = new Aguinaldos
            {
                EmpleadoId = dto.EmpleadoId,
                Anio = dto.Anio,        // FIX #1: poblar el campo Anio
                FechaCalculo = DateTime.UtcNow,
                DiasTrabajados = diasTrabajados,
                SalarioPromedio = salarioPromedio,
                MontoAguinaldo = montoAguinaldo,
                Estado = "PENDIENTE",
                FechaCreacion = DateTime.UtcNow
            };

            var creado = await _aguinaldoRepo.CreateAsync(entidad);
            var completo = await _aguinaldoRepo.GetByIdAsync(creado.IdAguinaldo);

            return MapToDTO(completo!);
        }

        // FIX #8: recibe el objeto empleado para no re-consultar la BD por cada uno
        public async Task<(List<AguinaldoDTO> registrados, List<string> errores)>
            CalcularAguinaldoMasivoAsync(CalcularAguinaldoMasivoDTO dto)
        {
            // FIX #5: GetAllAsync ya filtra ACTIVOS — no se necesita segundo .Where()
            var empleados = await _empleadosRepo.GetAllAsync();
            var registrados = new List<AguinaldoDTO>();
            var errores = new List<string>();

            foreach (var empleado in empleados)
            {
                try
                {
                    // FIX #7: validar FechaCorte una sola vez antes del loop
                    var fechaLimiteLegal = new DateTime(dto.Anio, MES_FIN_AGUINALDO, DIA_FIN_AGUINALDO);
                    if (dto.FechaCorte.HasValue && dto.FechaCorte.Value > fechaLimiteLegal)
                        throw new ArgumentException(
                            $"La fecha de corte no puede ser posterior al {fechaLimiteLegal:dd/MM/yyyy}");

                    var fechaFin = dto.FechaCorte ?? fechaLimiteLegal;
                    var fechaInicio = new DateTime(dto.Anio - 1, MES_INICIO_AGUINALDO, DIA_INICIO_AGUINALDO);
                    var fechaContratacion = empleado.FechaContratacion.ToDateTime(TimeOnly.MinValue);

                    if (fechaContratacion > fechaFin)
                        throw new InvalidOperationException(
                            $"Empleado contratado después del período de cálculo");

                    if (fechaContratacion > fechaInicio)
                        fechaInicio = fechaContratacion;

                    if (await _aguinaldoRepo.ExisteAguinaldoAsync(empleado.IdEmpleado, dto.Anio))
                        throw new InvalidOperationException(
                            $"Ya existe un aguinaldo para el año {dto.Anio}");

                    var diasTrabajados = CalcularDiasLaborados(fechaInicio, fechaFin);
                    var salarioPromedio = await CalcularSalarioPromedioAsync(
                        empleado.IdEmpleado, fechaInicio, fechaFin, empleado.SalarioBase);
                    var montoAguinaldo = CalcularMontoAguinaldo(salarioPromedio, diasTrabajados);

                    var entidad = new Aguinaldos
                    {
                        EmpleadoId = empleado.IdEmpleado,
                        Anio = dto.Anio,
                        FechaCalculo = DateTime.UtcNow,
                        DiasTrabajados = diasTrabajados,
                        SalarioPromedio = salarioPromedio,
                        MontoAguinaldo = montoAguinaldo,
                        Estado = "PENDIENTE",
                        FechaCreacion = DateTime.UtcNow
                    };

                    var creado = await _aguinaldoRepo.CreateAsync(entidad);
                    var completo = await _aguinaldoRepo.GetByIdAsync(creado.IdAguinaldo);
                    registrados.Add(MapToDTO(completo!));
                }
                catch (Exception ex)
                {
                    var nombre = $"{empleado.Nombre} {empleado.PrimerApellido}".Trim();
                    _logger.LogWarning(ex,
                        "No se pudo calcular aguinaldo para empleado {EmpleadoId} ({Nombre})",
                        empleado.IdEmpleado, nombre);
                    errores.Add($"{nombre}: {ex.Message}");
                }
            }

            return (registrados, errores);
        }

        private static int CalcularDiasLaborados(DateTime fechaInicio, DateTime fechaFin)
        {
            if (fechaFin < fechaInicio) return 0;
            var totalDias = (fechaFin.Date - fechaInicio.Date).Days + 1;
            return Math.Min(totalDias, DIAS_ANIO);
        }

        private async Task<decimal> CalcularSalarioPromedioAsync(
            int empleadoId,
            DateTime fechaInicio,
            DateTime fechaFin,
            decimal salarioBaseActual)
        {
            var nominas = (await _aguinaldoRepo
                .GetNominasPorPeriodoAsync(empleadoId, fechaInicio, fechaFin))
                .ToList();

            if (!nominas.Any())
                return salarioBaseActual;

            // Agrupar por año-mes y sumar quincenas del mismo mes
            // para obtener el salario mensual real antes de promediar
            var salariosPorMes = nominas
                .GroupBy(n => new { n.PeriodoNomina.Year, n.PeriodoNomina.Month })
                .Select(g => g.Sum(n => n.TotalBruto))
                .ToList();

            return salariosPorMes.Sum() / salariosPorMes.Count;
        }

        #endregion Cálculo y Registro

        #region Pago y Anulación

        public async Task<bool> AnularAguinaldoAsync(int idAguinaldo)
        {
            var aguinaldo = await _aguinaldoRepo.GetByIdAsync(idAguinaldo);

            if (aguinaldo == null)
                throw new KeyNotFoundException($"Aguinaldo {idAguinaldo} no encontrado");

            if (aguinaldo.Estado == "PAGADO")
                throw new InvalidOperationException("No se puede anular un aguinaldo ya pagado");

            return await _aguinaldoRepo.DeleteAsync(idAguinaldo);
        }

        public async Task<bool> PagarAguinaldoAsync(int idAguinaldo, DateTime fechaPago)
        {
            var aguinaldo = await _aguinaldoRepo.GetByIdAsync(idAguinaldo);

            if (aguinaldo == null)
                throw new KeyNotFoundException($"Aguinaldo {idAguinaldo} no encontrado");

            if (aguinaldo.Estado != "PENDIENTE")
                throw new InvalidOperationException(
                    $"El aguinaldo ya fue {aguinaldo.Estado?.ToLower()}");

            var maxFechaPago = new DateTime(fechaPago.Year, 12, 20);
            if (fechaPago > maxFechaPago)
                _logger.LogWarning(
                    "Pago de aguinaldo {Id} registrado después del 20 de diciembre (fecha límite legal CR)",
                    idAguinaldo);

            aguinaldo.FechaPago = fechaPago;
            aguinaldo.Estado = "PAGADO";
            aguinaldo.FechaModificacion = DateTime.UtcNow;

            return await _aguinaldoRepo.UpdateAsync(aguinaldo);
        }

        public async Task<(int exitosos, int fallidos, List<string> errores)>
            PagarAguinaldosMasivoAsync(List<int> idsAguinaldos, DateTime fechaPago)
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

        #endregion Pago y Anulación

        #region Métodos Privados

        private static decimal CalcularMontoAguinaldo(decimal salarioPromedio, int diasTrabajados)
        {
            var monto = salarioPromedio * diasTrabajados / DIAS_ANIO;
            return Math.Round(monto, 2, MidpointRounding.AwayFromZero);
        }

        private static AguinaldoDTO MapToDTO(Aguinaldos aguinaldo)
        {
            var nombreCompleto = aguinaldo.Empleado != null
                ? $"{aguinaldo.Empleado.Nombre} {aguinaldo.Empleado.PrimerApellido} {aguinaldo.Empleado.SegundoApellido}".Trim()
                : string.Empty;

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