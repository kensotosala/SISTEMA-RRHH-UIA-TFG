using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;

namespace BusinessLogicLayer.Managers
{
    public class VacacionesManager : IVacacionesManager
    {
        private readonly IVacacionesRepository _vacacionesRepo;
        private readonly NotificacionesManager _notificacionesManager;
        private readonly IAuditoriaService _auditoria;

        private const int DIAS_MINIMOS_SOLICITUD = 1;

        private const int DIAS_MAXIMOS_CONSECUTIVOS = 14;
        private const int DIAS_ANTICIPACION_MINIMA = 15;

        public VacacionesManager(IVacacionesRepository vacacionesRepo, NotificacionesManager notificacionesManager, IAuditoriaService auditoria)
        {
            _vacacionesRepo = vacacionesRepo ?? throw new ArgumentNullException(nameof(vacacionesRepo));
            _notificacionesManager = notificacionesManager;
            _auditoria = auditoria;
        }

        public async Task<ResultDTO<ListarVacacionByIdDTO>> CrearSolicitudAsync(CrearVacacionDTO dto)
        {
            try
            {
                var validacionesBasicas = ValidarDatosBasicos(dto);
                if (!validacionesBasicas.Exitoso)
                    return ResultDTO<ListarVacacionByIdDTO>.Failure(
                        validacionesBasicas.Mensaje, validacionesBasicas.Errores);

                var validacion = await ValidarSolicitudAsync(dto.EmpleadoId, dto.FechaInicio, dto.FechaFin);
                if (!validacion.Exitoso || !validacion.Datos!.EsValida)
                    return ResultDTO<ListarVacacionByIdDTO>.Failure(
                        "La solicitud no cumple con las validaciones",
                        validacion.Datos?.Errores ?? new List<string>());

                var vacacion = new Vacaciones
                {
                    EmpleadoId = dto.EmpleadoId,
                    FechaInicio = dto.FechaInicio,
                    FechaFin = dto.FechaFin
                };

                var vacacionCreada = await _vacacionesRepo.CrearAsync(vacacion);
                var resultado = MapearAVacacionByIdDTO(vacacionCreada);

                await _auditoria.RegistrarAsync(
                    tablaAfectada: "vacaciones",
                    descripcion: $"Solicitud de vacaciones creada (ID {vacacionCreada.IdVacacion}) " +
                                   $"para empleado ID {dto.EmpleadoId}, " +
                                   $"período: {dto.FechaInicio:dd/MM/yyyy} - {dto.FechaFin:dd/MM/yyyy}, " +
                                   $"días: {(dto.FechaFin - dto.FechaInicio).Days + 1}."
                );

                var dias = (dto.FechaFin - dto.FechaInicio).Days + 1;
                await _notificacionesManager.NotificarSolicitudCreadaAsync(
                    dto.EmpleadoId, "Vacaciones",
                    $@"<p><strong>Fecha de Inicio:</strong> {dto.FechaInicio:dd/MM/yyyy}</p>
                       <p><strong>Fecha de Fin:</strong> {dto.FechaFin:dd/MM/yyyy}</p>
                       <p><strong>Total de Días:</strong> {dias} día(s)</p>
                       <p><strong>Estado:</strong> PENDIENTE</p>"
                );

                return ResultDTO<ListarVacacionByIdDTO>.Success(
                    resultado, "Solicitud creada exitosamente. Pendiente de aprobación.");
            }
            catch (Exception ex)
            {
                return ResultDTO<ListarVacacionByIdDTO>.Failure(
                    "Error al crear la solicitud", new List<string> { ex.Message });
            }
        }

