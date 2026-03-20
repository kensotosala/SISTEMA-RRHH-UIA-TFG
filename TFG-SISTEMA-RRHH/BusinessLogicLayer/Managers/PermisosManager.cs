using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Shared;
using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;

namespace BusinessLogicLayer.Managers
{
    public class PermisosManager : IPermisosManager
    {
        private readonly SistemaRhContext _context;
        private readonly IPermisosRepository _repo;
        private readonly NotificacionesManager _notificacionesManager;
        private readonly IAuditoriaService _auditoria;

        public PermisosManager(IPermisosRepository repo, SistemaRhContext context, NotificacionesManager notificacionesManager, IAuditoriaService auditoria)
        {
            _repo = repo;
            _context = context;
            _notificacionesManager = notificacionesManager;
            _auditoria = auditoria;
        }

        public async Task<bool> ActualizarPermisoAsync(int id, ActualizarPermisoDTO dto)
        {
            var permiso = await _repo.GetPermisoByIdAsync(id)
                ?? throw new BusinessException("Permiso no encontrado", "PERMISO_NO_ENCONTRADO");

            var estadoAnterior = permiso.EstadoSolicitud;

            permiso.ConGoceSalario = dto.ConGoceSalario;
            permiso.EmpleadoId = dto.EmpleadoId;
            permiso.EstadoSolicitud = dto.EstadoSolicitud;
            permiso.FechaPermiso = dto.FechaPermiso;
            permiso.FechaAprobacion = dto.FechaAprobacion;
            permiso.Motivo = dto.Motivo;
            permiso.FechaModificacion = DateTime.Now;

            var resultado = await _repo.ActualizarPermisoAsync(permiso);

            if (resultado)
                await _auditoria.RegistrarAsync(
                    tablaAfectada: "permisos",
                    descripcion: $"Permiso ID {id} actualizado. " +
                                   $"Empleado ID {dto.EmpleadoId}, " +
                                   $"estado anterior: '{estadoAnterior}', " +
                                   $"estado nuevo: '{dto.EstadoSolicitud}', " +
                                   $"fecha permiso: {dto.FechaPermiso:dd/MM/yyyy}."
                );

            return resultado;
        }

        public async Task<CrearPermisoDto> CrearPermisoAsync(CrearPermisoDto dto)
        {
            var permiso = new Permisos
            {
                ConGoceSalario = dto.ConGoceSalario,
                EmpleadoId = dto.EmpleadoId,
                EstadoSolicitud = EstadoSolicitud.PENDIENTE.ToString(),
                FechaPermiso = dto.FechaPermiso,
                FechaSolicitud = DateTime.Now,
                Motivo = dto.Motivo,
                FechaCreacion = DateTime.Now
            };

            await _repo.CreatePermisoAsync(permiso);

            var registroCreado = await _repo.GetPermisoByIdAsync(permiso.IdPermiso);

            if (registroCreado == null)
                throw new BusinessException("Error al crear el permiso", "PERMISO_NO_CREADO");

            // ✅ Auditar tras persistir exitosamente
            await _auditoria.RegistrarAsync(
                tablaAfectada: "permisos",
                descripcion: $"Permiso creado (ID {registroCreado.IdPermiso}) " +
                               $"para empleado ID {dto.EmpleadoId}, " +
                               $"fecha permiso: {dto.FechaPermiso:dd/MM/yyyy}, " +
                               $"con goce de salario: {(dto.ConGoceSalario ?? false ? "Sí" : "No")}."
            );

            await _notificacionesManager.NotificarSolicitudCreadaAsync(
                dto.EmpleadoId, "Permiso",
                $@"<p><strong>Fecha del Permiso:</strong> {dto.FechaPermiso:dd/MM/yyyy}</p>
                   <p><strong>Motivo:</strong> {dto.Motivo}</p>
                   <p><strong>Con Goce de Salario:</strong> {(dto.ConGoceSalario ?? false ? "Sí" : "No")}</p>
                   <p><strong>Estado:</strong> PENDIENTE</p>"
            );

            return new CrearPermisoDto
            {
                ConGoceSalario = registroCreado.ConGoceSalario,
                EmpleadoId = registroCreado.EmpleadoId,
                FechaPermiso = registroCreado.FechaPermiso,
                Motivo = registroCreado.Motivo
            };
        }

