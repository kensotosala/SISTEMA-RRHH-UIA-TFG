using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Services;
using BusinessLogicLayer.Shared;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using System.Numerics;

namespace BusinessLogicLayer.Managers
{
    public class NominaManager : INominaManager
    {
        private readonly INominaRepository _nominaRepo;
        private readonly IEmpleadosRepository _empleadosRepo;
        private readonly IHorasExtrasRepository _horasExtraRepo;
        private readonly IIncapacidadesRepository _incapacidadesRepo;
        private readonly IPermisosRepository _permisosRepo;
        private readonly CalculadorNominaCostaRica _calculador;
        private readonly IAuditoriaService _auditoria;

        private const decimal TasaCCSSObrera = 0.1067m;

        public NominaManager(
            INominaRepository nominaRepo,
            IEmpleadosRepository empleadosRepo,
            IHorasExtrasRepository horasExtraRepo,
            IIncapacidadesRepository incapacidadesRepo,
            IPermisosRepository permisosRepo,
            IAuditoriaService auditoria)
        {
            _nominaRepo = nominaRepo;
            _empleadosRepo = empleadosRepo;
            _horasExtraRepo = horasExtraRepo;
            _incapacidadesRepo = incapacidadesRepo;
            _permisosRepo = permisosRepo;
            _calculador = new CalculadorNominaCostaRica();
            _auditoria = auditoria;
        }

        public async Task<List<DetalleNominaDTO>> GenerarNominaQuincenalAsync(
            GenerarNominaQuincenalDTO dto)
        {
            if (dto.Quincena != 1 && dto.Quincena != 2)
                throw new ArgumentException("La quincena debe ser 1 o 2");

            if (dto.Mes < 1 || dto.Mes > 12)
                throw new ArgumentException("El mes debe estar entre 1 y 12");

            if (dto.Anio < 2000 || dto.Anio > DateTime.Now.Year + 1)
                throw new ArgumentException("El año no es válido");

            List<Empleados> empleados;

            if (dto.EmpleadosIds != null && dto.EmpleadosIds.Any())
            {
                empleados = await _empleadosRepo.GetByIdsAsync(dto.EmpleadosIds);
                empleados = empleados.Where(e => e.Estado == "ACTIVO").ToList();
            }
            else
            {
                var todos = await _empleadosRepo.GetAllAsync();
                empleados = todos.Where(e => e.Estado == "ACTIVO").ToList();
            }

            var todasIncapacidades = await _incapacidadesRepo.ListarIncapacidadesAsync();
            var todosPermisos = await _permisosRepo.GetAllPermisosAsync();
            var detalles = new List<DetalleNominaDTO>();
            var nominasGeneradas = 0;

            foreach (var empleado in empleados)
            {
                var existe = await _nominaRepo.ExisteNominaQuincenaAsync(
                    empleado.IdEmpleado, dto.Quincena, dto.Mes, dto.Anio);

                if (existe) continue;

                var horasExtras = (await _horasExtraRepo
                    .GetByEmpleadoAsync(empleado.IdEmpleado)).ToList();

                var incapacidadesEmpleado = todasIncapacidades
                    .Where(i => i.EmpleadoId == empleado.IdEmpleado).ToList();
                var permisosEmpleado = todosPermisos
                    .Where(p => p.EmpleadoId == empleado.IdEmpleado).ToList();

                var detalle = _calculador.CalcularNominaQuincenal(
                    empleado, dto.Quincena, dto.Mes, dto.Anio,
                    horasExtras, incapacidadesEmpleado, permisosEmpleado);

                decimal? cantidadHorasExtra = detalle.TotalHorasExtra > 0
                    ? (decimal?)(detalle.HorasExtraDiurnas +
                                 detalle.HorasExtraNocturnas +
                                 detalle.HorasExtraFeriados)
                    : null;

                var nomina = new Nominas
                {
                    EmpleadoId = empleado.IdEmpleado,
                    PeriodoNomina = new DateTime(dto.Anio, dto.Mes, dto.Quincena == 1 ? 1 : 16),
                    FechaPago = dto.FechaPago,
                    SalarioBase = detalle.SalarioBaseQuincenal,
                    HorasExtras = cantidadHorasExtra,
                    MontoHorasExtra = detalle.TotalHorasExtra > 0 ? (decimal?)detalle.TotalHorasExtra : null,
                    Bonificaciones = detalle.Bonificaciones > 0 ? (decimal?)detalle.Bonificaciones : null,
                    Deducciones = detalle.TotalDeducciones,
                    TotalBruto = detalle.TotalBruto,
                    TotalNeto = detalle.TotalNeto,
                    Estado = "PENDIENTE"
                };

                await _nominaRepo.CrearNominaAsync(nomina);
                detalles.Add(detalle);
                nominasGeneradas++;
            }

            if (nominasGeneradas > 0)
                await _auditoria.RegistrarAsync(
                    tablaAfectada: "nominas",
                    descripcion: $"Generación de nómina quincenal completada: " +
                                   $"quincena {dto.Quincena}, mes {dto.Mes}/{dto.Anio}. " +
                                   $"{nominasGeneradas} nómina(s) generada(s)."
                );

            return detalles;
        }

