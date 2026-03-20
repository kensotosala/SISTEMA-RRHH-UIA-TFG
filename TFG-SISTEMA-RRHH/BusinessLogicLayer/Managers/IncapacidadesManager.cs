using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Shared;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;

namespace BusinessLogicLayer.Managers
{
    public class IncapacidadesManager : IIncapacidadesManager
    {
        private readonly IIncapacidadesRepository _repoIncapacidades;
        private readonly NotificacionesManager _notificacionesManager;
        private readonly IAuditoriaService _auditoria;

        public IncapacidadesManager(IIncapacidadesRepository repoIncapacidades, NotificacionesManager notificacionesManager, IAuditoriaService auditoria)
        {
            _repoIncapacidades = repoIncapacidades ??
                throw new ArgumentNullException(nameof(repoIncapacidades));
            _notificacionesManager = notificacionesManager;
            _auditoria = auditoria;
        }

        public async Task<IncapacidadDto> ActualizarIncapacidadAsync(ActualizarIncapacidadDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.IncapacidadId <= 0)
                throw new ArgumentException("El ID de la incapacidad es requerido", nameof(dto.IncapacidadId));

            var incapacidadExistente = await _repoIncapacidades.ListarIncapacidadPorId(dto.IncapacidadId);

            if (incapacidadExistente == null)
                throw new KeyNotFoundException($"No se encontró la incapacidad con ID {dto.IncapacidadId}");

            if (dto.FechaFin < dto.FechaInicio)
                throw new InvalidOperationException("La fecha fin no puede ser menor a la fecha de inicio");

            if (!Enum.TryParse<TipoIncapacidad>(dto.TipoIncapacidad, true, out _))
                throw new ArgumentException(
                    $"El tipo de incapacidad '{dto.TipoIncapacidad}' no es válido. " +
                    $"Valores permitidos: ENFERMEDAD, ACCIDENTE, MATERNIDAD, PATERNIDAD",
                    nameof(dto.TipoIncapacidad));

            var tipoAnterior = incapacidadExistente.TipoIncapacidad;

            incapacidadExistente.EmpleadoId = dto.EmpleadoId;
            incapacidadExistente.FechaInicio = dto.FechaInicio;
            incapacidadExistente.FechaFin = dto.FechaFin;
            incapacidadExistente.TipoIncapacidad = dto.TipoIncapacidad.ToUpper();
            incapacidadExistente.Diagnostico = dto.Diagnostico;
            incapacidadExistente.FechaModificacion = DateTime.Now;

            if (!string.IsNullOrEmpty(dto.ArchivoAdjunto))
                incapacidadExistente.ArchivoAdjunto = dto.ArchivoAdjunto;

            await _repoIncapacidades.ActualizarIncapacidadAsync(incapacidadExistente);

            await _auditoria.RegistrarAsync(
                tablaAfectada: "incapacidades",
                descripcion: $"Incapacidad ID {dto.IncapacidadId} actualizada. " +
                               $"Empleado ID {dto.EmpleadoId}, " +
                               $"tipo anterior: {ObtenerLabelTipo(tipoAnterior)}, " +
                               $"tipo nuevo: {ObtenerLabelTipo(dto.TipoIncapacidad)}, " +
                               $"período: {dto.FechaInicio:dd/MM/yyyy} - {dto.FechaFin:dd/MM/yyyy}."
            );

            var dias = (dto.FechaFin - dto.FechaInicio).Days + 1;
            var tipoLabel = ObtenerLabelTipo(dto.TipoIncapacidad);

            await _notificacionesManager.NotificarSolicitudCreadaAsync(
                dto.EmpleadoId, "Incapacidad",
                $@"<p><strong>Tipo:</strong> {tipoLabel}</p>
                   <p><strong>Diagnóstico:</strong> {dto.Diagnostico}</p>
                   <p><strong>Fecha de Inicio:</strong> {dto.FechaInicio:dd/MM/yyyy}</p>
                   <p><strong>Fecha de Fin:</strong> {dto.FechaFin:dd/MM/yyyy}</p>
                   <p><strong>Total de Días:</strong> {dias} día(s)</p>
                   <p><strong>Estado:</strong> ACTIVA</p>"
            );

