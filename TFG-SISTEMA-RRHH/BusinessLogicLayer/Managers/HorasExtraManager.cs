using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Shared;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using System.Globalization;

namespace BusinessLogicLayer.Managers
{
    public class HorasExtrasManager : IHorasExtrasManager
    {
        private readonly IHorasExtrasRepository _horasExtrasRepo;
        private readonly IEmpleadosRepository _empleadosRepo;

        public HorasExtrasManager(
            IHorasExtrasRepository horasExtrasRepo,
            IEmpleadosRepository empleadosRepo)
        {
            _horasExtrasRepo = horasExtrasRepo;
            _empleadosRepo = empleadosRepo;
        }

        public async Task<IEnumerable<HoraExtraDTO>> GetAllAsync()
        {
            var horasExtras = await _horasExtrasRepo.GetAllAsync();
            return horasExtras.Select(MapToDTO);
        }

        public async Task<HoraExtraDTO?> GetByIdAsync(int id)
        {
            var horaExtra = await _horasExtrasRepo.GetByIdAsync(id);
            return horaExtra != null ? MapToDTO(horaExtra) : null;
        }

        public async Task<IEnumerable<HoraExtraDTO>> GetByFiltrosAsync(FiltrosHorasExtrasDTO filtros)
        {
            var horasExtras = await _horasExtrasRepo.GetByFiltrosAsync(
                filtros.EmpleadoId,
                filtros.FechaInicio,
                filtros.FechaFin,
                filtros.EstadoSolicitud,
                filtros.DepartamentoId,
                filtros.JefeApruebaId
            );

            return horasExtras.Select(MapToDTO);
        }

        public async Task<IEnumerable<HoraExtraDTO>> GetByEmpleadoAsync(int empleadoId)
        {
            var empleado = await _empleadosRepo.GetByIdAsync(empleadoId);
            if (empleado == null)
            {
                throw new BusinessException("Empleado no encontrado", "EMPLEADO_NO_ENCONTRADO");
            }

            var horasExtras = await _horasExtrasRepo.GetByEmpleadoAsync(empleadoId);
            return horasExtras.Select(MapToDTO);
        }

        public async Task<IEnumerable<HoraExtraDTO>> GetPendientesByJefeAsync(int jefeId)
        {
            var jefe = await _empleadosRepo.GetByIdAsync(jefeId);
            if (jefe == null)
            {
                throw new BusinessException("Jefe no encontrado", "JEFE_NO_ENCONTRADO");
            }

            var horasExtras = await _horasExtrasRepo.GetPendientesByJefeAsync(jefeId);
            return horasExtras.Select(MapToDTO);
        }

        public async Task<HoraExtraDTO> CreateAsync(CrearHoraExtraDTO dto)
        {
            DateTime fechaInicio, fechaFin;

            try
            {
                if (!DateTime.TryParse(dto.FechaInicio, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out fechaInicio))
                {
                    throw new BusinessException(
                        "El formato de la fecha de inicio no es válido",
                        "FORMATO_FECHA_INVALIDO");
                }

                if (!DateTime.TryParse(dto.FechaFin, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out fechaFin))
                {
                    throw new BusinessException(
                        "El formato de la fecha de fin no es válido",
                        "FORMATO_FECHA_INVALIDO");
                }

                fechaInicio = DateTime.SpecifyKind(fechaInicio, DateTimeKind.Utc);
                fechaFin = DateTime.SpecifyKind(fechaFin, DateTimeKind.Utc);
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new BusinessException(
                    "Error al procesar las fechas proporcionadas",
                    "ERROR_PARSEO_FECHAS");
            }

            if (fechaFin <= fechaInicio)
            {
                throw new BusinessException(
                    "La fecha de fin debe ser posterior a la de inicio",
                    "FECHAS_INVALIDAS");
            }

            var fechaActual = DateTime.UtcNow;
            var limiteMaximo = fechaActual.AddMonths(3);

            if (fechaInicio > limiteMaximo || fechaFin > limiteMaximo)
            {
                throw new BusinessException(
                    "No se pueden solicitar horas extras con más de 3 meses de anticipación",
                    "FECHA_FUERA_LIMITE");
            }

            var empleado = await _empleadosRepo.GetByIdAsync(dto.EmpleadoId);
            if (empleado == null)
            {
                throw new BusinessException(
                    "Empleado no encontrado",
                    "EMPLEADO_NO_ENCONTRADO");
            }

            if (dto.JefeApruebaId.HasValue)
            {
                var jefe = await _empleadosRepo.GetByIdAsync(dto.JefeApruebaId.Value);
                if (jefe == null)
                {
                    throw new BusinessException(
                        "Jefe no encontrado",
                        "JEFE_NO_ENCONTRADO");
                }
            }

            var tieneSolapamiento = await _horasExtrasRepo.TieneSolapamientoAsync(
                dto.EmpleadoId,
                fechaInicio,
                fechaFin);

            if (tieneSolapamiento)
            {
                throw new BusinessException(
                    "Ya existe una solicitud de horas extras que se cruza con este horario",
                    "SOLAPAMIENTO_FECHAS");
            }

            var horaExtra = new HorasExtras
            {
                EmpleadoId = dto.EmpleadoId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                TipoHoraExtra = "PENDIENTE",
                Motivo = dto.Motivo.Trim(),
                EstadoSolicitud = "PENDIENTE",
                JefeApruebaId = dto.JefeApruebaId,
                FechaSolicitud = fechaActual,
                FechaCreacion = fechaActual
            };

            var creada = await _horasExtrasRepo.CreateAsync(horaExtra);

            var registroCreado = await _horasExtrasRepo.GetByIdAsync(creada.IdHoraExtra);

            if (registroCreado == null)
            {
                throw new BusinessException(
                    "Error al recuperar el registro creado",
                    "ERROR_CREACION");
            }

            return MapToDTO(registroCreado);
        }

