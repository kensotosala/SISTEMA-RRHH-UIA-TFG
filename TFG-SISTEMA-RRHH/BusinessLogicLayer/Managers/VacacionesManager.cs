using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;

namespace BusinessLogicLayer.Managers
{
    public class VacacionesManager : IVacacionesManager
    {
        private readonly IVacacionesRepository _vacacionesRepo;

        // Constantes según la ley de Costa Rica
        private const int DIAS_MINIMOS_SOLICITUD = 1;

        private const int DIAS_MAXIMOS_CONSECUTIVOS = 14;
        private const int DIAS_ANTICIPACION_MINIMA = 15;

        public VacacionesManager(IVacacionesRepository vacacionesRepo)
        {
            _vacacionesRepo = vacacionesRepo ?? throw new ArgumentNullException(nameof(vacacionesRepo));
        }

        // ========================================
        // OPERACIONES CRUD
        // ========================================

        public async Task<ResultDTO<ListarVacacionByIdDTO>> CrearSolicitudAsync(CrearVacacionDTO dto)
        {
            try
            {
                // Validaciones básicas
                var validacionesBasicas = ValidarDatosBasicos(dto);
                if (!validacionesBasicas.Exitoso)
                    return ResultDTO<ListarVacacionByIdDTO>.Failure(validacionesBasicas.Mensaje, validacionesBasicas.Errores);

                // Validar reglas de negocio
                var validacion = await ValidarSolicitudAsync(dto.EmpleadoId, dto.FechaInicio, dto.FechaFin);
                if (!validacion.Exitoso || !validacion.Datos!.EsValida)
                {
                    return ResultDTO<ListarVacacionByIdDTO>.Failure(
                        "La solicitud no cumple con las validaciones",
                        validacion.Datos?.Errores ?? new List<string>()
                    );
                }

                // Crear entidad
                var vacacion = new Vacaciones
                {
                    EmpleadoId = dto.EmpleadoId,
                    FechaInicio = dto.FechaInicio,
                    FechaFin = dto.FechaFin
                };

                // Guardar
                var vacacionCreada = await _vacacionesRepo.CrearAsync(vacacion);

                // Mapear manualmente a DTO
                var resultado = MapearAVacacionByIdDTO(vacacionCreada);

                return ResultDTO<ListarVacacionByIdDTO>.Success(
                    resultado,
                    "Solicitud creada exitosamente. Pendiente de aprobación."
                );
            }
            catch (Exception ex)
            {
                return ResultDTO<ListarVacacionByIdDTO>.Failure(
                    "Error al crear la solicitud",
                    new List<string> { ex.Message }
                );
            }
        }

