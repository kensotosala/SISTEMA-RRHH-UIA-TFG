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

        public PermisosManager(IPermisosRepository repo, SistemaRhContext context, NotificacionesManager notificacionesManager)
        {
            _repo = repo;
            _context = context;
            _notificacionesManager = notificacionesManager;
        }

        public async Task<bool> ActualizarPermisoAsync(int id, ActualizarPermisoDTO dto)
        {
            var permiso = await _repo.GetPermisoByIdAsync(id);

            if (permiso == null)
                throw new BusinessException("Permiso no encontrado", "PERMISO_NO_ENCONTRADO");

            permiso.ConGoceSalario = dto.ConGoceSalario;
            permiso.EmpleadoId = dto.EmpleadoId;
            permiso.EstadoSolicitud = dto.EstadoSolicitud;
            permiso.FechaPermiso = dto.FechaPermiso;
            permiso.FechaAprobacion = dto.FechaAprobacion;
            permiso.Motivo = dto.Motivo;
            permiso.FechaModificacion = DateTime.Now;

            return await _repo.ActualizarPermisoAsync(permiso);
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

            // Detalles de la notificacion
            var detalles = $@"
                <p><strong>Fecha del Permiso:</strong> {dto.FechaPermiso:dd/MM/yyyy}</p>
                <p><strong>Motivo:</strong> {dto.Motivo}</p>
                <p><strong>Con Goce de Salario:</strong> {(dto.ConGoceSalario ?? false ? "Sí" : "No")}</p>
                <p><strong>Estado:</strong> PENDIENTE</p>
            ";

            // Notificar al empleado
            await _notificacionesManager.NotificarSolicitudCreadaAsync(
                dto.EmpleadoId,
                "Permiso",
                detalles
            );

            return new CrearPermisoDto
            {
                ConGoceSalario = registroCreado.ConGoceSalario,
                EmpleadoId = registroCreado.EmpleadoId,
                FechaPermiso = registroCreado.FechaPermiso,
                Motivo = registroCreado.Motivo,
            };
        }

        public async Task<bool> EliminarPermisoAsync(int id)
        {
            var permiso = await _repo.GetPermisoByIdAsync(id);

            if (permiso == null)
                return false;

            await _repo.DeletePermisoAsync(id);
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
            var permiso = await _repo.GetPermisoByIdAsync(id);
            if (permiso == null) throw new Exception("Permios no encontrado");

            permiso.EstadoSolicitud = "APROBADA";
            permiso.JefeApruebaId = jefeId;
            permiso.FechaAprobacion = DateTime.Now;

            await _repo.ActualizarPermisoAsync(permiso);

            // Enviar notificación
            var detalles = $@"
                <p><strong>Fecha del Permiso:</strong> {permiso.FechaPermiso:dd/MM/yyyy}</p>
                <p><strong>Motivo:</strong> {permiso.Motivo}</p>
                <p><strong>Fecha de Aprobación:</strong> {permiso.FechaAprobacion:dd/MM/yyyy HH:mm}</p>
            ";

            await _notificacionesManager.NotificarSolicitudAprobadaAsync(
                permiso.EmpleadoId,
                "Permiso",
                detalles
            );
        }

        public async Task CancelarPermisoAsync(int id)
        {
            var permiso = await _repo.GetPermisoByIdAsync(id);

            if (permiso == null)
                throw new Exception("Permiso no encontrado");

            await _repo.DeletePermisoAsync(id);

            // Enviar notificacion
            var detalles = $@"
                <p><strong>Fecha del Permiso:</strong> {permiso.FechaPermiso:dd/MM/yyyy}</p>
                <p><strong>Motivo:</strong> {permiso.Motivo}</p>
                <p><strong>Fecha de Cancelación:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
            ";

            await _notificacionesManager.NotificarSolicitudCanceladaAsync(permiso.EmpleadoId, "Permiso", detalles);
        }
    }
}