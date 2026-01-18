using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Interfaces
{
    public interface IPermisosManager
    {
        Task<bool> ActualizarPermisoAsync(int id, ActualizarPermisoDTO dto);

        Task<CrearPermisoDTO> CrearPermisoAsync(CrearPermisoDTO dto);

        Task<bool> EliminarPermisoAsync(int id);

        Task<ListarPermisoByIdDTO?> ListarPermisoByIdAsync(int id);

        Task<IEnumerable<ListarPermisosDTO?>> ListarPermisosAsync();
    }
}