        public async Task<ResultDTO<bool>> ActualizarSolicitudAsync(int id, ActualizarVacacionDTO dto)
        {
            try
            {
                // Verificar existencia
                var vacacionExistente = await _vacacionesRepo.ObtenerPorIdAsync(id);
                if (vacacionExistente == null)
                    return ResultDTO<bool>.Failure("La solicitud no existe");

                // Validar estado
                if (vacacionExistente.EstadoSolicitud != "PENDIENTE")
                {
                    return ResultDTO<bool>.Failure(
                        "Solo se pueden modificar solicitudes PENDIENTES"
                    );
                }

                // Validar nuevas fechas si cambiaron
                if (dto.FechaInicio != vacacionExistente.FechaInicio ||
                    dto.FechaFin != vacacionExistente.FechaFin)
                {
                    var validacion = await ValidarSolicitudAsync(dto.EmpleadoId, dto.FechaInicio, dto.FechaFin);
                    if (!validacion.Exitoso || !validacion.Datos!.EsValida)
                    {
                        return ResultDTO<bool>.Failure(
                            "Las nuevas fechas no son válidas",
                            validacion.Datos?.Errores ?? new List<string>()
                        );
                    }
                }

                // Actualizar campos manualmente
                vacacionExistente.EmpleadoId = dto.EmpleadoId;
                vacacionExistente.FechaInicio = dto.FechaInicio;
                vacacionExistente.FechaFin = dto.FechaFin;
                vacacionExistente.EstadoSolicitud = dto.EstadoSolicitud ?? vacacionExistente.EstadoSolicitud;
                vacacionExistente.FechaAprobacion = dto.FechaAprobacon;
                vacacionExistente.ComentariosRechazo = dto.ComentariosRechazo;

                // Guardar
                var resultado = await _vacacionesRepo.ActualizarAsync(vacacionExistente);

                return resultado
                    ? ResultDTO<bool>.Success(true, "Solicitud actualizada exitosamente")
                    : ResultDTO<bool>.Failure("No se pudo actualizar");
            }
            catch (Exception ex)
            {
                return ResultDTO<bool>.Failure(
                    "Error al actualizar",
                    new List<string> { ex.Message }
                );
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
                {
                    return ResultDTO<bool>.Failure(
                        "Solo se pueden cancelar solicitudes PENDIENTES"
                    );
                }

                var resultado = await _vacacionesRepo.EliminarAsync(id);

                return resultado
                    ? ResultDTO<bool>.Success(true, "Solicitud cancelada")
                    : ResultDTO<bool>.Failure("No se pudo cancelar");
            }
            catch (Exception ex)
            {
                return ResultDTO<bool>.Failure(
                    "Error al cancelar",
                    new List<string> { ex.Message }
                );
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

        // ========================================
        // APROBACIÓN Y RECHAZO
        // ========================================

        public async Task<ResultDTO<bool>> AprobarSolicitudAsync(int idVacacion, int jefeId)
        {
            try
            {
                var vacacion = await _vacacionesRepo.ObtenerPorIdAsync(idVacacion);
                if (vacacion == null)
                    return ResultDTO<bool>.Failure("Solicitud no encontrada");

                if (vacacion.EstadoSolicitud != "PENDIENTE")
                    return ResultDTO<bool>.Failure("Solo se pueden aprobar solicitudes PENDIENTES");

                // Validar que aún tenga días disponibles
                var validacion = await ValidarSolicitudAsync(
                    vacacion.EmpleadoId,
                    vacacion.FechaInicio,
                    vacacion.FechaFin
                );

                if (!validacion.Exitoso || !validacion.Datos!.EsValida)
                {
                    return ResultDTO<bool>.Failure(
                        "El empleado ya no tiene días disponibles",
                        validacion.Datos?.Errores ?? new List<string>()
                    );
                }

                // Actualizar
                vacacion.EstadoSolicitud = "APROBADA";
                vacacion.JefeApruebaId = jefeId;
                vacacion.FechaAprobacion = DateTime.Now;

                var resultado = await _vacacionesRepo.ActualizarAsync(vacacion);

                return resultado
                    ? ResultDTO<bool>.Success(true, "Solicitud aprobada exitosamente")
                    : ResultDTO<bool>.Failure("No se pudo aprobar");
            }
            catch (Exception ex)
            {
                return ResultDTO<bool>.Failure(
                    "Error al aprobar",
                    new List<string> { ex.Message }
                );
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

                // Actualizar
                vacacion.EstadoSolicitud = "RECHAZADA";
                vacacion.JefeApruebaId = jefeId;
                vacacion.FechaAprobacion = DateTime.Now;
                vacacion.ComentariosRechazo = comentarios;

                var resultado = await _vacacionesRepo.ActualizarAsync(vacacion);

                return resultado
                    ? ResultDTO<bool>.Success(true, "Solicitud rechazada")
                    : ResultDTO<bool>.Failure("No se pudo rechazar");
            }
            catch (Exception ex)
            {
                return ResultDTO<bool>.Failure(
                    "Error al rechazar",
                    new List<string> { ex.Message }
                );
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

                var resultado = new SaldoVacacionesDTO
                {
                    EmpleadoId = saldo.EmpleadoId,
                    NombreEmpleado = saldo.Empleado?.Nombre ?? "N/A",
                    Anio = saldo.Anio,
                    DiasAcumulados = saldo.DiasAcumulados,
                    DiasDisfrutados = saldo.DiasDisfrutados ?? 0,
                    DiasPendientesAprobacion = 0,
                    Mensaje = "Saldo recalculado exitosamente"
                };

                return ResultDTO<SaldoVacacionesDTO>.Success(resultado, "Saldo recalculado");
            }
            catch (Exception ex)
            {
                return ResultDTO<SaldoVacacionesDTO>.Failure(
                    "Error al recalcular",
                    new List<string> { ex.Message }
                );
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