            return new IncapacidadDto
            {
                IdIncapacidad = incapacidadExistente.IdIncapacidad,
                EmpleadoId = incapacidadExistente.EmpleadoId,
                FechaInicio = incapacidadExistente.FechaInicio,
                FechaFin = incapacidadExistente.FechaFin,
                TipoIncapacidad = incapacidadExistente.TipoIncapacidad,
                Diagnostico = incapacidadExistente.Diagnostico,
                ArchivoAdjunto = incapacidadExistente.ArchivoAdjunto,
                Estado = incapacidadExistente.Estado ?? EstadoIncapacidad.ACTIVA.ToString(),
                FechaCreacion = incapacidadExistente.FechaCreacion ?? DateTime.Now,
                FechaModificacion = incapacidadExistente.FechaModificacion
            };
        }

        public async Task<bool> EliminarIncapacidad(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a 0", nameof(id));

            var incapacidadExistente = await _repoIncapacidades.ListarIncapacidadPorId(id);

            if (incapacidadExistente == null)
                return false;

            var empleadoId = incapacidadExistente.EmpleadoId;
            var tipo = incapacidadExistente.TipoIncapacidad;
            var fechaInicio = incapacidadExistente.FechaInicio;
            var fechaFin = incapacidadExistente.FechaFin;

            var dias = (fechaFin - fechaInicio).Days + 1;
            var tipoLabel = ObtenerLabelTipo(tipo);

            await _notificacionesManager.NotificarSolicitudCanceladaAsync(
                empleadoId, "Incapacidad",
                $@"<p><strong>Tipo:</strong> {tipoLabel}</p>
                   <p><strong>Diagnóstico:</strong> {incapacidadExistente.Diagnostico}</p>
                   <p><strong>Fecha de Inicio:</strong> {fechaInicio:dd/MM/yyyy}</p>
                   <p><strong>Fecha de Fin:</strong> {fechaFin:dd/MM/yyyy}</p>
                   <p><strong>Total de Días:</strong> {dias} día(s)</p>
                   <p><strong>Fecha de Cancelación:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>"
            );

            await _repoIncapacidades.EliminarIncapacidadAsync(id);

            await _auditoria.RegistrarAsync(
                tablaAfectada: "incapacidades",
                descripcion: $"Incapacidad ID {id} eliminada. " +
                               $"Empleado ID {empleadoId}, " +
                               $"tipo: {tipoLabel}, " +
                               $"período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}."
            );

            return true;
        }

        public async Task<IEnumerable<IncapacidadDto>> ListarIncapacidadesAsync()
        {
            var incapacidades = await _repoIncapacidades.ListarIncapacidadesAsync();

            var listaDtos = new List<IncapacidadDto>();

            foreach (var inc in incapacidades)
            {
                listaDtos.Add(new IncapacidadDto
                {
                    IdIncapacidad = inc.IdIncapacidad,
                    EmpleadoId = inc.EmpleadoId,
                    FechaInicio = inc.FechaInicio,
                    FechaFin = inc.FechaFin,
                    TipoIncapacidad = inc.TipoIncapacidad,
                    Diagnostico = inc.Diagnostico,
                    ArchivoAdjunto = inc.ArchivoAdjunto,
                    Estado = inc.Estado ?? EstadoIncapacidad.ACTIVA.ToString(),
                    FechaCreacion = inc.FechaCreacion ?? DateTime.Now,
                    FechaModificacion = inc.FechaModificacion
                });
            }

            return listaDtos;
        }

        public async Task<IncapacidadDto?> ObtenerIncapacidadPorIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a 0", nameof(id));

            var incapacidad = await _repoIncapacidades.ListarIncapacidadPorId(id);

            if (incapacidad == null)
                return null;

