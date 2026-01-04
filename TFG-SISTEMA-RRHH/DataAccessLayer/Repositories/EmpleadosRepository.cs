using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class EmpleadosRepository : IEmpleadosRepository
    {
        private readonly SistemaRhContext _context;

        public EmpleadosRepository(SistemaRhContext contex)
        {
            _context = contex;
        }

        public async Task<Empleados> CreateAsync(Empleados empleado)
        {
            _context.Empleados.Add(empleado);
            await _context.SaveChangesAsync();
            return empleado;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Empleados.FindAsync(id) ?? throw new KeyNotFoundException($"Empleado con ID {id} no encontrado.");

            _context.Empleados.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Empleados.AnyAsync(e => e.IdEmpleado == id);
        }

        public async Task<IEnumerable<Empleados>> GetAllAsync()
        {
            return await _context.Empleados.ToListAsync();
        }

        public async Task<Empleados?> GetByIdAsync(int id)
        {
            return await _context.Empleados.FindAsync(id);
        }

        public async Task UpdateAsync(Empleados empleado)
        {
            _context.Empleados.Update(empleado);
            await _context.SaveChangesAsync();
        }
    }
}