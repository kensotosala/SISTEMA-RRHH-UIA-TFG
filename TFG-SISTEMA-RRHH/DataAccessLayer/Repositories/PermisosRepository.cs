using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class PermisosRepository : IPermisosRepository
    {
        private readonly SistemaRhContext _context;

        public PermisosRepository(SistemaRhContext context)
        {
            _context = context;
        }

        public async Task CreatePermisoAsync(Permisos permiso)
        {
            await _context.Permisos.AddAsync(permiso);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ActualizarPermisoAsync(Permisos permiso)
        {
            _context.Permisos.Update(permiso);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePermisoAsync(int id)
        {
            var permiso = await _context.Permisos.FindAsync(id);
            if (permiso == null)
                return false;

            _context.Permisos.Remove(permiso);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Permisos>> GetAllPermisosAsync()
        {
            return await _context.Permisos.ToListAsync();
        }

        public async Task<Permisos?> GetPermisoByIdAsync(int id)
        {
            return await _context.Permisos.FindAsync(id);
        }
    }
}