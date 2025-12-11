namespace DataAccessLayer.Repositories
{
    using DataAccessLayer.Data;
    using DataAccessLayer.Entities;
    using DataAccessLayer.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class PuestoRepository : IPuestosRepository
    {
        private readonly SistemaRhContext _context;

        public PuestoRepository(SistemaRhContext context)
        {
            _context = context;
        }

        public async Task<Puestos> CreateAsync(Puestos puesto)
        {
            _context.Puestos.Add(puesto);
            await _context.SaveChangesAsync();
            return puesto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var puesto = await _context.Puestos.FindAsync(id);
            if (puesto == null)
                return false;

            _context.Puestos.Remove(puesto);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Puestos.AnyAsync(e => e.IdPuesto == id);
        }

        public async Task<IEnumerable<Puestos>> GetAllAsync()
        {
            return await _context.Puestos.ToListAsync();
        }

        public async Task<Puestos?> GetByIdAsync(int id)
        {
            return await _context.Puestos.FindAsync(id);
        }

        public async Task<bool> UpdateAsync(Puestos puesto)
        {
            _context.Puestos.Update(puesto);
            var affectedRows = await _context.SaveChangesAsync();
            return affectedRows > 0;
        }
    }
}