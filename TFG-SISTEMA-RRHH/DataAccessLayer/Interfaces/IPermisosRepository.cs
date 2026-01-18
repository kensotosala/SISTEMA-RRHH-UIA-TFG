using DataAccessLayer.Entities;

namespace DataAccessLayer.Interfaces
{
    public interface IPermisosRepository
    {
        Task<bool> ActualizarPermisoAsync(Permisos permiso);

        Task CreatePermisoAsync(Permisos permiso);

        Task<bool> DeletePermisoAsync(int id);

        Task<IEnumerable<Permisos>> GetAllPermisosAsync();

        Task<Permisos?> GetPermisoByIdAsync(int id);
    }
}