        public async Task<ResultDTO<bool>> ActualizarSolicitudAsync(int id, ActualizarVacacionDTO dto)
        {
            try
            {
                var vacacionExistente = await _vacacionesRepo.ObtenerPorIdAsync(id);
                if (vacacionExistente == null)
                    return ResultDTO<bool>.Failure("La solicitud no existe");

                if (vacacionExistente.EstadoSolicitud != "PENDIENTE")
                    return ResultDTO<bool>.Failure("Solo se pueden modificar solicitudes PENDIENTES");

                if (dto.FechaInicio != vacacionExistente.FechaInicio ||
                    dto.FechaFin != vacacionExistente.FechaFin)
                {
                    var validacion = await ValidarSolicitudAsync(dto.EmpleadoId, dto.FechaInicio, dto.FechaFin);
                    if (!validacion.Exitoso || !validacion.Datos!.EsValida)
                        return ResultDTO<bool>.Failure(
                            "Las nuevas fechas no son válidas",
                            validacion.Datos?.Errores ?? new List<string>());
                }

                var fechaInicioAnterior = vacacionExistente.FechaInicio;
                var fechaFinAnterior = vacacionExistente.FechaFin;

                vacacionExistente.EmpleadoId = dto.EmpleadoId;
                vacacionExistente.FechaInicio = dto.FechaInicio;
                vacacionExistente.FechaFin = dto.FechaFin;
                vacacionExistente.EstadoSolicitud = dto.EstadoSolicitud ?? vacacionExistente.EstadoSolicitud;
                vacacionExistente.FechaAprobacion = dto.FechaAprobacon;
                vacacionExistente.ComentariosRechazo = dto.ComentariosRechazo;

                var resultado = await _vacacionesRepo.ActualizarAsync(vacacionExistente);

                if (resultado)
                    await _auditoria.RegistrarAsync(
                        tablaAfectada: "vacaciones",
                        descripcion: $"Solicitud de vacaciones ID {id} actualizada. " +
                                       $"Empleado ID {dto.EmpleadoId}, " +
                                       $"período anterior: {fechaInicioAnterior:dd/MM/yyyy} - {fechaFinAnterior:dd/MM/yyyy}, " +
                                       $"período nuevo: {dto.FechaInicio:dd/MM/yyyy} - {dto.FechaFin:dd/MM/yyyy}."
                    );

                var dias = (vacacionExistente.FechaFin - vacacionExistente.FechaInicio).Days + 1;
                await _notificacionesManager.NotificarSolicitudAprobadaAsync(
                    vacacionExistente.EmpleadoId, "Vacaciones",
                    $@"<p><strong>Fecha de Inicio:</strong> {vacacionExistente.FechaInicio:dd/MM/yyyy}</p>
                       <p><strong>Fecha de Fin:</strong> {vacacionExistente.FechaFin:dd/MM/yyyy}</p>
                       <p><strong>Total de Días:</strong> {dias} día(s)</p>
                       <p><strong>Fecha de Aprobación:</strong> {vacacionExistente.FechaAprobacion:dd/MM/yyyy HH:mm}</p>"
                );

                return resultado
                    ? ResultDTO<bool>.Success(true, "Solicitud actualizada exitosamente")
                    : ResultDTO<bool>.Failure("No se pudo actualizar");
            }
            catch (Exception ex)
            {
                return ResultDTO<bool>.Failure("Error al actualizar", new List<string> { ex.Message });
            }
        }

        public async Task<ResultDTO<bool>> CancelarSolicitudAsync(int id)
        {
            try
            {
                var vacacion = await _vacacionesRepo.ObtenerPorIdAsync(id);
                if (vacacion == null)
                    return ResultDTO<bool>.Failure("La solicitud no existe");

                if (vacacion.EstadoSolicitud != "PENDIENTE")
                    return ResultDTO<bool>.Failure("Solo se pueden cancelar solicitudes PENDIENTES");

                var empleadoId = vacacion.EmpleadoId;
                var fechaInicio = vacacion.FechaInicio;
                var fechaFin = vacacion.FechaFin;
                var dias = (fechaFin - fechaInicio).Days + 1;

                var resultado = await _vacacionesRepo.EliminarAsync(id);

                if (resultado)
                    await _auditoria.RegistrarAsync(
                        tablaAfectada: "vacaciones",
                        descripcion: $"Solicitud de vacaciones ID {id} cancelada. " +
                                       $"Empleado ID {empleadoId}, " +
                                       $"período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}."
                    );

                await _notificacionesManager.NotificarSolicitudCanceladaAsync(
                    empleadoId, "Vacaciones",
                    $@"<p><strong>Fecha de Inicio:</strong> {fechaInicio:dd/MM/yyyy}</p>
                       <p><strong>Fecha de Fin:</strong> {fechaFin:dd/MM/yyyy}</p>
                       <p><strong>Total de Días:</strong> {dias} día(s)</p>
                       <p><strong>Fecha de Cancelación:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>"
                );

                return resultado
                    ? ResultDTO<bool>.Success(true, "Solicitud cancelada")
                    : ResultDTO<bool>.Failure("No se pudo cancelar");
            }
            catch (Exception ex)
            {
                return ResultDTO<bool>.Failure("Error al cancelar", new List<string> { ex.Message });
            }
        }

