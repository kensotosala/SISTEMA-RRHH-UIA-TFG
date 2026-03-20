using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class AguinaldoRepository : IAguinaldoRepository
    {
        private readonly SistemaRhContext _context;

        public AguinaldoRepository(SistemaRhContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Aguinaldos> CreateAsync(Aguinaldos aguinaldo)
        {
            aguinaldo.FechaCreacion = DateTime.UtcNow;
            aguinaldo.FechaModificacion = null;
            aguinaldo.Estado ??= "PENDIENTE";

            await _context.Aguinaldos.AddAsync(aguinaldo);
            await _context.SaveChangesAsync();

            return aguinaldo;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var aguinaldo = await _context.Aguinaldos.FindAsync(id);

            if (aguinaldo == null)
                return false;

            aguinaldo.Estado = "ANULADO";
            aguinaldo.FechaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExisteAguinaldoAsync(int empleadoId, int anio)
        {
            return await _context.Aguinaldos
                .AsNoTracking()
                .AnyAsync(a =>
                    a.EmpleadoId == empleadoId &&
                    a.Anio == anio &&
                    a.Estado != "ANULADO");
        }

        public async Task<IEnumerable<Aguinaldos>> GetAllAsync()
        {
            return await _context.Aguinaldos
                .AsNoTracking()
                .Where(a => a.Estado != "ANULADO")
                .Include(a => a.Empleado)
                    .ThenInclude(e => e.Departamento)
                .Include(a => a.Empleado.Puesto)
                .AsSplitQuery()
                .OrderByDescending(a => a.Anio)
                .ToListAsync();
        }

        public async Task<IEnumerable<Aguinaldos>> GetByAnioAsync(int anio)
        {
            return await _context.Aguinaldos
                .AsNoTracking()
                .Where(a => a.Anio == anio && a.Estado != "ANULADO")
                .Include(a => a.Empleado)
                    .ThenInclude(e => e.Departamento)
                .Include(a => a.Empleado.Puesto)
                .AsSplitQuery()
                .OrderBy(a => a.Empleado.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Aguinaldos>> GetByDepartamentoYAnioAsync(int departamentoId, int anio)
        {
            return await _context.Aguinaldos
                .AsNoTracking()
                .Where(a =>
                    a.Empleado.DepartamentoId == departamentoId &&
                    a.Anio == anio &&
                    a.Estado != "ANULADO")
                .Include(a => a.Empleado)
                    .ThenInclude(e => e.Departamento)
                .Include(a => a.Empleado.Puesto)
                .AsSplitQuery()
                .OrderBy(a => a.Empleado.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Aguinaldos>> GetByEmpleadoAsync(int empleadoId)
        {
            return await _context.Aguinaldos
                .AsNoTracking()
                .Where(a =>
                    a.EmpleadoId == empleadoId &&
                    a.Estado != "ANULADO")
                .Include(a => a.Empleado)
                .OrderByDescending(a => a.Anio)
                .ToListAsync();
        }

        public async Task<Aguinaldos?> GetByEmpleadoYAnioAsync(int empleadoId, int anio)
        {
            return await _context.Aguinaldos
                .AsNoTracking()
                .Include(a => a.Empleado)
                .FirstOrDefaultAsync(a =>
                    a.EmpleadoId == empleadoId &&
                    a.Anio == anio &&
                    a.Estado != "ANULADO");
        }

        public async Task<IEnumerable<Aguinaldos>> GetByEstadoAsync(string estado)
        {
            return await _context.Aguinaldos
                .AsNoTracking()
                .Where(a => a.Estado == estado)
                .Include(a => a.Empleado)
                    .ThenInclude(e => e.Departamento)
                .Include(a => a.Empleado.Puesto)
                .AsSplitQuery()
                .OrderByDescending(a => a.Anio)
                .ToListAsync();
        }

        public async Task<Aguinaldos?> GetByIdAsync(int id)
        {
            return await _context.Aguinaldos
                .AsNoTracking()
                .Include(a => a.Empleado)
                    .ThenInclude(e => e.Departamento)
                .Include(a => a.Empleado.Puesto)
                .AsSplitQuery()
                .FirstOrDefaultAsync(a => a.IdAguinaldo == id);
        }

        public async Task<Empleados?> GetEmpleadoConDetallesAsync(int empleadoId)
        {
            return await _context.Empleados
                .AsNoTracking()
                .Include(e => e.Departamento)
                .Include(e => e.Puesto)
                .Include(e => e.Nominas.Where(n => n.Estado == "PAGADA"))
                .AsSplitQuery()
                .FirstOrDefaultAsync(e => e.IdEmpleado == empleadoId);
        }

        public async Task<IEnumerable<Nominas>> GetNominasPorPeriodoAsync(
            int empleadoId,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            var inicio = fechaInicio.Date;
            var fin = fechaFin.Date.AddDays(1);

            return await _context.Nominas
                .AsNoTracking()
                .Where(n =>
                    n.EmpleadoId == empleadoId &&
                    n.PeriodoNomina >= inicio &&
                    n.PeriodoNomina < fin &&
                    n.Estado == "PAGADA")
                .OrderBy(n => n.PeriodoNomina)
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(Aguinaldos aguinaldo)
        {
            var existing = await _context.Aguinaldos
                .FirstOrDefaultAsync(a => a.IdAguinaldo == aguinaldo.IdAguinaldo);

            if (existing == null)
                return false;

            existing.MontoAguinaldo = aguinaldo.MontoAguinaldo;
            existing.Estado = aguinaldo.Estado;
            existing.FechaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<bool> ExistsAsync(int id)
        {
            return await _context.Aguinaldos
                .AsNoTracking()
                .AnyAsync(a => a.IdAguinaldo == id);
        }
    }
}