using DataAccessLayer.Entities;

namespace DataAccessLayer.Interfaces
{
    public interface IUsuariosRolesRepository
    {
        Task<IEnumerable<UsuariosRoles>> GetAllAsync();

        Task<UsuariosRoles?> GetByIdAsync(int id);

        Task<bool> ExistsAsync(int id);
    }
}