        public async Task<NominaDTO?> ObtenerNominaPorIdAsync(int id)
        {
            var nomina = await _nominaRepo.ObtenerNominaPorIdAsync(id);
            return nomina == null ? null : MapToDTO(nomina);
        }

        public async Task<List<NominaDTO>> ListarNominasAsync()
        {
            var nominas = await _nominaRepo.ListarNominasAsync();
            return nominas.Select(MapToDTO).ToList();
        }

        public async Task<List<NominaDTO>> ObtenerNominasPorEmpleadoAsync(int empleadoId)
        {
            var nominas = await _nominaRepo.ObtenerNominasPorEmpleadoAsync(empleadoId);
            return nominas.Select(MapToDTO).ToList();
        }

        public async Task<List<NominaDTO>> ObtenerNominasQuincenaAsync(
            int quincena, int mes, int anio)
        {
            var nominas = await _nominaRepo.ObtenerNominasQuincenaAsync(quincena, mes, anio);
            return nominas.Select(MapToDTO).ToList();
        }

        public async Task<AprobarNominaResultado> AprobarNominaAsync(int nominaId)
        {
            var nomina = await _nominaRepo.ObtenerNominaPorIdAsync(nominaId);
            if (nomina == null) return AprobarNominaResultado.NoEncontrada;

            if (nomina.Estado != "PENDIENTE")
                return AprobarNominaResultado.EstadoInvalido;

            nomina.Estado = "APROBADA";
            await _nominaRepo.ActualizarNominaAsync(nomina);

            await _auditoria.RegistrarAsync(
                tablaAfectada: "nominas",
                descripcion: $"Nómina ID {nominaId} aprobada. " +
                               $"Empleado ID {nomina.EmpleadoId}, " +
                               $"período: {nomina.PeriodoNomina:MM/yyyy}, " +
                               $"total neto: {nomina.TotalNeto:N2}."
            );

            return AprobarNominaResultado.Aprobada;
        }

        public async Task<PagarNominaResultado> PagarNominaAsync(int nominaId)
        {
            var nomina = await _nominaRepo.ObtenerNominaPorIdAsync(nominaId);
            if (nomina == null) return PagarNominaResultado.NoEncontrada;

            if (nomina.Estado != "APROBADA")
                return PagarNominaResultado.NoAprobada;

            nomina.Estado = "PAGADA";
            nomina.FechaPago = DateTime.Now;
            await _nominaRepo.ActualizarNominaAsync(nomina);

            await _auditoria.RegistrarAsync(
                tablaAfectada: "nominas",
                descripcion: $"Nómina ID {nominaId} marcada como PAGADA. " +
                               $"Empleado ID {nomina.EmpleadoId}, " +
                               $"período: {nomina.PeriodoNomina:MM/yyyy}, " +
                               $"total neto: {nomina.TotalNeto:N2}, " +
                               $"fecha pago: {nomina.FechaPago:dd/MM/yyyy}."
            );

            return PagarNominaResultado.Pagada;
        }

