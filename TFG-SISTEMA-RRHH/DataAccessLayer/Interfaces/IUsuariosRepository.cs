using DataAccessLayer.Entities;

namespace DataAccessLayer.Interfaces
{
    public interface IUsuariosRepository
    {
        Task<Usuarios?> GetByIdAsync(int id);

        Task<Usuarios?> GetByUsernameAsync(string username);

        Task<Usuarios?> GetByUsernameWithDetailsAsync(string username);

        Task<List<Usuarios>> GetAllAsync();

        Task<List<Usuarios>> GetActiveUsersAsync();

        Task<Usuarios> CreateAsync(Usuarios usuario);

        Task<bool> UpdateAsync(Usuarios usuario);

        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsByUsernameAsync(string username);

        Task<bool> ExistsByUsernameExcludingIdAsync(string username, int excludeId);

        Task<bool> UpdateLastAccessAsync(int userId);

        Task<bool> ChangeStatusAsync(int userId, string newStatus);
    }
}