        public async Task<ResultDTO<ListarVacacionByIdDTO>> ObtenerPorIdAsync(int id)
        {
            try
            {
                var vacacion = await _vacacionesRepo.ObtenerPorIdAsync(id);

                if (vacacion == null)
                    return ResultDTO<ListarVacacionByIdDTO>.Failure("Vacación no encontrada");

                var resultado = MapearAVacacionByIdDTO(vacacion);

                return ResultDTO<ListarVacacionByIdDTO>.Success(resultado);
            }
            catch (Exception ex)
            {
                return ResultDTO<ListarVacacionByIdDTO>.Failure(
                    "Error al obtener la vacación",
                    new List<string> { ex.Message }
                );
            }
        }

        public async Task<ResultDTO<IEnumerable<ListarVacacionesDTO>>> ObtenerTodosAsync()
        {
            try
            {
                var vacaciones = await _vacacionesRepo.ObtenerTodosAsync();

                // Mapeo manual
                var resultado = vacaciones.Select(v => MapearAVacacionDTO(v)).ToList();

                return ResultDTO<IEnumerable<ListarVacacionesDTO>>.Success(resultado);
            }
            catch (Exception ex)
            {
                return ResultDTO<IEnumerable<ListarVacacionesDTO>>.Failure(
                    "Error al obtener las vacaciones",
                    new List<string> { ex.Message }
                );
            }
        }

        public async Task<ResultDTO<IEnumerable<ListarVacacionesDTO>>> ObtenerPorEmpleadoAsync(int empleadoId)
        {
            try
            {
                var vacaciones = await _vacacionesRepo.ObtenerPorEmpleadoIdAsync(empleadoId);

                // Mapeo manual
                var resultado = vacaciones.Select(v => MapearAVacacionDTO(v)).ToList();

                return ResultDTO<IEnumerable<ListarVacacionesDTO>>.Success(resultado);
            }
            catch (Exception ex)
            {
                return ResultDTO<IEnumerable<ListarVacacionesDTO>>.Failure(
                    "Error al obtener las vacaciones del empleado",
                    new List<string> { ex.Message }
                );
            }
        }

        public async Task<ResultDTO<IEnumerable<ListarVacacionesDTO>>> ObtenerPorEstadoAsync(string estado)
        {
            try
            {
                var vacaciones = await _vacacionesRepo.ObtenerPorEstadoAsync(estado);

                // Mapeo manual
                var resultado = vacaciones.Select(v => MapearAVacacionDTO(v)).ToList();

                return ResultDTO<IEnumerable<ListarVacacionesDTO>>.Success(resultado);
            }
            catch (Exception ex)
            {
                return ResultDTO<IEnumerable<ListarVacacionesDTO>>.Failure(
                    "Error al obtener las vacaciones por estado",
                    new List<string> { ex.Message }
                );
            }
        }

        public async Task<ResultDTO<bool>> AprobarSolicitudAsync(int idVacacion, int jefeId)
        {
            try
            {
                var vacacion = await _vacacionesRepo.ObtenerPorIdAsync(idVacacion);
                if (vacacion == null)
                    return ResultDTO<bool>.Failure("Solicitud no encontrada");

                if (vacacion.EstadoSolicitud != "PENDIENTE")
                    return ResultDTO<bool>.Failure("Solo se pueden aprobar solicitudes PENDIENTES");

                var validacion = await ValidarSolicitudAsync(
                    vacacion.EmpleadoId, vacacion.FechaInicio, vacacion.FechaFin);

                if (!validacion.Exitoso || !validacion.Datos!.EsValida)
                    return ResultDTO<bool>.Failure(
                        "El empleado ya no tiene días disponibles",
                        validacion.Datos?.Errores ?? new List<string>());

                vacacion.EstadoSolicitud = "APROBADA";
                vacacion.JefeApruebaId = jefeId;
                vacacion.FechaAprobacion = DateTime.Now;

                var resultado = await _vacacionesRepo.ActualizarAsync(vacacion);

                if (resultado)
                    await _auditoria.RegistrarAsync(
                        tablaAfectada: "vacaciones",
                        descripcion: $"Solicitud de vacaciones ID {idVacacion} aprobada " +
                                       $"por jefe ID {jefeId}. " +
                                       $"Empleado ID {vacacion.EmpleadoId}, " +
                                       $"período: {vacacion.FechaInicio:dd/MM/yyyy} - {vacacion.FechaFin:dd/MM/yyyy}."
                    );

                return resultado
                    ? ResultDTO<bool>.Success(true, "Solicitud aprobada exitosamente")
                    : ResultDTO<bool>.Failure("No se pudo aprobar");
            }
            catch (Exception ex)
            {
                return ResultDTO<bool>.Failure("Error al aprobar", new List<string> { ex.Message });
            }
        }

