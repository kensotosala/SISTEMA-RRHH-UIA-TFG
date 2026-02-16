using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Services;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;

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

        public NominaManager(
            INominaRepository nominaRepo,
            IEmpleadosRepository empleadosRepo,
            IHorasExtrasRepository horasExtraRepo,
            IIncapacidadesRepository incapacidadesRepo,
            IPermisosRepository permisosRepo)
        {
            _nominaRepo = nominaRepo;
            _empleadosRepo = empleadosRepo;
            _horasExtraRepo = horasExtraRepo;
            _incapacidadesRepo = incapacidadesRepo;
            _permisosRepo = permisosRepo;
            _calculador = new CalculadorNominaCostaRica();
        }

        public async Task<List<DetalleNominaDTO>> GenerarNominaQuincenalAsync(GenerarNominaQuincenalDTO dto)
        {
            if (dto.Quincena != 1 && dto.Quincena != 2)
                throw new ArgumentException("La quincena debe ser 1 o 2");

            if (dto.Mes < 1 || dto.Mes > 12)
                throw new ArgumentException("El mes debe estar entre 1 y 12");

            List<Empleados> empleados;
            if (dto.EmpleadosIds != null && dto.EmpleadosIds.Any())
            {
                empleados = new List<Empleados>();
                foreach (var id in dto.EmpleadosIds)
                {
                    var emp = await _empleadosRepo.GetByIdAsync(id);
                    if (emp != null && emp.Estado == "ACTIVO")
                        empleados.Add(emp);
                }
            }
            else
            {
                var todosEmpleados = await _empleadosRepo.GetAllAsync();
                empleados = todosEmpleados.Where(e => e.Estado == "ACTIVO").ToList();
            }

            var detalles = new List<DetalleNominaDTO>();

            foreach (var empleado in empleados)
            {
                var existe = await _nominaRepo.ExisteNominaQuincenaAsync(
                    empleado.IdEmpleado, dto.Quincena, dto.Mes, dto.Anio);

                if (existe)
                    continue;

                var horasExtrasEnumerable = await _horasExtraRepo.GetByEmpleadoAsync(empleado.IdEmpleado);
                var horasExtras = horasExtrasEnumerable.ToList();

                var incapacidades = await _incapacidadesRepo.ListarIncapacidadesAsync();
                var permisos = await _permisosRepo.GetAllPermisosAsync();

                var incapacidadesEmpleado = incapacidades
                    .Where(i => i.EmpleadoId == empleado.IdEmpleado).ToList();
                var permisosEmpleado = permisos
                    .Where(p => p.EmpleadoId == empleado.IdEmpleado).ToList();

                var detalle = _calculador.CalcularNominaQuincenal(
                    empleado,
                    dto.Quincena,
                    dto.Mes,
                    dto.Anio,
                    horasExtras,
                    incapacidadesEmpleado,
                    permisosEmpleado
                );

                var nomina = new Nominas
                {
                    EmpleadoId = empleado.IdEmpleado,
                    PeriodoNomina = new DateTime(dto.Anio, dto.Mes, dto.Quincena == 1 ? 1 : 16),
                    FechaPago = dto.FechaPago,
                    SalarioBase = detalle.SalarioBaseQuincenal,
                    HorasExtras = detalle.TotalHorasExtra > 0
                        ? (decimal?)(detalle.HorasExtraDiurnas + detalle.HorasExtraNocturnas + detalle.HorasExtraFeriados) / (empleado.SalarioBase / 240)
                        : null,
                    MontoHorasExtra = detalle.TotalHorasExtra > 0 ? (decimal?)detalle.TotalHorasExtra : null,
                    Bonificaciones = detalle.Bonificaciones > 0 ? (decimal?)detalle.Bonificaciones : null,
                    Deducciones = detalle.TotalDeducciones,
                    TotalBruto = detalle.TotalBruto,
                    TotalNeto = detalle.TotalNeto,
                    Estado = "PENDIENTE"
                };

                await _nominaRepo.CrearNominaAsync(nomina);
                detalles.Add(detalle);
            }

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

        public async Task<List<NominaDTO>> ObtenerNominasQuincenaAsync(int quincena, int mes, int anio)
        {
            var nominas = await _nominaRepo.ObtenerNominasQuincenaAsync(quincena, mes, anio);
            return nominas.Select(MapToDTO).ToList();
        }

        public async Task<bool> AprobarNominaAsync(int nominaId)
        {
            var nomina = await _nominaRepo.ObtenerNominaPorIdAsync(nominaId);
            if (nomina == null) return false;

            nomina.Estado = "PAGADA";
            await _nominaRepo.ActualizarNominaAsync(nomina);
            return true;
        }

        public async Task<bool> PagarNominaAsync(int nominaId)
        {
            var nomina = await _nominaRepo.ObtenerNominaPorIdAsync(nominaId);
            if (nomina == null) return false;

            nomina.Estado = "PAGADA";
            nomina.FechaPago = DateTime.Now;
            await _nominaRepo.ActualizarNominaAsync(nomina);
            return true;
        }

        public async Task<bool> AnularNominaAsync(int nominaId)
        {
            var nomina = await _nominaRepo.ObtenerNominaPorIdAsync(nominaId);
            if (nomina == null || nomina.Estado == "PAGADA") return false;

            nomina.Estado = "ANULADA";
            await _nominaRepo.ActualizarNominaAsync(nomina);
            return true;
        }

        public async Task<ResumenNominaQuincenalDTO> ObtenerResumenQuincenaAsync(int quincena, int mes, int anio)
        {
            var nominas = await _nominaRepo.ObtenerNominasQuincenaAsync(quincena, mes, anio);

            return new ResumenNominaQuincenalDTO
            {
                Quincena = quincena,
                Mes = mes,
                Anio = anio,
                TotalEmpleados = nominas.Count,
                TotalBruto = nominas.Sum(n => n.TotalBruto),
                TotalCCSS = nominas.Sum(n => n.TotalBruto * 0.1667m),
                TotalDeducciones = nominas.Sum(n => n.Deducciones ?? 0),
                TotalNeto = nominas.Sum(n => n.TotalNeto),
                FechaGeneracion = DateTime.Now,
                Estado = nominas.Any() ? nominas.First().Estado ?? "PENDIENTE" : "PENDIENTE"
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
                    Cedula = nomina.Empleado.CodigoEmpleado,
                    NombreCompleto = $"{nomina.Empleado.Nombre} {nomina.Empleado.PrimerApellido}",
                    SalarioReportado = nomina.TotalBruto,
                    CuotaObrera = nomina.TotalBruto * 0.1667m,
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
                var detalle = new DetalleImpuestoEmpleadoDTO
                {
                    Cedula = nomina.Empleado.CodigoEmpleado,
                    NombreCompleto = $"{nomina.Empleado.Nombre} {nomina.Empleado.PrimerApellido}",
                    SalarioBruto = nomina.TotalBruto,
                    DeduccionCCSS = nomina.TotalBruto * 0.1667m,
                    BaseImponible = nomina.TotalBruto * (1 - 0.1667m),
                    ImpuestoRetenido = (nomina.Deducciones ?? 0) - (nomina.TotalBruto * 0.1667m)
                };
                declaracion.Empleados.Add(detalle);
            }

            declaracion.TotalRetenido = declaracion.Empleados.Sum(e => e.ImpuestoRetenido);

            return declaracion;
        }

        private NominaDTO MapToDTO(Nominas nomina)
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
                NombreEmpleado = $"{nomina.Empleado?.Nombre} {nomina.Empleado?.PrimerApellido}",
                CodigoEmpleado = nomina.Empleado?.CodigoEmpleado,
                Puesto = nomina.Empleado?.Puesto?.NombrePuesto,
                Departamento = nomina.Empleado?.Departamento?.NombreDepartamento
            };
        }
    }
}