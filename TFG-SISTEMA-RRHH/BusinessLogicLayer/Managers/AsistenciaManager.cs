using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Shared;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Shared;

namespace BusinessLogicLayer.Managers
{
    public class AsistenciaManager : IAsistenciaManager
    {
        private readonly IAsistenciasRepository _asistenciasRepo;
        private readonly IAuditoriaService _auditoria;
        private readonly IEmpleadosRepository _empleadosRepo;
        private readonly IHorasExtrasManager _horasExtrasManager;

        // Horario Laboral: 8:00 AM a 5:00 PM
        private readonly TimeSpan HoraEntradaNormal = new(8, 0, 0);

        private readonly TimeSpan HoraSalidaNormal = new(17, 0, 0);

        public AsistenciaManager(
            IAsistenciasRepository asistenciasRepo,
            IEmpleadosRepository empleadosRepo,
            IHorasExtrasManager horasExtrasManager,
            IAuditoriaService auditoria)
        {
            _asistenciasRepo = asistenciasRepo;
            _empleadosRepo = empleadosRepo;
            _horasExtrasManager = horasExtrasManager;
            _auditoria = auditoria;
        }

        public async Task<MarcarAsistenciaResponse> MarcarAsistenciaAsync(int empleadoId)
        {
            // BUSCAR EMPLEADO
            var empleado = await _empleadosRepo.GetByIdAsync(empleadoId);
            if (empleado == null)
                throw new BusinessException("Empleado no encontrado", "EMPLEADO_NO_ENCONTRADO");

            // FECHAS Y TIEMPO
            var hoy = DateTime.Now.Date;
            var ahora = DateTime.Now;

            // BUSCAR REGISTRO DE ASISTENCIA PARA HOY
            var registro = await _asistenciasRepo.GetByEmpleadoYFechaAsync(empleadoId, hoy);

            // ENTRADA
            if (registro == null)
            {
                var nuevoRegistro = new Asistencias
                {
                    EmpleadoId = empleadoId,
                    FechaRegistro = hoy,
                    HoraEntrada = ahora,
                    Estado = DeterminarEstado(ahora, null),
                    FechaCreacion = ahora
                };

                await _asistenciasRepo.CreateAsync(nuevoRegistro);

                await _auditoria.RegistrarAsync(
                    tablaAfectada: "asistencias",
                    descripcion: $"Entrada registrada para empleado ID {empleadoId} " +
                                 $"({empleado.Nombre} {empleado.PrimerApellido}), " +
                                 $"hora: {ahora:HH:mm:ss}, estado: {nuevoRegistro.Estado}."
                );

                return BuildResponse("ENTRADA", ahora, ahora, null, nuevoRegistro.Estado,
                                     "Entrada registrada correctamente", true);
            }

            // YA TIENE ENTRADA Y SALIDA
            if (registro.HoraSalida != null)
            {
                return BuildResponse("NINGUNA", ahora, registro.HoraEntrada, registro.HoraSalida,
                                     registro.Estado, "Ya has registrado entrada y salida para hoy", false);
            }

            // SALIDA
            var horaActual = ahora.TimeOfDay;

            // Dentro del horario normal
            if (horaActual <= HoraSalidaNormal)
            {
                registro.HoraSalida = ahora;
                registro.FechaModificacion = ahora;
                await _asistenciasRepo.UpdateAsync(registro);

                await _auditoria.RegistrarAsync(
                    tablaAfectada: "asistencias",
                    descripcion: $"Salida registrada para empleado ID {empleadoId} " +
                                 $"({empleado.Nombre} {empleado.PrimerApellido}), " +
                                 $"hora: {ahora:HH:mm:ss}."
                );

                return BuildResponse("SALIDA", ahora, registro.HoraEntrada, ahora,
                                     registro.Estado, "Salida registrada correctamente", true);
            }

            // Fuera del horario normal — verificar hora extra aprobada
            var horaExtra = await _horasExtrasManager.ObtenerHoraExtraActivaAsync(empleadoId, ahora);

            if (horaExtra == null)
                throw new BusinessException(
                    "No puedes registrar tu salida fuera del horario laboral sin una hora extra aprobada",
                    "HORA_EXTRA_NO_APROBADA");

            if (ahora > horaExtra.FechaFin)
                throw new BusinessException(
                    "Has excedido el límite de horas extras aprobadas",
                    "HORA_EXTRA_EXCEDIDA");

            // Se guarda la hora real de salida (no el límite del horario normal)
            registro.HoraSalida = ahora;
            registro.FechaModificacion = ahora;
            await _asistenciasRepo.UpdateAsync(registro);

            await _auditoria.RegistrarAsync(
                tablaAfectada: "asistencias",
                descripcion: $"Salida con horas extras registrada para empleado ID {empleadoId} " +
                             $"({empleado.Nombre} {empleado.PrimerApellido}), " +
                             $"hora: {ahora:HH:mm:ss}, hora extra ID: {horaExtra.IdHoraExtra}."
            );

            return BuildResponse("SALIDA", ahora, registro.HoraEntrada, ahora,
                                 registro.Estado, "Salida registrada con horas extras aprobadas", true);
        }