        public async Task<ResultDTO<bool>> RechazarSolicitudAsync(int idVacacion, int jefeId, string comentarios)
        {
            try
            {
                var vacacion = await _vacacionesRepo.ObtenerPorIdAsync(idVacacion);
                if (vacacion == null)
                    return ResultDTO<bool>.Failure("Solicitud no encontrada");

                if (vacacion.EstadoSolicitud != "PENDIENTE")
                    return ResultDTO<bool>.Failure("Solo se pueden rechazar solicitudes PENDIENTES");

                if (string.IsNullOrWhiteSpace(comentarios))
                    return ResultDTO<bool>.Failure("Debe proporcionar comentarios");

                vacacion.EstadoSolicitud = "RECHAZADA";
                vacacion.JefeApruebaId = jefeId;
                vacacion.FechaAprobacion = DateTime.Now;
                vacacion.ComentariosRechazo = comentarios;

                var resultado = await _vacacionesRepo.ActualizarAsync(vacacion);

                if (resultado)
                    await _auditoria.RegistrarAsync(
                        tablaAfectada: "vacaciones",
                        descripcion: $"Solicitud de vacaciones ID {idVacacion} rechazada " +
                                       $"por jefe ID {jefeId}. " +
                                       $"Empleado ID {vacacion.EmpleadoId}, " +
                                       $"comentarios: '{comentarios}'."
                    );

                var dias = (vacacion.FechaFin - vacacion.FechaInicio).Days + 1;
                await _notificacionesManager.NotificarSolicitudRechazadaAsync(
                    vacacion.EmpleadoId, "Vacaciones", comentarios,
                    $@"<p><strong>Fecha de Inicio:</strong> {vacacion.FechaInicio:dd/MM/yyyy}</p>
                       <p><strong>Fecha de Fin:</strong> {vacacion.FechaFin:dd/MM/yyyy}</p>
                       <p><strong>Total de Días:</strong> {dias} día(s)</p>"
                );

                return resultado
                    ? ResultDTO<bool>.Success(true, "Solicitud rechazada")
                    : ResultDTO<bool>.Failure("No se pudo rechazar");
            }
            catch (Exception ex)
            {
                return ResultDTO<bool>.Failure("Error al rechazar", new List<string> { ex.Message });
            }
        }

        // ========================================
        // SALDOS
        // ========================================

        public async Task<ResultDTO<SaldoVacacionesDTO>> ObtenerSaldoAsync(int empleadoId, int anio)
        {
            try
            {
                var saldo = await _vacacionesRepo.ObtenerSaldoVacacionesAsync(empleadoId, anio);

                if (saldo == null)
                {
                    saldo = await _vacacionesRepo.CalcularYGuardarSaldoAsync(empleadoId, anio);
                }

                // Contar días pendientes
                var vacacionesPendientes = await _vacacionesRepo.ObtenerPorEmpleadoIdAsync(empleadoId);
                var diasPendientes = vacacionesPendientes
                    .Where(v => v.EstadoSolicitud == "PENDIENTE" && v.FechaInicio.Year == anio)
                    .Sum(v => (v.FechaFin - v.FechaInicio).Days + 1);

                // Mapeo manual
                var resultado = new SaldoVacacionesDTO
                {
                    EmpleadoId = saldo.EmpleadoId,
                    NombreEmpleado = saldo.Empleado?.Nombre ?? "N/A",
                    Anio = saldo.Anio,
                    DiasAcumulados = saldo.DiasAcumulados,
                    DiasDisfrutados = saldo.DiasDisfrutados ?? 0,
                    DiasPendientesAprobacion = diasPendientes,
                    Mensaje = $"Tiene {saldo.DiasAcumulados - (saldo.DiasDisfrutados ?? 0)} días disponibles para {anio}"
                };

                return ResultDTO<SaldoVacacionesDTO>.Success(resultado);
            }
            catch (Exception ex)
            {
                return ResultDTO<SaldoVacacionesDTO>.Failure(
                    "Error al obtener el saldo",
                    new List<string> { ex.Message }
                );
            }
        }

