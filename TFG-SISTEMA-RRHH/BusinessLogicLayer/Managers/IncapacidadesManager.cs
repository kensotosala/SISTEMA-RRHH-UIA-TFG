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

        public IncapacidadesManager(IIncapacidadesRepository repoIncapacidades)
        {
            _repoIncapacidades = repoIncapacidades ??
                throw new ArgumentNullException(nameof(repoIncapacidades));
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

            incapacidadExistente.FechaModificacion = DateTime.UtcNow;

            // 5. Persistir cambios
            await _repoIncapacidades.ActualizarIncapacidadAsync(incapacidadExistente);

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
                FechaCreacion = incapacidadExistente.FechaCreacion ?? DateTime.UtcNow,
                FechaModificacion = incapacidadExistente.FechaModificacion
            };
        }

        public async Task<bool> EliminarIncapacidad(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a 0", nameof(id));

            var existe = await _repoIncapacidades.ListarIncapacidadPorId(id);

            if (existe == null)
                return false;

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
                    FechaCreacion = inc.FechaCreacion ?? DateTime.UtcNow,
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
                FechaCreacion = incapacidad.FechaCreacion ?? DateTime.UtcNow,
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
                FechaCreacion = DateTime.UtcNow
            };

            // 3. Guardar
            var incapacidadGuardada = await _repoIncapacidades.RegistarIncapacidadesAsync(nuevaIncapacidad);

            if (incapacidadGuardada == null)
                throw new InvalidOperationException("Error al registrar la incapacidad");

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
                FechaCreacion = incapacidadGuardada.FechaCreacion ?? DateTime.UtcNow,
                FechaModificacion = incapacidadGuardada.FechaModificacion
            };
        }
    }
}