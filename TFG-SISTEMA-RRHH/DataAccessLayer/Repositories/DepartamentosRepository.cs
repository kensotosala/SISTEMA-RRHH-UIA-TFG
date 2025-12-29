using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class DepartamentosRepository : IDepartamentosRepository
    {
        private readonly SistemaRhContext _context;

        public DepartamentosRepository(SistemaRhContext context)
        {
            _context = context;
        }

        public async Task<Departamentos> CreateAsync(Departamentos departamento)
        {
            _context.Departamentos.Add(departamento);
            await _context.SaveChangesAsync();
            return departamento;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var result = await GetByIdAsync(id);
            if (result == null)
                return false;

            _context.Departamentos.Remove(result);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Departamentos.AnyAsync(e => e.IdDepartamento == id);
        }

        public async Task<IEnumerable<Departamentos>> GetAllAsync()
        {
            return await _context.Departamentos.ToListAsync();
        }

        public async Task<Departamentos?> GetByIdAsync(int id)
        {
            return await _context.Departamentos.FindAsync(id);
        }

        public async Task<bool> UpdateAsync(Departamentos departamento)
        {
            _context.Departamentos.Update(departamento);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}