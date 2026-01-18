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

        public PermisosManager(IPermisosRepository repo, SistemaRhContext context)
        {
            _repo = repo;
            _context = context;
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
            permiso.FechaModificacion = DateTime.UtcNow;

            return await _repo.ActualizarPermisoAsync(permiso);
        }

        public async Task<CrearPermisoDTO> CrearPermisoAsync(CrearPermisoDTO dto)
        {
            var permiso = new Permisos
            {
                ConGoceSalario = dto.ConGoceSalario,
                EmpleadoId = dto.EmpleadoId,
                EstadoSolicitud = EstadoSolicitud.PENDIENTE.ToString(),
                FechaPermiso = dto.FechaPermiso,
                Motivo = dto.Motivo,
                FechaCreacion = DateTime.UtcNow
            };

            await _repo.CreatePermisoAsync(permiso);

            var registroCreado = await _repo.GetPermisoByIdAsync(permiso.IdPermiso);

            if (registroCreado == null)
                throw new BusinessException("Error al crear el permiso", "PERMISO_NO_CREADO");

            return new CrearPermisoDTO
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
    }
}