        public async Task<ResultDTO<IEnumerable<SaldoVacacionesDTO>>> ObtenerHistorialSaldosAsync(int empleadoId)
        {
            try
            {
                var saldos = await _vacacionesRepo.ObtenerHistorialSaldosAsync(empleadoId);

                // Mapeo manual
                var resultado = saldos.Select(s => new SaldoVacacionesDTO
                {
                    EmpleadoId = s.EmpleadoId,
                    NombreEmpleado = s.Empleado?.Nombre ?? "N/A",
                    Anio = s.Anio,
                    DiasAcumulados = s.DiasAcumulados,
                    DiasDisfrutados = s.DiasDisfrutados ?? 0,
                    DiasPendientesAprobacion = 0,
                    Mensaje = $"{s.Anio}: {s.DiasAcumulados - (s.DiasDisfrutados ?? 0)} días disponibles"
                }).ToList();

                return ResultDTO<IEnumerable<SaldoVacacionesDTO>>.Success(resultado);
            }
            catch (Exception ex)
            {
                return ResultDTO<IEnumerable<SaldoVacacionesDTO>>.Failure(
                    "Error al obtener el historial",
                    new List<string> { ex.Message }
                );
            }
        }

        public async Task<ResultDTO<SaldoVacacionesDTO>> RecalcularSaldoAsync(int empleadoId, int anio)
        {
            try
            {
                var saldo = await _vacacionesRepo.CalcularYGuardarSaldoAsync(empleadoId, anio);

                await _auditoria.RegistrarAsync(
                    tablaAfectada: "saldo_vacaciones",
                    descripcion: $"Saldo de vacaciones recalculado para empleado ID {empleadoId}, " +
                                   $"año {anio}. " +
                                   $"Días acumulados: {saldo.DiasAcumulados}, " +
                                   $"días disfrutados: {saldo.DiasDisfrutados ?? 0}."
                );

                return ResultDTO<SaldoVacacionesDTO>.Success(new SaldoVacacionesDTO
                {
                    EmpleadoId = saldo.EmpleadoId,
                    NombreEmpleado = saldo.Empleado?.Nombre ?? "N/A",
                    Anio = saldo.Anio,
                    DiasAcumulados = saldo.DiasAcumulados,
                    DiasDisfrutados = saldo.DiasDisfrutados ?? 0,
                    DiasPendientesAprobacion = 0,
                    Mensaje = "Saldo recalculado exitosamente"
                }, "Saldo recalculado");
            }
            catch (Exception ex)
            {
                return ResultDTO<SaldoVacacionesDTO>.Failure(
                    "Error al recalcular", new List<string> { ex.Message });
            }
        }

        // ========================================
        // VALIDACIONES
        // ========================================

