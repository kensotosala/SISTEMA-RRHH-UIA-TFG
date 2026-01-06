using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class UsuariosRolesRepository : IUsuariosRolesRepository
    {
        private readonly SistemaRhContext _context;

        public UsuariosRolesRepository(SistemaRhContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.UsuariosRoles.AnyAsync(ur => ur.IdUsuarioRol == id);
        }

        public async Task<IEnumerable<UsuariosRoles>> GetAllAsync()
        {
            return await _context.UsuariosRoles.ToListAsync();
        }

        public async Task<UsuariosRoles?> GetByIdAsync(int id)
        {
            return await _context.UsuariosRoles.FindAsync(id);
        }
    }
}