using DataAccessLayer.Entities;

namespace DataAccessLayer.Interfaces
{
    public interface IRolesRepository
    {
        Task<IEnumerable<Roles>> GetAllAsync();

        Task<Dictionary<int, string>> GetNamesByIdsAsync(IEnumerable<int> ids);

        Task<Roles?> GetByIdAsync(int id);

        Task<bool> ExistsAsync(int id);
    }
}