        public async Task<bool> EliminarPermisoAsync(int id)
        {
            var permiso = await _repo.GetPermisoByIdAsync(id);

            if (permiso == null) return false;

            var empleadoId = permiso.EmpleadoId;
            var fechaPermiso = permiso.FechaPermiso;

            await _repo.DeletePermisoAsync(id);

            await _auditoria.RegistrarAsync(
                tablaAfectada: "permisos",
                descripcion: $"Permiso ID {id} eliminado. " +
                               $"Empleado ID {empleadoId}, " +
                               $"fecha permiso: {fechaPermiso:dd/MM/yyyy}."
            );

            return true;
        }

        public async Task<ListarPermisoByIdDTO?> ListarPermisoByIdAsync(int id)
        {
            var permiso = await _repo.GetPermisoByIdAsync(id);

            if (permiso == null)
                return null;

            return new ListarPermisoByIdDTO
            {
                ComentariosRechazo = permiso.ComentariosRechazo,
                ConGoceSalario = permiso.ConGoceSalario,
                EmpleadoId = permiso.EmpleadoId,
                EstadoSolicitud = permiso.EstadoSolicitud,
                FechaAprobacion = permiso.FechaAprobacion,
                FechaPermiso = permiso.FechaPermiso,
                FechaCreacion = permiso.FechaCreacion,
                FechaModificacion = permiso.FechaModificacion,
                FechaSolicitud = permiso.FechaSolicitud,
                IdPermiso = permiso.IdPermiso,
                JefeApruebaId = permiso.JefeApruebaId,
                Motivo = permiso.Motivo,
            };
        }

        public async Task<IEnumerable<ListarPermisosDTO?>> ListarPermisosAsync()
        {
            var permisos = await _repo.GetAllPermisosAsync();

            if (permisos == null)
                return [];

            return permisos.Select(p => new ListarPermisosDTO
            {
                IdPermiso = p.IdPermiso,
                EmpleadoId = p.EmpleadoId,
                ConGoceSalario = p.ConGoceSalario,
                ComentariosRechazo = p.ComentariosRechazo,
                EstadoSolicitud = p.EstadoSolicitud,
                FechaPermiso = p.FechaPermiso,
                FechaSolicitud = p.FechaSolicitud,
                FechaCreacion = p.FechaCreacion,
                FechaModificacion = p.FechaModificacion,
                FechaAprobacion = p.FechaAprobacion,
                JefeApruebaId = p.JefeApruebaId,
                Motivo = p.Motivo
            }).ToList();
        }

        public async Task AprobarPermisoASync(int id, int jefeId)
        {
            var permiso = await _repo.GetPermisoByIdAsync(id)
                ?? throw new Exception("Permiso no encontrado");

            permiso.EstadoSolicitud = "APROBADA";
            permiso.JefeApruebaId = jefeId;
            permiso.FechaAprobacion = DateTime.Now;

            await _repo.ActualizarPermisoAsync(permiso);

            await _auditoria.RegistrarAsync(
                tablaAfectada: "permisos",
                descripcion: $"Permiso ID {id} aprobado por jefe ID {jefeId}. " +
                               $"Empleado ID {permiso.EmpleadoId}, " +
                               $"fecha permiso: {permiso.FechaPermiso:dd/MM/yyyy}."
            );

            await _notificacionesManager.NotificarSolicitudAprobadaAsync(
                permiso.EmpleadoId, "Permiso",
                $@"<p><strong>Fecha del Permiso:</strong> {permiso.FechaPermiso:dd/MM/yyyy}</p>
                   <p><strong>Motivo:</strong> {permiso.Motivo}</p>
                   <p><strong>Fecha de Aprobación:</strong> {permiso.FechaAprobacion:dd/MM/yyyy HH:mm}</p>"
            );
        }

