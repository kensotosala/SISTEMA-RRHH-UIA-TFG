using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    /// <summary>
    /// Repositorio para operaciones CRUD de Aguinaldos
    /// </summary>
    public class AguinaldoRepository : IAguinaldoRepository
    {
        private readonly SistemaRhContext _context;

        public AguinaldoRepository(SistemaRhContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Aguinaldos?> GetByIdAsync(int id)
        {
            return await _context.Aguinaldos
                .Include(a => a.Empleado)
                    .ThenInclude(e => e.Departamento)
                .Include(a => a.Empleado)
                    .ThenInclude(e => e.Puesto)
                .FirstOrDefaultAsync(a => a.IdAguinaldo == id);
        }

        public async Task<IEnumerable<Aguinaldos>> GetAllAsync()
        {
            return await _context.Aguinaldos
                .Include(a => a.Empleado)
                    .ThenInclude(e => e.Departamento)
                .Include(a => a.Empleado)
                    .ThenInclude(e => e.Puesto)
                .OrderByDescending(a => a.FechaCalculo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Aguinaldos>> GetByAnioAsync(int anio)
        {
            return await _context.Aguinaldos
                .Include(a => a.Empleado)
                    .ThenInclude(e => e.Departamento)
                .Include(a => a.Empleado)
                    .ThenInclude(e => e.Puesto)
                .Where(a => a.FechaCalculo.Year == anio)
                .OrderBy(a => a.Empleado.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Aguinaldos>> GetByEmpleadoAsync(int empleadoId)
        {
            return await _context.Aguinaldos
                .Include(a => a.Empleado)
                .Where(a => a.EmpleadoId == empleadoId)
                .OrderByDescending(a => a.FechaCalculo)
                .ToListAsync();
        }

        public async Task<Aguinaldos?> GetByEmpleadoYAnioAsync(int empleadoId, int anio)
        {
            return await _context.Aguinaldos
                .Include(a => a.Empleado)
                .FirstOrDefaultAsync(a =>
                    a.EmpleadoId == empleadoId &&
                    a.FechaCalculo.Year == anio);
        }

        public async Task<IEnumerable<Aguinaldos>> GetByEstadoAsync(string estado)
        {
            return await _context.Aguinaldos
                .Include(a => a.Empleado)
                    .ThenInclude(e => e.Departamento)
                .Include(a => a.Empleado)
                    .ThenInclude(e => e.Puesto)
                .Where(a => a.Estado == estado)
                .OrderByDescending(a => a.FechaCalculo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Aguinaldos>> GetByDepartamentoYAnioAsync(int departamentoId, int anio)
        {
            return await _context.Aguinaldos
                .Include(a => a.Empleado)
                    .ThenInclude(e => e.Departamento)
                .Include(a => a.Empleado)
                    .ThenInclude(e => e.Puesto)
                .Where(a =>
                    a.Empleado.DepartamentoId == departamentoId &&
                    a.FechaCalculo.Year == anio)
                .OrderBy(a => a.Empleado.Nombre)
                .ToListAsync();
        }

        public async Task<Aguinaldos> CreateAsync(Aguinaldos aguinaldo)
        {
            aguinaldo.FechaCreacion = DateTime.UtcNow;
            aguinaldo.Estado = aguinaldo.Estado ?? "PENDIENTE";

            _context.Aguinaldos.Add(aguinaldo);
            await _context.SaveChangesAsync();

            return aguinaldo;
        }

        public async Task<bool> UpdateAsync(Aguinaldos aguinaldo)
        {
            aguinaldo.FechaModificacion = DateTime.UtcNow;

            _context.Entry(aguinaldo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ExistsAsync(aguinaldo.IdAguinaldo))
                    return false;
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var aguinaldo = await _context.Aguinaldos.FindAsync(id);
            if (aguinaldo == null)
                return false;

            // Soft delete
            aguinaldo.Estado = "ANULADO";
            aguinaldo.FechaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExisteAguinaldoAsync(int empleadoId, int anio)
        {
            return await _context.Aguinaldos
                .AnyAsync(a =>
                    a.EmpleadoId == empleadoId &&
                    a.FechaCalculo.Year == anio);
        }

        public async Task<IEnumerable<Nominas>> GetNominasPorPeriodoAsync(
            int empleadoId,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            return await _context.Nominas
                .Where(n =>
                    n.EmpleadoId == empleadoId &&
                    n.PeriodoNomina >= fechaInicio &&
                    n.PeriodoNomina <= fechaFin &&
                    n.Estado == "PAGADA")
                .OrderBy(n => n.PeriodoNomina)
                .ToListAsync();
        }

        public async Task<Empleados?> GetEmpleadoConDetallesAsync(int empleadoId)
        {
            return await _context.Empleados
                .Include(e => e.Departamento)
                .Include(e => e.Puesto)
                .Include(e => e.Nominas.Where(n => n.Estado == "PAGADA"))
                .FirstOrDefaultAsync(e => e.IdEmpleado == empleadoId);
        }

        private async Task<bool> ExistsAsync(int id)
        {
            return await _context.Aguinaldos.AnyAsync(a => a.IdAguinaldo == id);
        }
    }
}