        public async Task<AnularNominaResultado> AnularNominaAsync(int nominaId)
        {
            var nomina = await _nominaRepo.ObtenerNominaPorIdAsync(nominaId);
            if (nomina == null) return AnularNominaResultado.NoEncontrada;

            if (nomina.Estado == "PAGADA")
                return AnularNominaResultado.NoPuedeAnularse;

            var estadoAnterior = nomina.Estado;

            nomina.Estado = "ANULADA";
            await _nominaRepo.ActualizarNominaAsync(nomina);

            await _auditoria.RegistrarAsync(
                tablaAfectada: "nominas",
                descripcion: $"Nómina ID {nominaId} anulada. " +
                               $"Empleado ID {nomina.EmpleadoId}, " +
                               $"período: {nomina.PeriodoNomina:MM/yyyy}, " +
                               $"estado anterior: '{estadoAnterior}'."
            );

            return AnularNominaResultado.Anulada;
        }

        public async Task<ResumenNominaQuincenalDTO> ObtenerResumenQuincenaAsync(
            int quincena, int mes, int anio)
        {
            var nominas = await _nominaRepo.ObtenerNominasQuincenaAsync(quincena, mes, anio);

            string estadoResumen = "SIN_DATOS";
            if (nominas.Any())
            {
                var prioridad = new[] { "PENDIENTE", "APROBADA", "PAGADA", "ANULADA" };
                estadoResumen = prioridad.FirstOrDefault(
                    p => nominas.Any(n => n.Estado == p)) ?? "PENDIENTE";
            }

            decimal totalCCSS = nominas.Sum(n => n.TotalBruto * TasaCCSSObrera);
            decimal totalDeducciones = nominas.Sum(n => n.Deducciones ?? 0);
            decimal totalImpuestoRenta = Math.Max(0, totalDeducciones - totalCCSS);

            return new ResumenNominaQuincenalDTO
            {
                Quincena = quincena,
                Mes = mes,
                Anio = anio,
                TotalEmpleados = nominas.Count,
                TotalBruto = nominas.Sum(n => n.TotalBruto),
                TotalCCSS = totalCCSS,
                TotalImpuestoRenta = totalImpuestoRenta,
                TotalDeducciones = totalDeducciones,
                TotalNeto = nominas.Sum(n => n.TotalNeto),
                FechaGeneracion = DateTime.Now,
                Estado = estadoResumen
            };
        }

        public async Task<PlanillaCCSSDTO> GenerarPlanillaCCSSAsync(int mes, int anio)
        {
            var nominas = await _nominaRepo.ObtenerNominasMesAsync(mes, anio);

            var planilla = new PlanillaCCSSDTO
            {
                Mes = mes,
                Anio = anio,
                Empleados = new List<DetalleCCSSEmpleadoDTO>()
            };

            foreach (var nomina in nominas)
            {
                var detalle = new DetalleCCSSEmpleadoDTO
                {
                    Cedula = nomina.Empleado?.CodigoEmpleado ?? string.Empty,
                    NombreCompleto = FormatearNombre(nomina.Empleado),
                    SalarioReportado = nomina.TotalBruto,
                    CuotaObrera = nomina.TotalBruto * TasaCCSSObrera,
                    CuotaPatronal = nomina.TotalBruto * 0.2658m
                };
                planilla.Empleados.Add(detalle);
            }

            planilla.TotalSalariosReportados = planilla.Empleados.Sum(e => e.SalarioReportado);
            planilla.TotalCuotaObrera = planilla.Empleados.Sum(e => e.CuotaObrera);
            planilla.TotalCuotaPatronal = planilla.Empleados.Sum(e => e.CuotaPatronal);

            return planilla;
        }

        public async Task<DeclaracionD151DTO> GenerarDeclaracionD151Async(int mes, int anio)
        {
            var nominas = await _nominaRepo.ObtenerNominasMesAsync(mes, anio);

            var declaracion = new DeclaracionD151DTO
            {
                Mes = mes,
                Anio = anio,
                Empleados = new List<DetalleImpuestoEmpleadoDTO>()
            };

            foreach (var nomina in nominas)
            {
                decimal cuotaCCSS = nomina.TotalBruto * TasaCCSSObrera;
                decimal baseImponible = nomina.TotalBruto - cuotaCCSS;

                decimal impuestoRetenido = Math.Max(
                    0, (nomina.Deducciones ?? 0) - cuotaCCSS);

                var detalle = new DetalleImpuestoEmpleadoDTO
                {
                    Cedula = nomina.Empleado?.CodigoEmpleado ?? string.Empty,
                    NombreCompleto = FormatearNombre(nomina.Empleado),
                    SalarioBruto = nomina.TotalBruto,
                    DeduccionCCSS = cuotaCCSS,
                    BaseImponible = baseImponible,
                    ImpuestoRetenido = impuestoRetenido
                };
                declaracion.Empleados.Add(detalle);
            }

            declaracion.TotalRetenido = declaracion.Empleados.Sum(e => e.ImpuestoRetenido);

            return declaracion;
        }