            return new IncapacidadDto
            {
                IdIncapacidad = incapacidad.IdIncapacidad,
                EmpleadoId = incapacidad.EmpleadoId,
                FechaInicio = incapacidad.FechaInicio,
                FechaFin = incapacidad.FechaFin,
                TipoIncapacidad = incapacidad.TipoIncapacidad,
                Diagnostico = incapacidad.Diagnostico,
                ArchivoAdjunto = incapacidad.ArchivoAdjunto,
                Estado = incapacidad.Estado ?? EstadoIncapacidad.ACTIVA.ToString(),
                FechaCreacion = incapacidad.FechaCreacion ?? DateTime.Now,
                FechaModificacion = incapacidad.FechaModificacion
            };
        }

        public async Task<IncapacidadDto> RegistrarIncapacidad(RegistrarIncapacidadDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.FechaFin < dto.FechaInicio)
                throw new InvalidOperationException("La fecha fin no puede ser menor a la fecha de inicio");

            if (!Enum.TryParse<TipoIncapacidad>(dto.TipoIncapacidad, true, out _))
                throw new ArgumentException(
                    $"El tipo de incapacidad '{dto.TipoIncapacidad}' no es válido. " +
                    $"Valores permitidos: ENFERMEDAD, ACCIDENTE, MATERNIDAD, PATERNIDAD",
                    nameof(dto.TipoIncapacidad));

            var nuevaIncapacidad = new Incapacidades
            {
                EmpleadoId = dto.EmpleadoId,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                TipoIncapacidad = dto.TipoIncapacidad.ToUpper(),
                Diagnostico = dto.Diagnostico,
                ArchivoAdjunto = dto.ArchivoAdjunto,
                Estado = EstadoIncapacidad.ACTIVA.ToString(),
                FechaCreacion = DateTime.Now
            };

            var incapacidadGuardada = await _repoIncapacidades.RegistarIncapacidadesAsync(nuevaIncapacidad);

            if (incapacidadGuardada == null)
                throw new InvalidOperationException("Error al registrar la incapacidad");

            await _auditoria.RegistrarAsync(
                tablaAfectada: "incapacidades",
                descripcion: $"Incapacidad registrada (ID {incapacidadGuardada.IdIncapacidad}) " +
                               $"para empleado ID {dto.EmpleadoId}, " +
                               $"tipo: {ObtenerLabelTipo(dto.TipoIncapacidad)}, " +
                               $"período: {dto.FechaInicio:dd/MM/yyyy} - {dto.FechaFin:dd/MM/yyyy}."
            );

            var dias = (dto.FechaFin - dto.FechaInicio).Days + 1;
            var tipoLabel = ObtenerLabelTipo(dto.TipoIncapacidad);

            await _notificacionesManager.NotificarSolicitudCreadaAsync(
                dto.EmpleadoId, "Incapacidad",
                $@"<p><strong>Tipo:</strong> {tipoLabel}</p>
                   <p><strong>Diagnóstico:</strong> {dto.Diagnostico}</p>
                   <p><strong>Fecha de Inicio:</strong> {dto.FechaInicio:dd/MM/yyyy}</p>
                   <p><strong>Fecha de Fin:</strong> {dto.FechaFin:dd/MM/yyyy}</p>
                   <p><strong>Total de Días:</strong> {dias} día(s)</p>
                   <p><strong>Estado:</strong> ACTIVA</p>"
            );

            return new IncapacidadDto
            {
                IdIncapacidad = incapacidadGuardada.IdIncapacidad,
                EmpleadoId = incapacidadGuardada.EmpleadoId,
                FechaInicio = incapacidadGuardada.FechaInicio,
                FechaFin = incapacidadGuardada.FechaFin,
                TipoIncapacidad = incapacidadGuardada.TipoIncapacidad,
                Diagnostico = incapacidadGuardada.Diagnostico,
                ArchivoAdjunto = incapacidadGuardada.ArchivoAdjunto,
                Estado = incapacidadGuardada.Estado ?? EstadoIncapacidad.ACTIVA.ToString(),
                FechaCreacion = incapacidadGuardada.FechaCreacion ?? DateTime.Now,
                FechaModificacion = incapacidadGuardada.FechaModificacion
            };
        }

        private string ObtenerLabelTipo(string tipo)
        {
            return tipo switch
            {
                "ENFERMEDAD" => "Enfermedad",
                "ACCIDENTE" => "Accidente",
                "MATERNIDAD" => "Maternidad",
                "PATERNIDAD" => "Paternidad",
                _ => tipo
            };
        }
    }
}