        public async Task<bool> UpdateAsync(int id, ActualizarHoraExtraDTO dto)
        {
            var horaExtra = await _horasExtrasRepo.GetByIdAsync(id);
            if (horaExtra == null)
            {
                throw new BusinessException(
                    "Registro de hora extra no encontrado",
                    "HORA_EXTRA_NO_ENCONTRADA");
            }

            if (horaExtra.EstadoSolicitud != "PENDIENTE")
            {
                throw new BusinessException(
                    "Solo se pueden editar solicitudes pendientes",
                    "SOLICITUD_NO_EDITABLE");
            }

            var empleado = await _empleadosRepo.GetByIdAsync(dto.EmpleadoId);
            if (empleado == null)
            {
                throw new BusinessException(
                    "Empleado no encontrado",
                    "EMPLEADO_NO_ENCONTRADO");
            }

            // Parsear fechas desde string
            DateTime fechaInicio, fechaFin;

            try
            {
                if (!DateTime.TryParse(dto.FechaInicio, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out fechaInicio))
                {
                    throw new BusinessException(
                        "El formato de la fecha de inicio no es válido",
                        "FORMATO_FECHA_INVALIDO");
                }

                if (!DateTime.TryParse(dto.FechaFin, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out fechaFin))
                {
                    throw new BusinessException(
                        "El formato de la fecha de fin no es válido",
                        "FORMATO_FECHA_INVALIDO");
                }

                fechaInicio = DateTime.SpecifyKind(fechaInicio, DateTimeKind.Utc);
                fechaFin = DateTime.SpecifyKind(fechaFin, DateTimeKind.Utc);
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new BusinessException(
                    "Error al procesar las fechas",
                    "ERROR_PARSEO_FECHAS");
            }

            if (fechaFin <= fechaInicio)
            {
                throw new BusinessException(
                    "La fecha de fin debe ser posterior a la fecha de inicio",
                    "FECHAS_INVALIDAS");
            }

            var tieneSolapamiento = await _horasExtrasRepo.TieneSolapamientoAsync(
                dto.EmpleadoId,
                fechaInicio,
                fechaFin,
                id);

            if (tieneSolapamiento)
            {
                throw new BusinessException(
                    "Ya existe una solicitud de horas extras en ese rango de fechas",
                    "SOLAPAMIENTO_FECHAS");
            }

            if (dto.JefeApruebaId.HasValue)
            {
                var jefe = await _empleadosRepo.GetByIdAsync(dto.JefeApruebaId.Value);
                if (jefe == null)
                {
                    throw new BusinessException(
                        "Jefe no encontrado",
                        "JEFE_NO_ENCONTRADO");
                }
            }

            horaExtra.EmpleadoId = dto.EmpleadoId;
            horaExtra.FechaInicio = fechaInicio;
            horaExtra.FechaFin = fechaFin;
            horaExtra.TipoHoraExtra = "PENDIENTE"; // ← No cambiar, mantener según BD
            horaExtra.Motivo = dto.Motivo.Trim();
            horaExtra.JefeApruebaId = dto.JefeApruebaId;
            horaExtra.FechaModificacion = DateTime.UtcNow;

            return await _horasExtrasRepo.UpdateAsync(horaExtra);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var horaExtra = await _horasExtrasRepo.GetByIdAsync(id);
            if (horaExtra == null)
            {
                throw new BusinessException(
                    "Registro de hora extra no encontrado",
                    "HORA_EXTRA_NO_ENCONTRADA");
            }

            if (horaExtra.EstadoSolicitud == "APROBADA")
            {
                throw new BusinessException(
                    "No se pueden eliminar solicitudes aprobadas",
                    "SOLICITUD_NO_ELIMINABLE");
            }

            return await _horasExtrasRepo.DeleteAsync(id);
        }

