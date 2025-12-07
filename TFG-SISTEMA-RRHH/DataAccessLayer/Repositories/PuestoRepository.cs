using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
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

        public async Task DeleteAsync(int id)
        {
            var puesto = await GetByIdAsync(id);
            if (puesto != null) 
            { 
                _context.Puestos.Remove(puesto);
                await _context.SaveChangesAsync();
            }

        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Puestos.AnyAsync(e => e.IdPuesto == id);
        }

        public async Task<IEnumerable<Puestos>> GetAllAsync()
        {
            return await _context.Puestos.ToListAsync();
        }

        public async Task<Puestos?> GetByIdAsync(int ?id)
        {
            return await _context.Puestos.FindAsync(id);
        }

        public Task UpdateAsync(Puestos puesto)
        {
            throw new NotImplementedException();
        }
    }
}