        public async Task CancelarPermisoAsync(int id)
        {
            var permiso = await _repo.GetPermisoByIdAsync(id)
                ?? throw new Exception("Permiso no encontrado");

            var empleadoId = permiso.EmpleadoId;
            var fechaPermiso = permiso.FechaPermiso;
            var motivo = permiso.Motivo;

            await _repo.DeletePermisoAsync(id);

            await _auditoria.RegistrarAsync(
                tablaAfectada: "permisos",
                descripcion: $"Permiso ID {id} cancelado. " +
                               $"Empleado ID {empleadoId}, " +
                               $"fecha permiso: {fechaPermiso:dd/MM/yyyy}."
            );

            await _notificacionesManager.NotificarSolicitudCanceladaAsync(
                empleadoId, "Permiso",
                $@"<p><strong>Fecha del Permiso:</strong> {fechaPermiso:dd/MM/yyyy}</p>
                   <p><strong>Motivo:</strong> {motivo}</p>
                   <p><strong>Fecha de Cancelación:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>"
            );
        }

        public async Task<bool> AprobarRechazarPermisoAsync(int id, AprobarRechazarPermisoDTO dto)
        {
            var permiso = await _repo.GetPermisoByIdAsync(id)
                ?? throw new BusinessException("Permiso no encontrado", "PERMISO_NO_ENCONTRADO");

            if (permiso.EstadoSolicitud != EstadoSolicitud.PENDIENTE.ToString())
                throw new BusinessException("Solo se pueden aprobar o rechazar permisos pendientes", "ESTADO_INVALIDO");

            if (dto.EstadoSolicitud != EstadoSolicitud.APROBADA.ToString() &&
                dto.EstadoSolicitud != EstadoSolicitud.RECHAZADA.ToString())
                throw new BusinessException("Estado inválido. Use APROBADA o RECHAZADA", "ESTADO_INVALIDO");

            if (dto.EstadoSolicitud == EstadoSolicitud.RECHAZADA.ToString() &&
                string.IsNullOrWhiteSpace(dto.ComentariosRechazo))
                throw new BusinessException("Los comentarios son requeridos al rechazar", "COMENTARIOS_REQUERIDOS");

            if (!dto.JefeApruebaId.HasValue)
                throw new BusinessException("El ID del jefe aprobador es requerido", "JEFE_REQUERIDO");

            permiso.JefeApruebaId = dto.JefeApruebaId;
            permiso.EstadoSolicitud = dto.EstadoSolicitud;
            permiso.FechaAprobacion = DateTime.Now;
            permiso.FechaModificacion = DateTime.Now;
            permiso.ComentariosRechazo = dto.ComentariosRechazo;

            var resultado = await _repo.ActualizarPermisoAsync(permiso);

            if (resultado)
                await _auditoria.RegistrarAsync(
                    tablaAfectada: "permisos",
                    descripcion: $"Permiso ID {id} {dto.EstadoSolicitud.ToLower()} " +
                                   $"por jefe ID {dto.JefeApruebaId}. " +
                                   $"Empleado ID {permiso.EmpleadoId}, " +
                                   $"fecha permiso: {permiso.FechaPermiso:dd/MM/yyyy}" +
                                   (dto.EstadoSolicitud == EstadoSolicitud.RECHAZADA.ToString()
                                       ? $", comentarios: '{dto.ComentariosRechazo}'."
                                       : ".")
                );

            if (dto.EstadoSolicitud == EstadoSolicitud.APROBADA.ToString())
            {
                await _notificacionesManager.NotificarSolicitudAprobadaAsync(
                    permiso.EmpleadoId, "Permiso",
                    $@"<p><strong>Fecha del Permiso:</strong> {permiso.FechaPermiso:dd/MM/yyyy}</p>
                       <p><strong>Motivo:</strong> {permiso.Motivo}</p>
                       <p><strong>Fecha de Aprobación:</strong> {permiso.FechaAprobacion:dd/MM/yyyy HH:mm}</p>"
                );
            }
            else
            {
                await _notificacionesManager.NotificarSolicitudCanceladaAsync(
                    permiso.EmpleadoId, "Permiso",
                    $@"<p><strong>Fecha del Permiso:</strong> {permiso.FechaPermiso:dd/MM/yyyy}</p>
                       <p><strong>Motivo:</strong> {permiso.Motivo}</p>
                       <p><strong>Comentarios:</strong> {permiso.ComentariosRechazo}</p>
                       <p><strong>Fecha de Rechazo:</strong> {permiso.FechaAprobacion:dd/MM/yyyy HH:mm}</p>"
                );
            }

            return resultado;
        }
    }
}