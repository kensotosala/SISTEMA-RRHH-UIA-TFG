using BusinessLogicLayer.DTOs;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Interfaces
{
    public interface IRolesManager
    {
        Task<IEnumerable<RolDTO>> GetAllAsync();
    }
}