        private static string FormatearNombre(Empleados? empleado)
        {
            if (empleado == null) return string.Empty;
            return $"{empleado.Nombre} {empleado.PrimerApellido}".Trim();
        }

        private static NominaDTO MapToDTO(Nominas nomina)
        {
            return new NominaDTO
            {
                IdNomina = nomina.IdNomina,
                EmpleadoId = nomina.EmpleadoId,
                PeriodoNomina = nomina.PeriodoNomina,
                FechaPago = nomina.FechaPago,
                SalarioBase = nomina.SalarioBase,
                HorasExtras = nomina.HorasExtras,
                MontoHorasExtra = nomina.MontoHorasExtra,
                Bonificaciones = nomina.Bonificaciones,
                Deducciones = nomina.Deducciones,
                TotalBruto = nomina.TotalBruto,
                TotalNeto = nomina.TotalNeto,
                Estado = nomina.Estado,
                FechaCreacion = nomina.FechaCreacion,
                FechaActualizacion = nomina.FechaActualizacion,
                NombreEmpleado = FormatearNombre(nomina.Empleado),
                CodigoEmpleado = nomina.Empleado?.CodigoEmpleado,
                Puesto = nomina.Empleado?.Puesto?.NombrePuesto,
                Departamento = nomina.Empleado?.Departamento?.NombreDepartamento
            };
        }

