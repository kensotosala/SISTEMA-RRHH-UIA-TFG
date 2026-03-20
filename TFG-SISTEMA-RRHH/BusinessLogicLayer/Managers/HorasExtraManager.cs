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
        private readonly IAuditoriaService _auditoria;

        public HorasExtrasManager(
            IHorasExtrasRepository horasExtrasRepo,
            IEmpleadosRepository empleadosRepo,
            IAuditoriaService auditoria)
        {
            _horasExtrasRepo = horasExtrasRepo;
            _empleadosRepo = empleadosRepo;
            _auditoria = auditoria;
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
                filtros.JefeApruebaId);

            return horasExtras.Select(MapToDTO);
        }

        public async Task<IEnumerable<HoraExtraDTO>> GetByEmpleadoAsync(int empleadoId)
        {
            await ValidarEmpleadoExisteAsync(empleadoId);
            var horasExtras = await _horasExtrasRepo.GetByEmpleadoAsync(empleadoId);
            return horasExtras.Select(MapToDTO);
        }

        public async Task<IEnumerable<HoraExtraDTO>> GetPendientesByJefeAsync(int jefeId)
        {
            var jefe = await _empleadosRepo.GetByIdAsync(jefeId);
            if (jefe == null)
                throw new BusinessException("Jefe no encontrado", "JEFE_NO_ENCONTRADO");

            var horasExtras = await _horasExtrasRepo.GetPendientesByJefeAsync(jefeId);
            return horasExtras.Select(MapToDTO);
        }

        public async Task<HoraExtraDTO> CreateAsync(CrearHoraExtraDTO dto)
        {
            var (fechaInicio, fechaFin) = ParsearFechas(dto.FechaInicio, dto.FechaFin);

            ValidarRangoFechas(fechaInicio, fechaFin);

            await ValidarEmpleadoExisteAsync(dto.EmpleadoId);
            await ValidarJefeExisteAsync(dto.JefeApruebaId);
            await ValidarSolapamientoAsync(dto.EmpleadoId, fechaInicio, fechaFin);

            var fechaActual = DateTime.UtcNow;

            var horaExtra = new HorasExtras
            {
                EmpleadoId = dto.EmpleadoId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                TipoHoraExtra = dto.TipoHoraExtra,
                Motivo = dto.Motivo.Trim(),
                EstadoSolicitud = "PENDIENTE",
                JefeApruebaId = dto.JefeApruebaId,
                FechaSolicitud = fechaActual,
                FechaCreacion = fechaActual
            };

            var creada = await _horasExtrasRepo.CreateAsync(horaExtra);
            var registroCreado = await _horasExtrasRepo.GetByIdAsync(creada.IdHoraExtra);

            if (registroCreado == null)
                throw new BusinessException("Error al recuperar el registro creado", "ERROR_CREACION");

            await _auditoria.RegistrarAsync(
                tablaAfectada: "horas_extras",
                descripcion: $"Solicitud de hora extra creada (ID {creada.IdHoraExtra}) " +
                               $"para empleado ID {dto.EmpleadoId}, " +
                               $"período: {fechaInicio:dd/MM/yyyy HH:mm} - {fechaFin:dd/MM/yyyy HH:mm}, " +
                               $"tipo: {dto.TipoHoraExtra}, jefe aprueba ID: {dto.JefeApruebaId}."
            );

            return MapToDTO(registroCreado);
        }

        public async Task<bool> UpdateAsync(int id, ActualizarHoraExtraDTO dto)
        {
            var horaExtra = await _horasExtrasRepo.GetByIdAsync(id);
            if (horaExtra == null)
                throw new BusinessException("Registro de hora extra no encontrado", "HORA_EXTRA_NO_ENCONTRADA");

            if (horaExtra.EstadoSolicitud != "PENDIENTE")
                throw new BusinessException("Solo se pueden editar solicitudes pendientes", "SOLICITUD_NO_EDITABLE");

            await ValidarEmpleadoExisteAsync(dto.EmpleadoId);

            var (fechaInicio, fechaFin) = ParsearFechas(dto.FechaInicio, dto.FechaFin);

            ValidarRangoFechas(fechaInicio, fechaFin);
            await ValidarJefeExisteAsync(dto.JefeApruebaId);
            await ValidarSolapamientoAsync(dto.EmpleadoId, fechaInicio, fechaFin, id);

            horaExtra.EmpleadoId = dto.EmpleadoId;
            horaExtra.FechaInicio = fechaInicio;
            horaExtra.FechaFin = fechaFin;
            horaExtra.TipoHoraExtra = dto.TipoHoraExtra;
            horaExtra.Motivo = dto.Motivo.Trim();
            horaExtra.JefeApruebaId = dto.JefeApruebaId;
            horaExtra.FechaModificacion = DateTime.UtcNow;

            var resultado = await _horasExtrasRepo.UpdateAsync(horaExtra);

            if (resultado)
                await _auditoria.RegistrarAsync(
                    tablaAfectada: "horas_extras",
                    descripcion: $"Hora extra ID {id} actualizada. " +
                                   $"Empleado ID {dto.EmpleadoId}, " +
                                   $"nuevo período: {fechaInicio:dd/MM/yyyy HH:mm} - {fechaFin:dd/MM/yyyy HH:mm}, " +
                                   $"tipo: {dto.TipoHoraExtra}."
                );

            return resultado;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var horaExtra = await _horasExtrasRepo.GetByIdAsync(id);
            if (horaExtra == null)
                throw new BusinessException("Registro de hora extra no encontrado", "HORA_EXTRA_NO_ENCONTRADA");

            if (horaExtra.EstadoSolicitud == "APROBADA")
                throw new BusinessException("No se pueden eliminar solicitudes aprobadas", "SOLICITUD_NO_ELIMINABLE");

            var empleadoId = horaExtra.EmpleadoId;
            var fechaInicio = horaExtra.FechaInicio;

            var resultado = await _horasExtrasRepo.DeleteAsync(id);

            if (resultado)
                await _auditoria.RegistrarAsync(
                    tablaAfectada: "horas_extras",
                    descripcion: $"Hora extra ID {id} eliminada. " +
                                   $"Empleado ID {empleadoId}, " +
                                   $"fecha inicio: {fechaInicio:dd/MM/yyyy HH:mm}."
                );

            return resultado;
        }

        public async Task<bool> AprobarRechazarAsync(int id, AprobarRechazarHoraExtraDTO dto)
        {
            var horaExtra = await _horasExtrasRepo.GetByIdAsync(id);
            if (horaExtra == null)
                throw new BusinessException("Registro de hora extra no encontrado", "HORA_EXTRA_NO_ENCONTRADA");

            if (horaExtra.EstadoSolicitud != "PENDIENTE")
                throw new BusinessException("Solo se pueden aprobar/rechazar solicitudes pendientes", "SOLICITUD_YA_PROCESADA");

            var jefe = await _empleadosRepo.GetByIdAsync(dto.JefeApruebaId);
            if (jefe == null)
                throw new BusinessException("Jefe no encontrado", "JEFE_NO_ENCONTRADO");

            horaExtra.EstadoSolicitud = dto.EstadoSolicitud;
            horaExtra.JefeApruebaId = dto.JefeApruebaId;
            horaExtra.FechaAprobacion = DateTime.UtcNow;
            horaExtra.FechaModificacion = DateTime.UtcNow;

            var resultado = await _horasExtrasRepo.UpdateAsync(horaExtra);

            if (resultado)
                await _auditoria.RegistrarAsync(
                    tablaAfectada: "horas_extras",
                    descripcion: $"Hora extra ID {id} {dto.EstadoSolicitud.ToLower()} " +
                                   $"por jefe ID {dto.JefeApruebaId} " +
                                   $"({jefe.Nombre} {jefe.PrimerApellido}). " +
                                   $"Empleado ID {horaExtra.EmpleadoId}."
                );

            return resultado;
        }

        public async Task<ReporteHorasExtrasDTO> GetReporteEmpleadoAsync(
            int empleadoId, DateTime fechaInicio, DateTime fechaFin)
        {
            var empleado = await _empleadosRepo.GetByIdAsync(empleadoId);
            if (empleado == null)
                throw new BusinessException("Empleado no encontrado", "EMPLEADO_NO_ENCONTRADO");

            var lista = (await _horasExtrasRepo.GetByFiltrosAsync(
                empleadoId, fechaInicio, fechaFin, null, null, null)).ToList();

            var totalHorasAprobadas = lista.Where(h => h.EstadoSolicitud == "APROBADA")
                                            .Sum(h => (h.FechaFin - h.FechaInicio).TotalHours);
            var totalHorasSolicitadas = lista.Sum(h => (h.FechaFin - h.FechaInicio).TotalHours);

            return new ReporteHorasExtrasDTO
            {
                EmpleadoId = empleadoId,
                NombreCompleto = $"{empleado.Nombre} {empleado.PrimerApellido}".Trim(),
                Departamento = empleado.Departamento?.NombreDepartamento ?? "N/A",
                TotalSolicitudes = lista.Count,
                SolicitudesPendientes = lista.Count(h => h.EstadoSolicitud == "PENDIENTE"),
                SolicitudesAprobadas = lista.Count(h => h.EstadoSolicitud == "APROBADA"),
                SolicitudesRechazadas = lista.Count(h => h.EstadoSolicitud == "RECHAZADA"),
                TotalHorasAprobadas = TimeSpan.FromHours(totalHorasAprobadas),
                TotalHorasSolicitadas = TimeSpan.FromHours(totalHorasSolicitadas)
            };
        }

        public async Task<HoraExtraHoyDTO> ObtenerHoraExtraHoyAsync(int empleadoId)
        {
            if (empleadoId == 0)
                return new HoraExtraHoyDTO { TieneHoraExtra = false };

            var hoy = DateTime.Today;

            var horas = await _horasExtrasRepo.GetByEmpleadoAsync(empleadoId);

            var horaExtraHoy = horas
                .Where(h => h.EstadoSolicitud == "APROBADA")
                .FirstOrDefault(h => h.FechaInicio.Date <= hoy && h.FechaFin.Date >= hoy);

            return horaExtraHoy != null
                ? new HoraExtraHoyDTO
                {
                    TieneHoraExtra = true,
                    Inicio = horaExtraHoy.FechaInicio,
                    Fin = horaExtraHoy.FechaFin
                }
                : new HoraExtraHoyDTO { TieneHoraExtra = false };
        }

        public async Task<HoraExtraDTO?> ObtenerHoraExtraActivaAsync(int empleadoId, DateTime fechaHora)
        {
            var horas = await _horasExtrasRepo.GetByEmpleadoAsync(empleadoId);

            var activa = horas.FirstOrDefault(h =>
                h.EstadoSolicitud == "APROBADA" &&
                h.FechaInicio <= fechaHora &&
                h.FechaFin >= fechaHora);

            return activa != null ? MapToDTO(activa) : null;
        }

        #region Métodos privados

        private static (DateTime inicio, DateTime fin) ParsearFechas(string fechaInicioStr, string fechaFinStr)
        {
            try
            {
                if (!DateTime.TryParse(fechaInicioStr, CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var inicio))
                    throw new BusinessException("El formato de la fecha de inicio no es válido", "FORMATO_FECHA_INVALIDO");

                if (!DateTime.TryParse(fechaFinStr, CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var fin))
                    throw new BusinessException("El formato de la fecha de fin no es válido", "FORMATO_FECHA_INVALIDO");

                return (
                    DateTime.SpecifyKind(inicio, DateTimeKind.Utc),
                    DateTime.SpecifyKind(fin, DateTimeKind.Utc)
                );
            }
            catch (BusinessException) { throw; }
            catch (Exception)
            {
                throw new BusinessException("Error al procesar las fechas proporcionadas", "ERROR_PARSEO_FECHAS");
            }
        }

        private static void ValidarRangoFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            if (fechaFin <= fechaInicio)
                throw new BusinessException("La fecha de fin debe ser posterior a la de inicio", "FECHAS_INVALIDAS");

            var limiteMaximo = DateTime.UtcNow.AddMonths(3);
            if (fechaInicio > limiteMaximo || fechaFin > limiteMaximo)
                throw new BusinessException(
                    "No se pueden solicitar horas extras con más de 3 meses de anticipación",
                    "FECHA_FUERA_LIMITE");
        }

        private async Task ValidarEmpleadoExisteAsync(int empleadoId)
        {
            var empleado = await _empleadosRepo.GetByIdAsync(empleadoId);
            if (empleado == null)
                throw new BusinessException("Empleado no encontrado", "EMPLEADO_NO_ENCONTRADO");
        }

        private async Task ValidarJefeExisteAsync(int? jefeId)
        {
            if (!jefeId.HasValue) return;
            var jefe = await _empleadosRepo.GetByIdAsync(jefeId.Value);
            if (jefe == null)
                throw new BusinessException("Jefe no encontrado", "JEFE_NO_ENCONTRADO");
        }

        private async Task ValidarSolapamientoAsync(
            int empleadoId, DateTime fechaInicio, DateTime fechaFin, int? excludeId = null)
        {
            var tieneSolapamiento = await _horasExtrasRepo.TieneSolapamientoAsync(
                empleadoId, fechaInicio, fechaFin, excludeId);

            if (tieneSolapamiento)
                throw new BusinessException(
                    "Ya existe una solicitud de horas extras que se cruza con este horario",
                    "SOLAPAMIENTO_FECHAS");
        }

        private HoraExtraDTO MapToDTO(HorasExtras horaExtra)
        {
            if (horaExtra.Empleado == null)
                throw new BusinessException("Los datos del empleado no están cargados", "DATOS_INCOMPLETOS");

            return new HoraExtraDTO
            {
                IdHoraExtra = horaExtra.IdHoraExtra,
                EmpleadoId = horaExtra.EmpleadoId,
                CodigoEmpleado = horaExtra.Empleado.CodigoEmpleado,
                NombreEmpleado = $"{horaExtra.Empleado.Nombre} {horaExtra.Empleado.PrimerApellido}".Trim(),
                FechaSolicitud = horaExtra.FechaSolicitud,
                FechaInicio = horaExtra.FechaInicio,
                FechaFin = horaExtra.FechaFin,
                HorasTotales = horaExtra.FechaFin - horaExtra.FechaInicio,
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

        #endregion Métodos privados
    }
}