        public async Task<MarcarAsistenciaResponse> MarcarAsistenciaAsyncV2(int empleadoId)
        {
            var empleado = await _empleadosRepo.GetByIdAsync(empleadoId);
            if (empleado == null)
                throw new BusinessException("Empleado no encontrado", "EMPLEADO_NO_ENCONTRADO");

            var ahora = DateTime.Now;
            var hoy = ahora.Date;

            // Un solo registro por día
            var asistencia = await _asistenciasRepo.GetByEmpleadoYFechaAsync(empleadoId, hoy);

            // 1. Sin registro → registrar entrada normal
            if (asistencia == null)
                return await RegistrarEntrada(empleado, TipoAsistencia.NORMAL, ahora);

            // 2. Con entrada pero sin salida → registrar salida
            if (asistencia.HoraSalida == null)
                return await RegistrarSalida(asistencia, ahora);

            // 3. Ya tiene salida → verificar si tiene hora extra aprobada
            var horaExtra = await _horasExtrasManager
                .ObtenerHoraExtraActivaAsync(empleadoId, ahora);

            if (horaExtra == null)
                return BuildResponse("NINGUNA", ahora,
                    asistencia.HoraEntrada,
                    asistencia.HoraSalida,
                    asistencia.Estado,
                    "Ya completaste tu jornada", false);

            if (ahora > horaExtra.FechaFin)
                throw new BusinessException(
                    "Has excedido el límite de horas extras aprobadas",
                    "HORA_EXTRA_EXCEDIDA");

            // Actualizar la salida del mismo registro (no crear uno nuevo)
            asistencia.HoraSalida = ahora;
            asistencia.TipoAsistencia = TipoAsistencia.HORA_EXTRA;
            asistencia.FechaModificacion = ahora;
            await _asistenciasRepo.UpdateAsync(asistencia);

            await _auditoria.RegistrarAsync(
                "asistencias",
                $"Salida con hora extra actualizada para empleado {empleadoId}, " +
                $"hora extra ID: {horaExtra.IdHoraExtra}, hora: {ahora:HH:mm:ss}."
            );

            return BuildResponse("SALIDA", ahora, asistencia.HoraEntrada, ahora,
                asistencia.Estado, "Salida con hora extra registrada", true);
        }