        public async Task<ResultDTO<ValidacionVacacionesDTO>> ValidarSolicitudAsync(
            int empleadoId,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            var validacion = new ValidacionVacacionesDTO
            {
                EsValida = true,
                Errores = new List<string>(),
                Advertencias = new List<string>()
            };

            try
            {
                // 1️⃣ Validar fechas
                if (fechaFin < fechaInicio)
                {
                    validacion.EsValida = false;
                    validacion.Errores.Add("La fecha de fin no puede ser anterior a la de inicio");
                }

                if (fechaInicio < DateTime.Today)
                {
                    validacion.EsValida = false;
                    validacion.Errores.Add("No se pueden solicitar vacaciones en el pasado");
                }

                var diasSolicitados = (fechaFin - fechaInicio).Days + 1;
                validacion.DiasSolicitados = diasSolicitados;

                if (diasSolicitados < DIAS_MINIMOS_SOLICITUD)
                {
                    validacion.EsValida = false;
                    validacion.Errores.Add($"Debe solicitar al menos {DIAS_MINIMOS_SOLICITUD} día");
                }

                if (diasSolicitados > DIAS_MAXIMOS_CONSECUTIVOS)
                {
                    validacion.Advertencias.Add(
                        $"Está solicitando más de {DIAS_MAXIMOS_CONSECUTIVOS} días consecutivos"
                    );
                }

                // 2️⃣ Validar anticipación
                var diasAnticipacion = (fechaInicio - DateTime.Today).Days;
                if (diasAnticipacion < DIAS_ANTICIPACION_MINIMA)
                {
                    validacion.Advertencias.Add(
                        $"Se recomienda solicitar con al menos {DIAS_ANTICIPACION_MINIMA} días de anticipación"
                    );
                }

                // 3️⃣ Verificar solapamiento
                var tieneSolapamiento = await _vacacionesRepo.TieneVacacionesEnRangoAsync(
                    empleadoId,
                    fechaInicio,
                    fechaFin
                );

                if (tieneSolapamiento)
                {
                    validacion.EsValida = false;
                    validacion.Errores.Add("Ya tiene vacaciones aprobadas en esas fechas");
                }

                // 4️⃣ Verificar saldo
                var anio = fechaInicio.Year;
                var saldo = await _vacacionesRepo.ObtenerSaldoVacacionesAsync(empleadoId, anio);

                if (saldo == null)
                {
                    saldo = await _vacacionesRepo.CalcularYGuardarSaldoAsync(empleadoId, anio);
                }

                var diasDisponibles = saldo.DiasAcumulados - (saldo.DiasDisfrutados ?? 0);
                validacion.DiasDisponibles = diasDisponibles;

                if (diasSolicitados > diasDisponibles)
                {
                    validacion.EsValida = false;
                    validacion.Errores.Add(
                        $"No tiene suficientes días. Disponibles: {diasDisponibles}, Solicitados: {diasSolicitados}"
                    );
                }

                return ResultDTO<ValidacionVacacionesDTO>.Success(validacion);
            }
            catch (Exception ex)
            {
                validacion.EsValida = false;
                validacion.Errores.Add($"Error en validación: {ex.Message}");
                return ResultDTO<ValidacionVacacionesDTO>.Failure(
                    "Error al validar",
                    validacion.Errores
                );
            }
        }

        // ========================================
        // MÉTODOS PRIVADOS DE MAPEO MANUAL
        // ========================================

        private ListarVacacionesDTO MapearAVacacionDTO(Vacaciones vacacion)
        {
            return new ListarVacacionesDTO
            {
                IdVacacion = vacacion.IdVacacion,
                EmpleadoId = vacacion.EmpleadoId,
                FechaSolicitud = vacacion.FechaSolicitud,
                FechaInicio = vacacion.FechaInicio,
                FechaFin = vacacion.FechaFin,
                EstadoSolicitud = vacacion.EstadoSolicitud,
                JefeApruebaId = vacacion.JefeApruebaId,
                FechaAprobacion = vacacion.FechaAprobacion,
                ComentariosRechazo = vacacion.ComentariosRechazo,
                FechaCreacion = vacacion.FechaCreacion,
                FechaModificacion = vacacion.FechaModificacion
            };
        }

        private ListarVacacionByIdDTO MapearAVacacionByIdDTO(Vacaciones vacacion)
        {
            return new ListarVacacionByIdDTO
            {
                IdVacacion = vacacion.IdVacacion,
                EmpleadoId = vacacion.EmpleadoId,
                FechaSolicitud = vacacion.FechaSolicitud,
                FechaInicio = vacacion.FechaInicio,
                FechaFin = vacacion.FechaFin,
                EstadoSolicitud = vacacion.EstadoSolicitud,
                JefeApruebaId = vacacion.JefeApruebaId,
                FechaAprobacion = vacacion.FechaAprobacion,
                ComentariosRechazo = vacacion.ComentariosRechazo,
                FechaCreacion = vacacion.FechaCreacion,
                FechaModificacion = vacacion.FechaModificacion
            };
        }

        private ResultDTO<bool> ValidarDatosBasicos(CrearVacacionDTO dto)
        {
            var errores = new List<string>();

            if (dto.EmpleadoId <= 0)
                errores.Add("ID de empleado inválido");

            if (dto.FechaInicio == default)
                errores.Add("Fecha de inicio es requerida");

            if (dto.FechaFin == default)
                errores.Add("Fecha de fin es requerida");

            if (errores.Any())
            {
                return ResultDTO<bool>.Failure("Datos inválidos", errores);
            }

            return ResultDTO<bool>.Success(true);
        }
    }
}