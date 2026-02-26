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

        public IncapacidadesManager(IIncapacidadesRepository repoIncapacidades, NotificacionesManager notificacionesManager)
        {
            _repoIncapacidades = repoIncapacidades ??
                throw new ArgumentNullException(nameof(repoIncapacidades));
            _notificacionesManager = notificacionesManager;
        }

        public async Task<IncapacidadDto> ActualizarIncapacidadAsync(ActualizarIncapacidadDto dto)
        {
            // 1. Validar DTO
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.IncapacidadId <= 0)
                throw new ArgumentException("El ID de la incapacidad es requerido", nameof(dto.IncapacidadId));

            // 2. Obtener la entidad existente
            var incapacidadExistente = await _repoIncapacidades.ListarIncapacidadPorId(dto.IncapacidadId);

            if (incapacidadExistente == null)
                throw new KeyNotFoundException($"No se encontró la incapacidad con ID {dto.IncapacidadId}");

            // 3. Validaciones de negocio
            if (dto.FechaFin < dto.FechaInicio)
                throw new InvalidOperationException("La fecha fin no puede ser menor a la fecha de inicio");

            // Validar que el TipoIncapacidad sea válido
            if (!Enum.TryParse<TipoIncapacidad>(dto.TipoIncapacidad, true, out _))
            {
                throw new ArgumentException(
                    $"El tipo de incapacidad '{dto.TipoIncapacidad}' no es válido. " +
                    $"Valores permitidos: ENFERMEDAD, ACCIDENTE, MATERNIDAD, PATERNIDAD",
                    nameof(dto.TipoIncapacidad));
            }

            // 4. Actualizar campos
            incapacidadExistente.EmpleadoId = dto.EmpleadoId;
            incapacidadExistente.FechaInicio = dto.FechaInicio;
            incapacidadExistente.FechaFin = dto.FechaFin;
            incapacidadExistente.TipoIncapacidad = dto.TipoIncapacidad.ToUpper();
            incapacidadExistente.Diagnostico = dto.Diagnostico;

            if (!string.IsNullOrEmpty(dto.ArchivoAdjunto))
                incapacidadExistente.ArchivoAdjunto = dto.ArchivoAdjunto;

            incapacidadExistente.FechaModificacion = DateTime.Now;

            // 5. Persistir cambios
            var resultado = await _repoIncapacidades.ActualizarIncapacidadAsync(incapacidadExistente);

            var dias = (dto.FechaFin - dto.FechaInicio).Days + 1;
            var tipoLabel = ObtenerLabelTipo(dto.TipoIncapacidad);

            var detalles = $@"
                <p><strong>Tipo:</strong> {tipoLabel}</p>
                <p><strong>Diagnóstico:</strong> {dto.Diagnostico}</p>
                <p><strong>Fecha de Inicio:</strong> {dto.FechaInicio:dd/MM/yyyy}</p>
                <p><strong>Fecha de Fin:</strong> {dto.FechaFin:dd/MM/yyyy}</p>
                <p><strong>Total de Días:</strong> {dias} día(s)</p>
                <p><strong>Estado:</strong> ACTIVA</p>
            ";

            await _notificacionesManager.NotificarSolicitudCreadaAsync(
                dto.EmpleadoId,
                "Incapacidad",
                detalles
            );

            // 6. Retornar DTO actualizado
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

            var dias = (incapacidadExistente.FechaFin - incapacidadExistente.FechaInicio).Days + 1;
            var tipoLabel = ObtenerLabelTipo(incapacidadExistente.TipoIncapacidad);

            var detalles = $@"
                <p><strong>Tipo:</strong> {tipoLabel}</p>
                <p><strong>Diagnóstico:</strong> {incapacidadExistente.Diagnostico}</p>
                <p><strong>Fecha de Inicio:</strong> {incapacidadExistente.FechaInicio:dd/MM/yyyy}</p>
                <p><strong>Fecha de Fin:</strong> {incapacidadExistente.FechaFin:dd/MM/yyyy}</p>
                <p><strong>Total de Días:</strong> {dias} día(s)</p>
                <p><strong>Fecha de Cancelación:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
            ";

            await _notificacionesManager.NotificarSolicitudCanceladaAsync(
                incapacidadExistente.EmpleadoId,
                "Incapacidad",
                detalles
            );

            await _repoIncapacidades.EliminarIncapacidadAsync(id);

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
            // 1. Validaciones
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.FechaFin < dto.FechaInicio)
                throw new InvalidOperationException("La fecha fin no puede ser menor a la fecha de inicio");

            // Validar que el TipoIncapacidad sea válido
            if (!Enum.TryParse<TipoIncapacidad>(dto.TipoIncapacidad, true, out _))
            {
                throw new ArgumentException(
                    $"El tipo de incapacidad '{dto.TipoIncapacidad}' no es válido. " +
                    $"Valores permitidos: ENFERMEDAD, ACCIDENTE, MATERNIDAD, PATERNIDAD",
                    nameof(dto.TipoIncapacidad));
            }

            // 2. Crear entidad
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

            // 3. Guardar
            var incapacidadGuardada = await _repoIncapacidades.RegistarIncapacidadesAsync(nuevaIncapacidad);

            if (incapacidadGuardada == null)
                throw new InvalidOperationException("Error al registrar la incapacidad");

            // Enviar notificacion
            var dias = (dto.FechaFin - dto.FechaInicio).Days + 1;
            var tipoLabel = ObtenerLabelTipo(dto.TipoIncapacidad);

            var detalles = $@"
                <p><strong>Tipo:</strong> {tipoLabel}</p>
                <p><strong>Diagnóstico:</strong> {dto.Diagnostico}</p>
                <p><strong>Fecha de Inicio:</strong> {dto.FechaInicio:dd/MM/yyyy}</p>
                <p><strong>Fecha de Fin:</strong> {dto.FechaFin:dd/MM/yyyy}</p>
                <p><strong>Total de Días:</strong> {dias} día(s)</p>
                <p><strong>Estado:</strong> ACTIVA</p>
            ";

            await _notificacionesManager.NotificarSolicitudCreadaAsync(
                dto.EmpleadoId,
                "Incapacidad",
                detalles
            );

            // 4. Retornar DTO
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