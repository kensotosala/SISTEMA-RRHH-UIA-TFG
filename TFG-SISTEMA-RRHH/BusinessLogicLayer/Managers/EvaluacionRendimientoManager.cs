using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogicLayer.Managers
{
    public class EvaluacionRendimientoManager : IEvaluacionRendimientoManager
    {
        private readonly IEvaluacionRendimientoRepository _repo;
        private readonly ILogger<EvaluacionRendimientoManager> _logger;

        private static readonly HashSet<string> EstadosPermitidos =
            new(StringComparer.OrdinalIgnoreCase) { "COMPLETADA", "ANULADA" };

        public EvaluacionRendimientoManager(
            IEvaluacionRendimientoRepository repo,
            ILogger<EvaluacionRendimientoManager> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<ResultDTO<IEnumerable<EvaluacionResponseDTO>>> GetAllAsync()
        {
            try
            {
                var evaluaciones = await _repo.GetAllAsync();
                var response = evaluaciones.Select(MapToResponseDTO);
                return ResultDTO<IEnumerable<EvaluacionResponseDTO>>
                    .Success(response, $"{response.Count()} evaluaciones encontradas.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las evaluaciones.");
                return ResultDTO<IEnumerable<EvaluacionResponseDTO>>
                    .Failure("Error interno al obtener las evaluaciones.");
            }
        }

        public async Task<ResultDTO<EvaluacionResponseDTO>> GetByIdAsync(int idEvaluacion)
        {
            try
            {
                var evaluacion = await _repo.GetByIdAsync(idEvaluacion);
                if (evaluacion is null)
                    return ResultDTO<EvaluacionResponseDTO>
                        .Failure($"No se encontró la evaluación con Id {idEvaluacion}.");

                return ResultDTO<EvaluacionResponseDTO>.Success(MapToResponseDTO(evaluacion));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener evaluación con Id {Id}.", idEvaluacion);
                return ResultDTO<EvaluacionResponseDTO>.Failure("Error interno al obtener la evaluación.");
            }
        }

        public async Task<ResultDTO<EvaluacionResponseDTO>> CreateAsync(CreateEvaluacionDTO dto)
        {
            try
            {
                var errores = ValidarCreateDTO(dto);
                if (errores.Any())
                    return ResultDTO<EvaluacionResponseDTO>.Failure("Datos de entrada inválidos.", errores);

                int anioEvaluacion = dto.FechaInicio.Year;
                bool yaExiste = await _repo.ExisteEvaluacionEnAnioAsync(dto.EmpleadoId, anioEvaluacion);
                if (yaExiste)
                    return ResultDTO<EvaluacionResponseDTO>.Failure(
                        $"El empleado {dto.EmpleadoId} ya tiene una evaluación registrada " +
                        $"para el año {anioEvaluacion}. Solo se permite una evaluación por año.");

                sbyte puntuacionTotal = CalcularPuntuacionTotal(
                    dto.Detalles.Select(d => d.Puntuacion).ToList());

                var entity = new EvaluacionesRendimiento
                {
                    EmpleadoId = dto.EmpleadoId,
                    FechaInicio = dto.FechaInicio,
                    FechaFin = dto.FechaFin,
                    EvaluadorId = dto.EvaluadorId,
                    PuntuacionTotal = puntuacionTotal,
                    Comentarios = dto.Comentarios?.Trim(),
                    Estado = "PENDIENTE",
                    DetalleEvaluaciones = dto.Detalles.Select(d => new DetalleEvaluaciones
                    {
                        IdMetrica = d.IdMetrica,
                        Puntuacion = d.Puntuacion,
                        Comentarios = d.Comentarios?.Trim(),
                        FechaCreacion = DateTime.UtcNow,
                        FechaModificacion = DateTime.UtcNow
                    }).ToList()
                };

                var creada = await _repo.CreateAsync(entity);
                var resultado = await _repo.GetByIdAsync(creada.IdEvaluacion);

                return ResultDTO<EvaluacionResponseDTO>
                    .Success(MapToResponseDTO(resultado!), "Evaluación creada exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear evaluación.");
                return ResultDTO<EvaluacionResponseDTO>.Failure("Error interno al crear la evaluación.");
            }
        }

        public async Task<ResultDTO<EvaluacionResponseDTO>> UpdateAsync(int idEvaluacion, UpdateEvaluacionDTO dto)
        {
            try
            {
                if (idEvaluacion != dto.IdEvaluacion)
                    return ResultDTO<EvaluacionResponseDTO>
                        .Failure("El Id del recurso no coincide con el Id del cuerpo de la solicitud.");

                var errores = ValidarUpdateDTO(dto);
                if (errores.Any())
                    return ResultDTO<EvaluacionResponseDTO>.Failure("Datos de entrada inválidos.", errores);

                var existente = await _repo.GetByIdAsync(idEvaluacion);
                if (existente is null)
                    return ResultDTO<EvaluacionResponseDTO>
                        .Failure($"No se encontró la evaluación con Id {idEvaluacion}.");

                bool yaExisteOtra = await _repo.ExisteEvaluacionEnAnioAsync(existente.EmpleadoId, dto.FechaInicio.Year, excluirIdEvaluacion: idEvaluacion);
                if (yaExisteOtra)
                    return ResultDTO<EvaluacionResponseDTO>.Failure(
                        $"Ya existe otra evaluación para este empleado en el año {dto.FechaInicio.Year}.");

                // ── Merge de detalles ─────────────────────────────────────────
                // IDs entrantes con IdDetalle > 0 son actualizaciones; los de IdDetalle == 0 son nuevos.
                var idsEntrantes = dto.Detalles
                    .Where(d => d.IdDetalle > 0)
                    .Select(d => d.IdDetalle)
                    .ToHashSet();

                var idsExistentes = existente.DetalleEvaluaciones
                    .Select(d => d.IdDetalle)
                    .ToHashSet();

                // Eliminar detalles que ya no están en la lista
                var aEliminar = idsExistentes.Except(idsEntrantes).ToList();
                foreach (var idDetalle in aEliminar)
                    await _repo.DeleteDetalleAsync(idDetalle);

                // Actualizar o crear detalles
                foreach (var detalleDto in dto.Detalles)
                {
                    if (detalleDto.IdDetalle > 0 && idsExistentes.Contains(detalleDto.IdDetalle))
                    {
                        // Actualizar existente
                        var detalleEntity = existente.DetalleEvaluaciones
                            .First(d => d.IdDetalle == detalleDto.IdDetalle);
                        detalleEntity.IdMetrica = detalleDto.IdMetrica;
                        detalleEntity.Puntuacion = detalleDto.Puntuacion;
                        detalleEntity.Comentarios = detalleDto.Comentarios?.Trim();
                        await _repo.UpdateDetalleAsync(detalleEntity);
                    }
                    else
                    {
                        // Crear nuevo detalle
                        await _repo.CreateDetalleAsync(new DetalleEvaluaciones
                        {
                            IdEvaluacion = idEvaluacion,
                            IdMetrica = detalleDto.IdMetrica,
                            Puntuacion = detalleDto.Puntuacion,
                            Comentarios = detalleDto.Comentarios?.Trim()
                        });
                    }
                }

                // ── Actualizar cabecera ───────────────────────────────────────
                sbyte puntuacionTotal = CalcularPuntuacionTotal(
                    dto.Detalles.Select(d => d.Puntuacion).ToList());

                existente.FechaInicio = dto.FechaInicio;
                existente.FechaFin = dto.FechaFin;
                existente.EvaluadorId = dto.EvaluadorId;
                existente.Comentarios = dto.Comentarios?.Trim();
                existente.Estado = dto.Estado;
                existente.PuntuacionTotal = puntuacionTotal;

                await _repo.UpdateAsync(existente);

                var resultado = await _repo.GetByIdAsync(idEvaluacion);
                return ResultDTO<EvaluacionResponseDTO>
                    .Success(MapToResponseDTO(resultado!), "Evaluación actualizada exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar evaluación con Id {Id}.", idEvaluacion);
                return ResultDTO<EvaluacionResponseDTO>.Failure("Error interno al actualizar la evaluación.");
            }
        }

        public async Task<ResultDTO<bool>> DeleteAsync(int idEvaluacion)
        {
            try
            {
                var evaluacion = await _repo.GetByIdAsync(idEvaluacion);

                if (evaluacion is null)
                    return ResultDTO<bool>
                        .Failure($"No se encontró la evaluación con Id {idEvaluacion}.");

                if (evaluacion.Estado?.ToUpper() == "ANULADA")
                    return ResultDTO<bool>
                        .Failure($"La evaluación con Id {idEvaluacion} ya se encuentra anulada.");

                evaluacion.Estado = "ANULADA";
                await _repo.UpdateAsync(evaluacion);

                return ResultDTO<bool>.Success(true,
                    $"Evaluación {idEvaluacion} anulada exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al anular evaluación con Id {Id}.", idEvaluacion);
                return ResultDTO<bool>.Failure("Error interno al anular la evaluación.");
            }
        }

        public async Task<ResultDTO<bool>> AproveAsync(int idEvaluacion)
        {
            try
            {
                var evaluacion = await _repo.GetByIdAsync(idEvaluacion);

                if (evaluacion is null)
                    return ResultDTO<bool>
                        .Failure($"No se encontró la evaluación con Id {idEvaluacion}.");

                if (evaluacion.Estado?.ToUpper() == "ANULADA")
                    return ResultDTO<bool>
                        .Failure($"La evaluación con Id {idEvaluacion} ya se encuentra anulada. No puede ser aprobada");

                if (evaluacion.Estado?.ToUpper() == "APROBADA")
                    return ResultDTO<bool>
                        .Failure($"La evaluación con Id {idEvaluacion} ya se encuentra aprobada.");

                evaluacion.Estado = "APROBADA";
                await _repo.UpdateAsync(evaluacion);

                return ResultDTO<bool>.Success(true,
                    $"Evaluación {idEvaluacion} aprobada exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aprobar evaluación con Id {Id}.", idEvaluacion);
                return ResultDTO<bool>.Failure("Error interno al aprobar la evaluación.");
            }
        }

        private static sbyte CalcularPuntuacionTotal(List<sbyte> puntuaciones)
        {
            if (!puntuaciones.Any()) return 0;
            return (sbyte)Math.Round(puntuaciones.Average(p => p));
        }

        private static List<string> ValidarCreateDTO(CreateEvaluacionDTO dto)
        {
            var errores = new List<string>();

            if (dto.EmpleadoId <= 0)
                errores.Add("EmpleadoId debe ser mayor a 0.");
            if (dto.EvaluadorId <= 0)
                errores.Add("EvaluadorId debe ser mayor a 0.");
            if (dto.EmpleadoId == dto.EvaluadorId)
                errores.Add("El evaluador no puede ser el mismo empleado evaluado.");
            if (dto.FechaInicio >= dto.FechaFin)
                errores.Add("FechaInicio debe ser anterior a FechaFin.");

            errores.AddRange(ValidarDetallesCreate(dto.Detalles));
            return errores;
        }

        private static List<string> ValidarUpdateDTO(UpdateEvaluacionDTO dto)
        {
            var errores = new List<string>();

            if (dto.EvaluadorId <= 0)
                errores.Add("EvaluadorId debe ser mayor a 0.");
            if (dto.FechaInicio >= dto.FechaFin)
                errores.Add("FechaInicio debe ser anterior a FechaFin.");
            if (!EstadosPermitidos.Contains(dto.Estado))
                errores.Add($"Estado inválido. Valores permitidos: {string.Join(", ", EstadosPermitidos)}.");

            errores.AddRange(ValidarDetallesUpdate(dto.Detalles));
            return errores;
        }

        private static List<string> ValidarDetallesCreate(List<CreateDetalleEvaluacionDTO> detalles)
        {
            var errores = new List<string>();
            var metricas = new HashSet<int>();

            for (int i = 0; i < detalles.Count; i++)
            {
                var d = detalles[i];
                if (d.IdMetrica <= 0)
                    errores.Add($"Detalle[{i}]: IdMetrica debe ser mayor a 0.");
                if (d.Puntuacion < 0 || d.Puntuacion > 100)
                    errores.Add($"Detalle[{i}]: Puntuacion debe estar entre 0 y 100.");
                if (!metricas.Add(d.IdMetrica))
                    errores.Add($"Detalle[{i}]: La métrica {d.IdMetrica} está duplicada.");
            }
            return errores;
        }

        private static List<string> ValidarDetallesUpdate(List<UpdateDetalleEvaluacionDTO> detalles)
        {
            var errores = new List<string>();
            var metricas = new HashSet<int>();

            for (int i = 0; i < detalles.Count; i++)
            {
                var d = detalles[i];
                if (d.IdMetrica <= 0)
                    errores.Add($"Detalle[{i}]: IdMetrica debe ser mayor a 0.");
                if (d.Puntuacion < 0 || d.Puntuacion > 100)
                    errores.Add($"Detalle[{i}]: Puntuacion debe estar entre 0 y 100.");
                if (!metricas.Add(d.IdMetrica))
                    errores.Add($"Detalle[{i}]: La métrica {d.IdMetrica} está duplicada.");
            }
            return errores;
        }

        private static EvaluacionResponseDTO MapToResponseDTO(EvaluacionesRendimiento e)
        {
            return new EvaluacionResponseDTO
            {
                IdEvaluacion = e.IdEvaluacion,
                EmpleadoId = e.EmpleadoId,
                NombreEmpleado = $"{e.Empleado?.Nombre} {e.Empleado?.PrimerApellido}".Trim(),
                FechaInicio = e.FechaInicio,
                FechaFin = e.FechaFin,
                EvaluadorId = e.EvaluadorId,
                NombreEvaluador = $"{e.Evaluador?.Nombre} {e.Evaluador?.PrimerApellido}".Trim(),
                PuntuacionTotal = e.PuntuacionTotal,
                Comentarios = e.Comentarios,
                Estado = e.Estado,
                FechaCreacion = e.FechaCreacion,
                FechaModificacion = e.FechaModificacion,
                Detalles = e.DetalleEvaluaciones.Select(d => new DetalleEvaluacionResponseDTO
                {
                    IdDetalle = d.IdDetalle,
                    IdEvaluacion = d.IdEvaluacion,
                    IdMetrica = d.IdMetrica,
                    NombreMetrica = d.IdMetricaNavigation?.NombreMetrica ?? string.Empty,
                    Puntuacion = d.Puntuacion,
                    Comentarios = d.Comentarios,
                    FechaCreacion = d.FechaCreacion,
                    FechaModificacion = d.FechaModificacion
                }).ToList()
            };
        }
    }
}