        public async Task<EstadoAsistenciaDTO> ObtenerEstadoAsistenciaAsync(int empleadoId)
        {
            var empleado = await _empleadosRepo.GetByIdAsync(empleadoId);
            if (empleado == null)
            {
                throw new BusinessException("Empleado no encontrado", "EMPLEADO_NO_ENCONTRADO");
            }

            var hoy = DateTime.Today;
            var registro = await _asistenciasRepo.GetByEmpleadoYFechaAsync(empleadoId, hoy);

            if (registro == null)
            {
                return new EstadoAsistenciaDTO
                {
                    TieneRegistro = false,
                    Estado = "SIN_REGISTRO",
                    PuedeMarcarEntrada = true,
                    PuedeMarcarSalida = false,
                    Mensaje = "No has registrado asistencia hoy"
                };
            }

            return new EstadoAsistenciaDTO
            {
                TieneRegistro = true,
                HoraEntrada = registro.HoraEntrada,
                HoraSalida = registro.HoraSalida,
                Estado = registro.Estado ?? "DESCONOCIDO",
                PuedeMarcarEntrada = false,
                PuedeMarcarSalida = registro.HoraSalida == null,
                Mensaje = registro.HoraSalida == null
                    ? "Puedes registrar tu salida"
                    : "Asistencia completa para hoy"
            };
        }

        private static MarcarAsistenciaResponse BuildResponse(
            string accion, DateTime hora, DateTime? horaEntrada,
            DateTime? horaSalida, string? estado, string mensaje, bool exito)
        {
            return new MarcarAsistenciaResponse
            {
                Accion = accion,
                Hora = hora,
                HoraEntrada = horaEntrada,
                HoraSalida = horaSalida,
                Estado = estado,
                Mensaje = mensaje,
                Exito = exito
            };
        }

        private async Task<MarcarAsistenciaResponse> RegistrarEntrada(
    Empleados empleado, TipoAsistencia tipo, DateTime ahora)
        {
            var existe = await _asistenciasRepo.ExisteRegistroAsync(
                empleado.IdEmpleado,
                ahora.Date);

            if (existe)
                throw new BusinessException(
                    "Ya existe un registro de asistencia para este empleado en esta fecha. " +
                    "La base de datos no permite múltiples registros por día.",
                    "REGISTRO_DUPLICADO");

            var nuevo = new Asistencias
            {
                EmpleadoId = empleado.IdEmpleado,
                FechaRegistro = ahora.Date,
                HoraEntrada = ahora,
                TipoAsistencia = tipo,
                Estado = DeterminarEstado(ahora, tipo),
                FechaCreacion = ahora
            };

            await _asistenciasRepo.CreateAsync(nuevo);

            await _auditoria.RegistrarAsync(
                "asistencias",
                $"Entrada {tipo} registrada para empleado {empleado.IdEmpleado}"
            );

            return BuildResponse("ENTRADA", ahora, ahora, null,
                nuevo.Estado, $"Entrada {tipo} registrada", true);
        }

        private async Task<MarcarAsistenciaResponse> RegistrarSalida(
    Asistencias registro, DateTime ahora)
        {
            registro.HoraSalida = ahora;
            registro.FechaModificacion = ahora;
            await _asistenciasRepo.UpdateAsync(registro);

            await _auditoria.RegistrarAsync(
                "asistencias",
                $"Salida registrada para asistencia ID {registro.IdAsistencia}, hora: {ahora:HH:mm:ss}."
            );

            return BuildResponse("SALIDA", ahora, registro.HoraEntrada, ahora,
                registro.Estado, "Salida registrada correctamente", true);
        }