        public async Task<NominaParcialDTO> CalcularNominaParcialHoyAsync()
        {
            var hoy = DateTime.Today;
            var mes = hoy.Month;
            var anio = hoy.Year;

            // Siempre del 16 hasta hoy — fijo, sin importar en qué día estamos
            var inicioQuincena = new DateTime(anio, mes, 16);
            var finQuincena = new DateTime(anio, mes, DateTime.DaysInMonth(anio, mes));

            int diasTotales = (finQuincena - inicioQuincena).Days + 1;
            int diasTranscurridos = (hoy - inicioQuincena).Days + 1;

            // Factor proporcional: ej. si hoy es día 25, son 10 días de 16 → 10/16
            decimal factor = (decimal)diasTranscurridos / diasTotales;

            // Empleados activos
            var todos = await _empleadosRepo.GetAllAsync();
            var empleados = todos.Where(e => e.Estado == "ACTIVO").ToList();

            var todasIncapacidades = await _incapacidadesRepo.ListarIncapacidadesAsync();
            var todosPermisos = await _permisosRepo.GetAllPermisosAsync();

            var detallesEmpleados = new List<DetalleNominaParcialEmpleadoDTO>();

            foreach (var empleado in empleados)
            {
                var horasExtras = (await _horasExtraRepo
                    .GetByEmpleadoAsync(empleado.IdEmpleado)).ToList();

                var incapacidades = todasIncapacidades
                    .Where(i => i.EmpleadoId == empleado.IdEmpleado).ToList();

                var permisos = todosPermisos
                    .Where(p => p.EmpleadoId == empleado.IdEmpleado).ToList();

                // Calcula la quincena 2 completa y luego aplica el factor
                var detalleCompleto = _calculador.CalcularNominaQuincenal(
                    empleado, quincena: 2, mes, anio,
                    horasExtras, incapacidades, permisos);

                decimal salarioProporcional = detalleCompleto.SalarioBaseQuincenal * factor;
                decimal horasExtraProporcionadas = detalleCompleto.TotalHorasExtra * factor;
                decimal bonificacionesProporcionales = detalleCompleto.Bonificaciones * factor;
                decimal totalBruto = salarioProporcional + horasExtraProporcionadas + bonificacionesProporcionales;
                decimal totalDeducciones = detalleCompleto.TotalDeducciones * factor;
                decimal totalNeto = totalBruto - totalDeducciones;

                // Guardar en BD como si fuera una nómina normal de la quincena 2
                var nomina = new Nominas
                {
                    EmpleadoId = empleado.IdEmpleado,
                    PeriodoNomina = inicioQuincena,               // 16 del mes actual
                    FechaPago = hoy,                          // hoy como fecha de pago
                    SalarioBase = Math.Round(salarioProporcional, 2),
                    HorasExtras = horasExtraProporcionadas > 0
                                        ? (decimal?)Math.Round(horasExtraProporcionadas, 2) : null,
                    MontoHorasExtra = horasExtraProporcionadas > 0
                                        ? (decimal?)Math.Round(horasExtraProporcionadas, 2) : null,
                    Bonificaciones = bonificacionesProporcionales > 0
                                        ? (decimal?)Math.Round(bonificacionesProporcionales, 2) : null,
                    Deducciones = Math.Round(totalDeducciones, 2),
                    TotalBruto = Math.Round(totalBruto, 2),
                    TotalNeto = Math.Round(totalNeto, 2),
                    Estado = "PENDIENTE"   // igual que cualquier nómina normal
                };

                await _nominaRepo.CrearNominaAsync(nomina);

                // DTO de respuesta
                detallesEmpleados.Add(new DetalleNominaParcialEmpleadoDTO
                {
                    EmpleadoId = empleado.IdEmpleado,
                    CodigoEmpleado = empleado.CodigoEmpleado ?? string.Empty,
                    NombreCompleto = $"{empleado.Nombre} {empleado.PrimerApellido}".Trim(),
                    Departamento = empleado.Departamento?.NombreDepartamento ?? "Sin departamento",
                    Puesto = empleado.Puesto?.NombrePuesto ?? "Sin puesto",
                    SalarioBaseQuincenal = detalleCompleto.SalarioBaseQuincenal,
                    SalarioProporcional = Math.Round(salarioProporcional, 2),
                    TotalHorasExtra = Math.Round(horasExtraProporcionadas, 2),
                    Bonificaciones = Math.Round(bonificacionesProporcionales, 2),
                    TotalBruto = Math.Round(totalBruto, 2),
                    DeduccionesCCSS = new DeduccionesCCSSDTO
                    {
                        SEM = Math.Round(detalleCompleto.DeduccionesCCSS.SEM * factor, 2),
                        IVM = Math.Round(detalleCompleto.DeduccionesCCSS.IVM * factor, 2),
                        BancoPopular = Math.Round(detalleCompleto.DeduccionesCCSS.BancoPopular * factor, 2),
                        ANP = Math.Round(detalleCompleto.DeduccionesCCSS.ANP * factor, 2),
                        Total = Math.Round(detalleCompleto.DeduccionesCCSS.Total * factor, 2)
                    },
                    ImpuestoRenta = new ImpuestoRentaDTO
                    {
                        BaseImponible = Math.Round(detalleCompleto.ImpuestoRenta.BaseImponible * factor, 2),
                        ProyeccionMensual = detalleCompleto.ImpuestoRenta.ProyeccionMensual,
                        ImpuestoMensual = detalleCompleto.ImpuestoRenta.ImpuestoMensual,
                        ImpuestoQuincenal = Math.Round(detalleCompleto.ImpuestoRenta.ImpuestoQuincenal * factor, 2),
                        TramoAplicado = detalleCompleto.ImpuestoRenta.TramoAplicado
                    },
                    TotalDeducciones = Math.Round(totalDeducciones, 2),
                    TotalNeto = Math.Round(totalNeto, 2),
                    PorcentajeCompletado = Math.Round(factor * 100, 1)
                });
            }

            await _auditoria.RegistrarAsync(
                tablaAfectada: "nominas",
                descripcion: $"Nómina demostrativa del 16/{mes}/{anio} al {hoy:dd/MM/yyyy}. " +
                             $"{detallesEmpleados.Count} empleado(s) procesado(s)."
            );

            return new NominaParcialDTO
            {
                Quincena = 2,
                InicioQuincena = inicioQuincena,
                FechaCalculo = hoy,
                DiasTranscurridos = diasTranscurridos,
                DiasTotalesQuincena = diasTotales,
                Empleados = detallesEmpleados,
                TotalBruto = detallesEmpleados.Sum(e => e.TotalBruto),
                TotalDeducciones = detallesEmpleados.Sum(e => e.TotalDeducciones),
                TotalNeto = detallesEmpleados.Sum(e => e.TotalNeto)
            };
        }
    }
}