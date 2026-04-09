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
        private readonly IAsistenciasRepository _asistenciaRepo;
        private readonly IAuditoriaService _auditoria;
        private readonly IEmpleadosRepository _empleadosRepo;
        private readonly ILogger<AguinaldoManager> _logger;
        private readonly INominaRepository _nominaRepo;

        public AguinaldoManager(
        IAguinaldoRepository aguinaldoRepo,
        IEmpleadosRepository empleadosRepo,
        IAuditoriaService auditoria,
        INominaRepository nominaRepo,
        IAsistenciasRepository asistenciaRepo,
        ILogger<AguinaldoManager> logger)
        {
            _aguinaldoRepo = aguinaldoRepo ?? throw new ArgumentNullException(nameof(aguinaldoRepo));
            _empleadosRepo = empleadosRepo ?? throw new ArgumentNullException(nameof(empleadosRepo));
            _nominaRepo = nominaRepo ?? throw new ArgumentNullException(nameof(nominaRepo));
            _asistenciaRepo = asistenciaRepo ?? throw new ArgumentNullException(nameof(asistenciaRepo));
            _auditoria = auditoria ?? throw new ArgumentNullException(nameof(auditoria));
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

            if (await _aguinaldoRepo.ExisteAguinaldoAsync(dto.EmpleadoId, dto.Anio))
                throw new InvalidOperationException(
                    $"Ya existe un aguinaldo registrado para {empleado.Nombre} en el año {dto.Anio}");

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

            var diasTrabajados = CalcularDiasLaborados(fechaInicio, fechaFin);
            var salarioPromedio = await CalcularSalarioPromedioAsync(dto.EmpleadoId, fechaInicio, fechaFin, empleado.SalarioBase);
            var montoAguinaldo = CalcularMontoAguinaldo(salarioPromedio, diasTrabajados);

            var entidad = new Aguinaldos
            {
                EmpleadoId = dto.EmpleadoId,
                Anio = dto.Anio,
                FechaCalculo = DateTime.Now,
                DiasTrabajados = diasTrabajados,
                SalarioPromedio = salarioPromedio,
                MontoAguinaldo = montoAguinaldo,
                Estado = "PENDIENTE",
                FechaCreacion = DateTime.Now
            };

            var creado = await _aguinaldoRepo.CreateAsync(entidad);
            var completo = await _aguinaldoRepo.GetByIdAsync(creado.IdAguinaldo);

            await _auditoria.RegistrarAsync(
                tablaAfectada: "aguinaldos",
                descripcion: $"Aguinaldo calculado para empleado ID {dto.EmpleadoId} " +
                             $"({empleado.Nombre} {empleado.PrimerApellido}), " +
                             $"año {dto.Anio}, monto: {montoAguinaldo:N2}, " +
                             $"días trabajados: {diasTrabajados}."
            );

            return MapToDTO(completo!);
        }

        public async Task<(List<AguinaldoDTO> registrados, List<string> errores)>
    CalcularAguinaldoMasivoAsync(CalcularAguinaldoMasivoDTO dto)
        {
            var empleados = await _empleadosRepo.GetAllAsync();
            var registrados = new List<AguinaldoDTO>();
            var errores = new List<string>();

            foreach (var empleado in empleados)
            {
                try
                {
                    var fechaLimiteLegal = new DateTime(dto.Anio, MES_FIN_AGUINALDO, DIA_FIN_AGUINALDO);
                    if (dto.FechaCorte.HasValue && dto.FechaCorte.Value > fechaLimiteLegal)
                        throw new ArgumentException(
                            $"La fecha de corte no puede ser posterior al {fechaLimiteLegal:dd/MM/yyyy}");

                    var fechaFin = dto.FechaCorte ?? fechaLimiteLegal;
                    var fechaInicio = new DateTime(dto.Anio - 1, MES_INICIO_AGUINALDO, DIA_INICIO_AGUINALDO);
                    var fechaContratacion = empleado.FechaContratacion.ToDateTime(TimeOnly.MinValue);

                    if (fechaContratacion > fechaFin)
                        throw new InvalidOperationException(
                            "Empleado contratado después del período de cálculo");

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
                        Estado = "PAGADO",
                        FechaCreacion = DateTime.UtcNow
                    };

                    var creado = await _aguinaldoRepo.CreateAsync(entidad);
                    var completo = await _aguinaldoRepo.GetByIdAsync(creado.IdAguinaldo);
                    registrados.Add(MapToDTO(completo!));

                    await _auditoria.RegistrarAsync(
                        tablaAfectada: "aguinaldos",
                        descripcion: $"Cálculo masivo: aguinaldo generado para empleado ID {empleado.IdEmpleado} " +
                                     $"({empleado.Nombre} {empleado.PrimerApellido}), " +
                                     $"año {dto.Anio}, monto: {montoAguinaldo:N2}."
                    );
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

            await _auditoria.RegistrarAsync(
                tablaAfectada: "aguinaldos",
                descripcion: $"Cálculo masivo año {dto.Anio} finalizado: " +
                             $"{registrados.Count} exitosos, {errores.Count} fallidos."
            );

            return (registrados, errores);
        }

        public async Task<(int registrados, List<string> errores)> CalcularAguinaldoMasivoV2Async()
        {
            var empleados = await _empleadosRepo.GetAllAsync();
            var registrados = 0;
            var errores = new List<string>();

            foreach (var empleado in empleados)
            {
                try
                {
                    await CalcularAguinaldoPresenteAnio(empleado.IdEmpleado);
                    registrados++;
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
            return (fechaFin.Date - fechaInicio.Date).Days + 1;
        }

        private async Task<decimal> CalcularSalarioPromedioAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin, decimal salarioBaseActual)
        {
            var nominas = (await _aguinaldoRepo
                .GetNominasPorPeriodoAsync(empleadoId, fechaInicio, fechaFin))
                .ToList();

            if (!nominas.Any())
                return salarioBaseActual;

            var salariosPorMes = nominas
                .GroupBy(n => new { n.PeriodoNomina.Year, n.PeriodoNomina.Month })
                .Select(g => g.Sum(n => n.TotalBruto))
                .ToList();

            if (salariosPorMes.Count == 0)
                return salarioBaseActual;

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

            var resultado = await _aguinaldoRepo.DeleteAsync(idAguinaldo);

            if (resultado)
                await _auditoria.RegistrarAsync(
                    tablaAfectada: "aguinaldos",
                    descripcion: $"Aguinaldo ID {idAguinaldo} anulado. " +
                                 $"Empleado ID {aguinaldo.EmpleadoId}, año {aguinaldo.Anio}."
                );

            return resultado;
        }

        public async Task<AguinaldoDTO> CalcularAguinaldoPresenteAnio(int idEmpleado)
        {
            var anioActual = DateTime.Now.Year;

            // VALIDACIONES

            var empleado = await _empleadosRepo.GetByIdAsync(idEmpleado);

            if (empleado == null)
                throw new ArgumentException($"Empleado {idEmpleado} no encontrado");

            if (empleado.Estado != "ACTIVO")
                throw new InvalidOperationException(
                    $"Empleado {empleado.Nombre} no está activo");

            // Obtener Salarios entre el 01 de dicimeb re del año anterior y el 30 de noviembre del annio actual
            var totalSalariosBrutos = await _nominaRepo.GetTotalSalariosBrutosAsync(
                idEmpleado,
                new DateTime(anioActual - 1, MES_INICIO_AGUINALDO, DIA_INICIO_AGUINALDO),
                new DateTime(anioActual, MES_FIN_AGUINALDO, DIA_FIN_AGUINALDO));

            // CALCULAR AGUINALDO TOTAL
            var montoAguinaldo = totalSalariosBrutos / 12;

            // DIAS TRABAJADOS ENTRE EL 1 DE DICIEMBRE DEL AÑO ANTERIOR Y EL 30 DE NOVIEMBRE DEL AÑO ACTUAL
            var diasTrabajados = await _asistenciaRepo.DiasTrabajadosPorPeriodoAsync(
                idEmpleado,
                new DateTime(anioActual - 1, MES_INICIO_AGUINALDO, DIA_INICIO_AGUINALDO),
                new DateTime(anioActual, MES_FIN_AGUINALDO, DIA_FIN_AGUINALDO));

            // SALARIO PROMEDIO
            var nominas = await _nominaRepo.ObtenerNominasPorEmpleadoAsync(idEmpleado);
            var totalSalarios = nominas.Sum(n => n.TotalBruto);
            var salarioPromedio = totalSalarios / 12;

            // RESULTADO FINAL

            var entidad = new Aguinaldos
            {
                EmpleadoId = idEmpleado,
                Anio = anioActual,
                FechaCalculo = DateTime.Now,
                DiasTrabajados = diasTrabajados,
                SalarioPromedio = salarioPromedio,
                MontoAguinaldo = montoAguinaldo,
                Estado = "PAGADO",
                FechaCreacion = DateTime.Now
            };

            var creado = await _aguinaldoRepo.CreateAsync(entidad);
            var completo = await _aguinaldoRepo.GetByIdAsync(creado.IdAguinaldo);

            await _auditoria.RegistrarAsync(
                tablaAfectada: "aguinaldos",
                descripcion: $"Aguinaldo calculado para empleado ID {idEmpleado} " +
                             $"({empleado.Nombre} {empleado.PrimerApellido}), " +
                             $"año {anioActual}, monto: {montoAguinaldo:N2}, " +
                             $"días trabajados: {diasTrabajados}."
            );

            return MapToDTO(entidad);
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

            var resultado = await _aguinaldoRepo.UpdateAsync(aguinaldo);

            if (resultado)
                await _auditoria.RegistrarAsync(
                    tablaAfectada: "aguinaldos",
                    descripcion: $"Aguinaldo ID {idAguinaldo} marcado como PAGADO. " +
                                 $"Empleado ID {aguinaldo.EmpleadoId}, " +
                                 $"monto: {aguinaldo.MontoAguinaldo:N2}, " +
                                 $"fecha pago: {fechaPago:dd/MM/yyyy}."
                );

            return resultado;
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

            await _auditoria.RegistrarAsync(
               tablaAfectada: "aguinaldos",
               descripcion: $"Pago masivo completado: {exitosos} exitosos, {fallidos} fallidos. " +
                            $"Fecha de pago: {fechaPago:dd/MM/yyyy}."
           );

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
                Anio = aguinaldo.Anio,
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