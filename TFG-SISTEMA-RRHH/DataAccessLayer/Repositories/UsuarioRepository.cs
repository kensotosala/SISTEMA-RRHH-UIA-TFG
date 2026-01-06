using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly SistemaRhContext _context;

        public UsuarioRepository(SistemaRhContext context)
        {
            _context = context;
        }

        public async Task<Usuarios?> CreateAsync(Usuarios usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var usuario = await GetByIdAsync(id);
            if (usuario is null)
                return false;

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Usuarios.AnyAsync(u => u.IdUsuario == id);
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await _context.Usuarios.AnyAsync(u => u.NombreUsuario == username);
        }

        public async Task<IEnumerable<Usuarios>> GetAllAsync()
        {
            return await _context.Usuarios.ToListAsync();
        }

        public async Task<Usuarios?> GetByIdAsync(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task<Usuarios?> GetByUsernameAsync(string username)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == username);
        }

        public async Task<Usuarios?> GetByUsernameWithDetailsAsync(string username)
        {
            return await _context.Usuarios
                .Include(u => u.UsuariosRoles)
                    .ThenInclude(ur => ur.Rol)
                .Include(u => u.Empleado)
                .FirstOrDefaultAsync(u => u.NombreUsuario == username);
        }

        public async Task<bool> UpdateAsync(Usuarios usuario)
        {
            _context.Usuarios.Update(usuario);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<Usuarios?> GetByEmpleadoIdAsync(int empleadoId)
        {
            return await _context.Usuarios
                .Include(u => u.UsuariosRoles)
                    .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.EmpleadoId == empleadoId);
        }
    }
}