        public async Task<AsistenciaDTO> CreateAsync(CrearAsistenciaDTO dto)
        {
            var empleado = await _empleadosRepo.GetByIdAsync(dto.EmpleadoId);
            if (empleado == null)
            {
                throw new BusinessException("Empleado no encontrado", "EMPLEADO_NO_ENCONTRADO");
            }

            var existe = await _asistenciasRepo.ExisteRegistroAsync(
                dto.EmpleadoId,
                dto.FechaRegistro.Date);

            if (existe)
            {
                throw new BusinessException(
                    "Ya existe un registro de asistencia para este empleado en esta fecha",
                    "REGISTRO_DUPLICADO");
            }

            var asistencia = new Asistencias
            {
                EmpleadoId = dto.EmpleadoId,
                FechaRegistro = dto.FechaRegistro.Date,
                HoraEntrada = dto.HoraEntrada,
                HoraSalida = dto.HoraSalida,
                Estado = dto.Estado,
                FechaCreacion = DateTime.Now
            };

            await _asistenciasRepo.CreateAsync(asistencia);

            await _auditoria.RegistrarAsync(
                tablaAfectada: "asistencias",
                descripcion: $"Asistencia creada manualmente por admin para empleado ID {dto.EmpleadoId} " +
                             $"({empleado.Nombre} {empleado.PrimerApellido}), " +
                             $"fecha: {dto.FechaRegistro:dd/MM/yyyy}, estado: {dto.Estado}."
            );

            var registroCreado = await _asistenciasRepo.GetByIdAsync(asistencia.IdAsistencia);

            return MapToDTO(registroCreado!);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var asistencia = await _asistenciasRepo.GetByIdAsync(id);
            if (asistencia == null)
                throw new BusinessException("Registro de asistencia no encontrado", "ASISTENCIA_NO_ENCONTRADA");

            var resultado = await _asistenciasRepo.DeleteAsync(id);

            if (resultado)
                await _auditoria.RegistrarAsync(
                    tablaAfectada: "asistencias",
                    descripcion: $"Asistencia ID {id} eliminada por admin. " +
                                 $"Empleado ID {asistencia.EmpleadoId}, " +
                                 $"fecha registro: {asistencia.FechaRegistro:dd/MM/yyyy}."
                );

            return resultado;
        }

        public async Task<IEnumerable<AsistenciaDTO>> GetAllAsync()
        {
            var asistencias = await _asistenciasRepo.GetAllAsync();
            return asistencias.Select(MapToDTO);
        }

        public async Task<IEnumerable<AsistenciaDTO>> GetByFiltrosAsync(FiltrosAsistenciaDTO filtros)
        {
            var asistencias = await _asistenciasRepo.GetByFiltrosAsync(
                filtros.EmpleadoId,
                filtros.FechaInicio,
                filtros.FechaFin,
                filtros.Estado,
                filtros.DepartamentoId
            );

            return asistencias.Select(MapToDTO);
        }

        public async Task<AsistenciaDTO?> GetByIdAsync(int id)
        {
            var asistencia = await _asistenciasRepo.GetByIdAsync(id);
            return asistencia != null ? MapToDTO(asistencia) : null;
        }

        public async Task<ReporteAsistenciaDTO> GetReporteEmpleadoAsync(
            int empleadoId,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            var empleado = await _empleadosRepo.GetByIdAsync(empleadoId);
            if (empleado == null)
            {
                throw new BusinessException("Empleado no encontrado", "EMPLEADO_NO_ENCONTRADO");
            }

            var asistencias = await _asistenciasRepo.GetByFiltrosAsync(
                empleadoId, fechaInicio, fechaFin, null, null);

            var lista = asistencias.ToList();
            var totalDias = lista.Count;
            var presente = lista.Count(a => a.Estado == "PRESENTE");
            var ausente = lista.Count(a => a.Estado == "AUSENTE");
            var tardanza = lista.Count(a => a.Estado == "TARDANZA");
            var permiso = lista.Count(a => a.Estado == "PERMISO");

            return new ReporteAsistenciaDTO
            {
                EmpleadoId = empleadoId,
                NombreCompleto = $"{empleado.Nombre} {empleado.PrimerApellido}",
                Departamento = empleado.Departamento?.NombreDepartamento ?? "N/A",
                TotalDias = totalDias,
                DiasPresente = presente,
                DiasAusente = ausente,
                DiasTardanza = tardanza,
                DiasPermiso = permiso,
                PorcentajeAsistencia = totalDias > 0
                    ? Math.Round((decimal)presente / totalDias * 100, 2)
                    : 0
            };
        }

