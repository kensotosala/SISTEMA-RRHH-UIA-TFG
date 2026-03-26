using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class NominaRepository : INominaRepository
    {
        private readonly SistemaRhContext _context;

        public NominaRepository(SistemaRhContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Nominas> CrearNominaAsync(Nominas nomina)
        {
            nomina.FechaCreacion = DateTime.UtcNow;
            nomina.Estado = nomina.Estado ?? "PENDIENTE";

            _context.Nominas.Add(nomina);
            await _context.SaveChangesAsync();
            return nomina;
        }

        public async Task<Nominas?> ObtenerNominaPorIdAsync(int id)
        {
            return await _context.Nominas
                .Include(n => n.Empleado)
                    .ThenInclude(e => e.Puesto)
                .Include(n => n.Empleado)
                    .ThenInclude(e => e.Departamento)
                .FirstOrDefaultAsync(n => n.IdNomina == id);
        }

        public async Task<List<Nominas>> ListarNominasAsync()
        {
            return await _context.Nominas
                .Include(n => n.Empleado)
                    .ThenInclude(e => e.Puesto)
                .Include(n => n.Empleado)
                    .ThenInclude(e => e.Departamento)
                .OrderByDescending(n => n.PeriodoNomina)
                .ToListAsync();
        }

        public async Task<Nominas> ActualizarNominaAsync(Nominas nomina)
        {
            nomina.FechaActualizacion = DateTime.UtcNow;
            _context.Nominas.Update(nomina);
            await _context.SaveChangesAsync();
            return nomina;
        }

        public async Task<bool> EliminarNominaAsync(int id)
        {
            var nomina = await _context.Nominas.FindAsync(id);
            if (nomina == null) return false;

            _context.Nominas.Remove(nomina);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Nominas>> ObtenerNominasPorEmpleadoAsync(int empleadoId)
        {
            return await _context.Nominas
                .Include(n => n.Empleado)
                .Where(n => n.EmpleadoId == empleadoId)
                .OrderByDescending(n => n.PeriodoNomina)
                .ToListAsync();
        }

        public async Task<List<Nominas>> ObtenerNominasPorPeriodoAsync(
            DateTime periodoInicio, DateTime periodoFin)
        {
            return await _context.Nominas
                .Include(n => n.Empleado)
                    .ThenInclude(e => e.Puesto)
                .Include(n => n.Empleado)
                    .ThenInclude(e => e.Departamento)
                .Where(n => n.PeriodoNomina >= periodoInicio && n.PeriodoNomina <= periodoFin)
                .OrderBy(n => n.EmpleadoId)
                .ThenBy(n => n.PeriodoNomina)
                .ToListAsync();
        }

        public async Task<List<Nominas>> ObtenerNominasQuincenaAsync(int quincena, int mes, int anio)
        {
            var fechaInicio = new DateTime(anio, mes, quincena == 1 ? 1 : 16);
            var fechaFin = quincena == 1
                ? new DateTime(anio, mes, 15)
                : new DateTime(anio, mes, DateTime.DaysInMonth(anio, mes));

            return await _context.Nominas
                .Include(n => n.Empleado)
                    .ThenInclude(e => e.Puesto)
                .Include(n => n.Empleado)
                    .ThenInclude(e => e.Departamento)
                .Where(n => n.PeriodoNomina >= fechaInicio && n.PeriodoNomina <= fechaFin)
                .OrderBy(n => n.Empleado.Nombre)
                .ToListAsync();
        }

        public async Task<Nominas?> ObtenerNominaEmpleadoQuincenaAsync(
            int empleadoId, int quincena, int mes, int anio)
        {
            var fechaInicio = new DateTime(anio, mes, quincena == 1 ? 1 : 16);
            var fechaFin = quincena == 1
                ? new DateTime(anio, mes, 15)
                : new DateTime(anio, mes, DateTime.DaysInMonth(anio, mes));

            return await _context.Nominas
                .Include(n => n.Empleado)
                .FirstOrDefaultAsync(n =>
                    n.EmpleadoId == empleadoId &&
                    n.PeriodoNomina >= fechaInicio &&
                    n.PeriodoNomina <= fechaFin);
        }

        public async Task<bool> ExisteNominaQuincenaAsync(
            int empleadoId, int quincena, int mes, int anio)
        {
            var fechaInicio = new DateTime(anio, mes, quincena == 1 ? 1 : 16);
            var fechaFin = quincena == 1
                ? new DateTime(anio, mes, 15)
                : new DateTime(anio, mes, DateTime.DaysInMonth(anio, mes));

            return await _context.Nominas.AnyAsync(n =>
                n.EmpleadoId == empleadoId &&
                n.PeriodoNomina >= fechaInicio &&
                n.PeriodoNomina <= fechaFin);
        }

        public async Task<List<Nominas>> ObtenerNominasMesAsync(int mes, int anio)
        {
            var fechaInicio = new DateTime(anio, mes, 1);
            var fechaFin = new DateTime(anio, mes, DateTime.DaysInMonth(anio, mes));

            return await _context.Nominas
                .Include(n => n.Empleado)
                    .ThenInclude(e => e.Puesto)
                .Include(n => n.Empleado)
                    .ThenInclude(e => e.Departamento)
                .Where(n => n.PeriodoNomina >= fechaInicio && n.PeriodoNomina <= fechaFin)
                .OrderBy(n => n.Empleado.Nombre)
                .ToListAsync();
        }

        public async Task<decimal> ObtenerTotalNominaMesAsync(int mes, int anio)
        {
            var fechaInicio = new DateTime(anio, mes, 1);
            var fechaFin = new DateTime(anio, mes, DateTime.DaysInMonth(anio, mes));

            return await _context.Nominas
                .Where(n => n.PeriodoNomina >= fechaInicio && n.PeriodoNomina <= fechaFin)
                .SumAsync(n => n.TotalNeto);
        }

        public async Task<Nominas?> ObtenerNominaParcialEmpleadoQuincenaAsync(
            int empleadoId, int quincena, int mes, int anio)
        {
            var fechaInicio = new DateTime(anio, mes, quincena == 1 ? 1 : 16);
            var fechaFin = quincena == 1
                ? new DateTime(anio, mes, 15)
                : new DateTime(anio, mes, DateTime.DaysInMonth(anio, mes));

            return await _context.Nominas
                .FirstOrDefaultAsync(n =>
                    n.EmpleadoId == empleadoId &&
                    n.Estado == "PARCIAL" &&  
                    n.PeriodoNomina >= fechaInicio &&
                    n.PeriodoNomina <= fechaFin);
        }
    }
}