using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class LiquidacionesRepository : ILiquidacionesRepository
    {
        private readonly SistemaRhContext _context;

        public LiquidacionesRepository(SistemaRhContext context)
        {
            _context = context;
        }

        public async Task<bool> AnularLiquidacion(int id)
        {
            var liquidacion = await _context.Liquidaciones.FindAsync(id);

            if (liquidacion is null)
                return false;

            liquidacion.Estado = "ANULADA";
            liquidacion.FechaModificacion = DateTime.Now;

            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<Liquidaciones> CrearLiquidacion(Liquidaciones liquidacion)
        {
            var result = await _context.Liquidaciones.AddAsync(liquidacion);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<IEnumerable<Liquidaciones>> ListarLiquidaciones()
        {
            return await _context.Liquidaciones
                .Include(l => l.Empleado)
                    .ThenInclude(e => e.VacacionesEmpleado)
                .ToListAsync();
        }

        public async Task<bool> ModificarLiquidacion(Liquidaciones liquidacion)
        {
            _context.Liquidaciones.Update(liquidacion);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<Empleados?> ObtenerEmpleadoPorId(int idEmpleado)
        {
            return await _context.Empleados
                .Include(e => e.VacacionesEmpleado)
                .FirstOrDefaultAsync(e => e.IdEmpleado == idEmpleado);
        }

        public async Task<Liquidaciones?> ObtenerLiquidacionPorId(int id)
        {
            return await _context.Liquidaciones
                .Include(l => l.Empleado)
                .FirstOrDefaultAsync(l => l.IdLiquidacion == id);
        }

        public Task<List<Nominas>> ObtenerNominasUltimos12Meses(int idEmpleado)
        {
            var fechaLimite = DateTime.Now.AddMonths(-12);
            return _context.Nominas
                .Where(n => n.EmpleadoId == idEmpleado && n.PeriodoNomina >= fechaLimite)
                .ToListAsync();
        }

        public Task<List<Nominas>> ObtenerNominasUltimos6Meses(int idEmpleado)
        {
            var fechaLimite = DateTime.Now.AddMonths(-6);
            return _context.Nominas
                .Where(n => n.EmpleadoId == idEmpleado && n.PeriodoNomina >= fechaLimite)
                .ToListAsync();
        }
    }
}