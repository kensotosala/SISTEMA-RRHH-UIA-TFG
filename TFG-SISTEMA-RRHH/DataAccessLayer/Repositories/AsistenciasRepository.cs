using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class AsistenciasRepository : IAsistenciasRepository
    {
        private readonly SistemaRhContext _context;

        public AsistenciasRepository(SistemaRhContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Asistencias asistencia)
        {
            asistencia.FechaCreacion = DateTime.Now;
            await _context.Asistencias.AddAsync(asistencia);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var asistencia = await _context.Asistencias.FindAsync(id);
            if (asistencia == null)
                return false;

            _context.Asistencias.Remove(asistencia);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> DiasTrabajadosPorPeriodoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.Asistencias
                .Where(a => a.EmpleadoId == empleadoId &&
                            a.FechaRegistro.Date >= fechaInicio.Date &&
                            a.FechaRegistro.Date <= fechaFin.Date &&
                            a.Estado == "Presente")
                .CountAsync();
        }

        public async Task<bool> ExisteRegistroAsync(int empleadoId, DateTime fecha)
        {
            var fechaSolo = fecha.Date;
            return await _context.Asistencias
                .AnyAsync(a => a.EmpleadoId == empleadoId &&
                              a.FechaRegistro.Date == fechaSolo);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Asistencias.AnyAsync(a => a.IdAsistencia == id);
        }

        public async Task<IEnumerable<Asistencias>> GetAllAsync()
        {
            return await _context.Asistencias
                .Include(a => a.Empleado)
                    .ThenInclude(e => e.Puesto)  
                .Include(a => a.Empleado.Departamento) 
                .OrderByDescending(a => a.FechaRegistro)
                .ToListAsync();
        }

        public async Task<Asistencias?> GetByEmpleadoYFechaAsync(int empleadoId, DateTime fecha)
        {
            var fechaSolo = fecha.Date;
            return await _context.Asistencias
                .Include(a => a.Empleado)
                .FirstOrDefaultAsync(a =>
                    a.EmpleadoId == empleadoId &&
                    a.FechaRegistro.Date == fechaSolo);
        }

        public async Task<IEnumerable<Asistencias>> GetByFiltrosAsync(
            int? empleadoId,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            string? estado,
            int? departamentoId)
        {
            var query = _context.Asistencias
                .Include(a => a.Empleado)
                    .ThenInclude(e => e.Departamento)
                .Include(a => a.Empleado.Puesto)
                .AsQueryable();

            if (empleadoId.HasValue)
                query = query.Where(a => a.EmpleadoId == empleadoId.Value);

            if (fechaInicio.HasValue)
                query = query.Where(a => a.FechaRegistro.Date >= fechaInicio.Value.Date);

            if (fechaFin.HasValue)
                query = query.Where(a => a.FechaRegistro.Date <= fechaFin.Value.Date);

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(a => a.Estado == estado);

            if (departamentoId.HasValue)
                query = query.Where(a => a.Empleado.DepartamentoId == departamentoId.Value);

            return await query
                .OrderByDescending(a => a.FechaRegistro)
                .ToListAsync();
        }

        public async Task<Asistencias?> GetByIdAsync(int id)
        {
            return await _context.Asistencias
                .Include(a => a.Empleado)
                    .ThenInclude(e => e.Departamento)
                .Include(a => a.Empleado.Puesto)  
                .FirstOrDefaultAsync(a => a.IdAsistencia == id);
        }
        public async Task<bool> UpdateAsync(Asistencias asistencia)
        {
            var existe = await _context.Asistencias
                .AnyAsync(a => a.IdAsistencia == asistencia.IdAsistencia);

            if (!existe)
                return false;

            asistencia.FechaModificacion = DateTime.Now;
            _context.Asistencias.Update(asistencia);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}