        public async Task<bool> UpdateAsync(int id, ActualizarAsistenciaDTO dto)
        {
            var asistencia = await _asistenciasRepo.GetByIdAsync(id);
            if (asistencia == null)
            {
                throw new BusinessException("Registro de asistencia no encontrado", "ASISTENCIA_NO_ENCONTRADA");
            }

            var empleado = await _empleadosRepo.GetByIdAsync(dto.EmpleadoId);
            if (empleado == null)
            {
                throw new BusinessException("Empleado no encontrado", "EMPLEADO_NO_ENCONTRADO");
            }

            if (asistencia.EmpleadoId != dto.EmpleadoId ||
                asistencia.FechaRegistro.Date != dto.FechaRegistro.Date)
            {
                var existe = await _asistenciasRepo.ExisteRegistroAsync(
                    dto.EmpleadoId,
                    dto.FechaRegistro.Date);

                if (existe)
                {
                    throw new BusinessException(
                        "Ya existe un registro de asistencia para este empleado en esta fecha",
                        "REGISTRO_DUPLICADO");
                }
            }

            asistencia.EmpleadoId = dto.EmpleadoId;
            asistencia.FechaRegistro = dto.FechaRegistro.Date;
            asistencia.HoraEntrada = dto.HoraEntrada;
            asistencia.HoraSalida = dto.HoraSalida;
            asistencia.Estado = dto.Estado;
            asistencia.FechaModificacion = DateTime.Now;

            var resultado = await _asistenciasRepo.UpdateAsync(asistencia);

            if (resultado)
                await _auditoria.RegistrarAsync(
                    tablaAfectada: "asistencias",
                    descripcion: $"Asistencia ID {id} actualizada por admin. " +
                                 $"Empleado ID {dto.EmpleadoId} " +
                                 $"({empleado.Nombre} {empleado.PrimerApellido}), " +
                                 $"fecha: {dto.FechaRegistro:dd/MM/yyyy}, " +
                                 $"nuevo estado: {dto.Estado}."
                );

            return await _asistenciasRepo.UpdateAsync(asistencia);
        }

        private string DeterminarEstado(DateTime horaEntrada, TipoAsistencia? tipo)
        {
            if (tipo == TipoAsistencia.HORA_EXTRA)
                return TipoAsistencia.HORA_EXTRA.ToString();

            var horaTrabajo = new TimeSpan(8, 0, 0);
            return horaEntrada.TimeOfDay > horaTrabajo
                ? EstadoAsistencia.TARDANZA.ToString()
                : EstadoAsistencia.PRESENTE.ToString();
        }

        private AsistenciaDTO MapToDTO(Asistencias asistencia)
        {
            if (asistencia.Empleado == null)
            {
                throw new BusinessException("Los datos del empleado no están cargados", "DATOS_INCOMPLETOS");
            }

            TimeSpan? horasTrabajadas = null;
            if (asistencia.HoraEntrada.HasValue && asistencia.HoraSalida.HasValue)
            {
                horasTrabajadas = asistencia.HoraSalida.Value - asistencia.HoraEntrada.Value;
            }

            return new AsistenciaDTO
            {
                IdAsistencia = asistencia.IdAsistencia,
                EmpleadoId = asistencia.EmpleadoId,
                NombreEmpleado = $"{asistencia.Empleado.Nombre} {asistencia.Empleado.PrimerApellido}",
                CodigoEmpleado = asistencia.Empleado.CodigoEmpleado,
                FechaRegistro = asistencia.FechaRegistro,
                HoraEntrada = asistencia.HoraEntrada,
                HoraSalida = asistencia.HoraSalida,
                Estado = asistencia.Estado ?? "DESCONOCIDO",
                HorasTrabajadas = horasTrabajadas
            };
        }
    }
}