        public async Task<bool> AprobarRechazarAsync(int id, AprobarRechazarHoraExtraDTO dto)
        {
            var horaExtra = await _horasExtrasRepo.GetByIdAsync(id);
            if (horaExtra == null)
            {
                throw new BusinessException(
                    "Registro de hora extra no encontrado",
                    "HORA_EXTRA_NO_ENCONTRADA");
            }

            if (horaExtra.EstadoSolicitud != "PENDIENTE")
            {
                throw new BusinessException(
                    "Solo se pueden aprobar/rechazar solicitudes pendientes",
                    "SOLICITUD_YA_PROCESADA");
            }

            var jefe = await _empleadosRepo.GetByIdAsync(dto.JefeApruebaId);
            if (jefe == null)
            {
                throw new BusinessException(
                    "Jefe no encontrado",
                    "JEFE_NO_ENCONTRADO");
            }

            horaExtra.EstadoSolicitud = dto.EstadoSolicitud;
            horaExtra.JefeApruebaId = dto.JefeApruebaId;
            horaExtra.FechaAprobacion = DateTime.UtcNow;
            horaExtra.FechaModificacion = DateTime.UtcNow;

            return await _horasExtrasRepo.UpdateAsync(horaExtra);
        }

        public async Task<ReporteHorasExtrasDTO> GetReporteEmpleadoAsync(
            int empleadoId,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            var empleado = await _empleadosRepo.GetByIdAsync(empleadoId);
            if (empleado == null)
            {
                throw new BusinessException(
                    "Empleado no encontrado",
                    "EMPLEADO_NO_ENCONTRADO");
            }

            var horasExtras = await _horasExtrasRepo.GetByFiltrosAsync(
                empleadoId, fechaInicio, fechaFin, null, null, null);

            var lista = horasExtras.ToList();
            var totalSolicitudes = lista.Count;
            var pendientes = lista.Count(h => h.EstadoSolicitud == "PENDIENTE");
            var aprobadas = lista.Count(h => h.EstadoSolicitud == "APROBADA");
            var rechazadas = lista.Count(h => h.EstadoSolicitud == "RECHAZADA");

            var totalHorasAprobadas = lista
                .Where(h => h.EstadoSolicitud == "APROBADA")
                .Sum(h => (h.FechaFin - h.FechaInicio).TotalHours);

            var totalHorasSolicitadas = lista
                .Sum(h => (h.FechaFin - h.FechaInicio).TotalHours);

            return new ReporteHorasExtrasDTO
            {
                EmpleadoId = empleadoId,
                NombreCompleto = $"{empleado.Nombre} {empleado.PrimerApellido}".Trim(),
                Departamento = empleado.Departamento?.NombreDepartamento ?? "N/A",
                TotalSolicitudes = totalSolicitudes,
                SolicitudesPendientes = pendientes,
                SolicitudesAprobadas = aprobadas,
                SolicitudesRechazadas = rechazadas,
                TotalHorasAprobadas = TimeSpan.FromHours(totalHorasAprobadas),
                TotalHorasSolicitadas = TimeSpan.FromHours(totalHorasSolicitadas)
            };
        }

        private HoraExtraDTO MapToDTO(HorasExtras horaExtra)
        {
            if (horaExtra.Empleado == null)
            {
                throw new BusinessException(
                    "Los datos del empleado no están cargados",
                    "DATOS_INCOMPLETOS");
            }

            var horasTotales = horaExtra.FechaFin - horaExtra.FechaInicio;

            return new HoraExtraDTO
            {
                IdHoraExtra = horaExtra.IdHoraExtra,
                EmpleadoId = horaExtra.EmpleadoId,
                CodigoEmpleado = horaExtra.Empleado.CodigoEmpleado,
                NombreEmpleado = $"{horaExtra.Empleado.Nombre} {horaExtra.Empleado.PrimerApellido}".Trim(),
                FechaSolicitud = horaExtra.FechaSolicitud,
                FechaInicio = horaExtra.FechaInicio,
                FechaFin = horaExtra.FechaFin,
                HorasTotales = horasTotales,
                TipoHoraExtra = horaExtra.TipoHoraExtra,
                Motivo = horaExtra.Motivo,
                EstadoSolicitud = horaExtra.EstadoSolicitud ?? "PENDIENTE",
                JefeApruebaId = horaExtra.JefeApruebaId,
                NombreJefe = horaExtra.JefeAprueba != null
                    ? $"{horaExtra.JefeAprueba.Nombre} {horaExtra.JefeAprueba.PrimerApellido}".Trim()
                    : null,
                FechaAprobacion = horaExtra.FechaAprobacion,
                FechaCreacion = horaExtra.FechaCreacion
            };
        }
    }
}