using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class RolesRepository : IRolesRepository
    {
        private readonly SistemaRhContext _context;

        public RolesRepository(SistemaRhContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Roles.AnyAsync(r => r.IdRol == id);
        }

        public async Task<IEnumerable<Roles>> GetAllAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<Roles?> GetByIdAsync(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

        public async Task<Dictionary<int, string>> GetNamesByIdsAsync(IEnumerable<int> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var idList = ids
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (!idList.Any())
                return new Dictionary<int, string>();

            return await _context.Roles
                .Where(r => idList.Contains(r.IdRol))
                .ToDictionaryAsync(
                    r => r.IdRol,
                    r => r.Nombre
                );
        }
    }
}