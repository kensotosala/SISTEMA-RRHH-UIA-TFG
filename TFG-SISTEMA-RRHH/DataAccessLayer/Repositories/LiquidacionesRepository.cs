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

        public Task<bool> AnularLiquidacion(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Liquidaciones> CrearLiquidacion(Liquidaciones liquidacion)
        {
            var result = await _context.Liquidaciones.AddAsync(liquidacion);

            await _context.SaveChangesAsync();

            return result.Entity;
        }

        public Task<IEnumerable<Liquidaciones>> ListarLiquidaciones()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ModificarLiquidacion(Liquidaciones liquidacion)
        {
            await _context.Liquidaciones.AddAsync(liquidacion);

            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<Empleados?> ObtenerEmpleadoPorId(int idEmpleado)
        {
            return await _context.Empleados.FindAsync(idEmpleado);
        }

        public async Task<Liquidaciones?> ObtenerLiquidacionPorId(int id)
        {
            return await _context.Liquidaciones.FindAsync(id);
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