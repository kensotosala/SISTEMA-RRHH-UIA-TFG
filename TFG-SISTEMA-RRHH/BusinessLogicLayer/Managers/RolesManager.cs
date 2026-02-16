using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;

namespace BusinessLogicLayer.Managers
{
    public class RolesManager : IRolesManager
    {
        private readonly IRolesRepository _repoRoles;

        public RolesManager(IRolesRepository repoRoles)
        {
            _repoRoles = repoRoles;
        }

        public async Task<IEnumerable<RolDTO>> GetAllAsync()
        {
            var roles = await _repoRoles.GetAllAsync();

            return roles.Select(r => new RolDTO
            {
                Id = r.IdRol,
                Nombre = r.Nombre
            });
        }
    }
}
