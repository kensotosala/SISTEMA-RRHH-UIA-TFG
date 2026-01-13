using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class HorasExtrasRepository : IHorasExtrasRepository
    {
        private readonly SistemaRhContext _context;

        public HorasExtrasRepository(SistemaRhContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<HorasExtras>> GetAllAsync()
        {
            return await _context.HorasExtras
                .Include(h => h.Empleado)
                    .ThenInclude(e => e.Departamento)
                .Include(h => h.JefeAprueba)
                .OrderByDescending(h => h.FechaSolicitud)
                .ToListAsync();
        }

        public async Task<HorasExtras?> GetByIdAsync(int id)
        {
            return await _context.HorasExtras
                .Include(h => h.Empleado)
                    .ThenInclude(e => e.Departamento)
                .Include(h => h.JefeAprueba)
                .FirstOrDefaultAsync(h => h.IdHoraExtra == id);
        }

        public async Task<IEnumerable<HorasExtras>> GetByFiltrosAsync(
            int? empleadoId,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            string? estadoSolicitud,
            int? departamentoId,
            int? jefeApruebaId)
        {
            var query = _context.HorasExtras
                .Include(h => h.Empleado)
                    .ThenInclude(e => e.Departamento)
                .Include(h => h.JefeAprueba)
                .AsQueryable();

            if (empleadoId.HasValue)
                query = query.Where(h => h.EmpleadoId == empleadoId.Value);

            if (fechaInicio.HasValue)
                query = query.Where(h => h.FechaInicio >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(h => h.FechaFin <= fechaFin.Value);

            if (!string.IsNullOrEmpty(estadoSolicitud))
                query = query.Where(h => h.EstadoSolicitud == estadoSolicitud);

            if (departamentoId.HasValue)
                query = query.Where(h => h.Empleado.DepartamentoId == departamentoId.Value);

            if (jefeApruebaId.HasValue)
                query = query.Where(h => h.JefeApruebaId == jefeApruebaId.Value);

            return await query
                .OrderByDescending(h => h.FechaSolicitud)
                .ToListAsync();
        }

        public async Task<IEnumerable<HorasExtras>> GetByEmpleadoAsync(int empleadoId)
        {
            return await _context.HorasExtras
                .Include(h => h.JefeAprueba)
                .Where(h => h.EmpleadoId == empleadoId)
                .OrderByDescending(h => h.FechaSolicitud)
                .ToListAsync();
        }

        public async Task<IEnumerable<HorasExtras>> GetPendientesByJefeAsync(int jefeId)
        {
            return await _context.HorasExtras
                .Include(h => h.Empleado)
                    .ThenInclude(e => e.Departamento)
                .Where(h => h.Empleado.JefeInmediatoId == jefeId &&
                           h.EstadoSolicitud == "PENDIENTE")
                .OrderBy(h => h.FechaSolicitud)
                .ToListAsync();
        }

        public async Task<HorasExtras> CreateAsync(HorasExtras horaExtra)
        {
            await _context.HorasExtras.AddAsync(horaExtra);
            await _context.SaveChangesAsync();
            return horaExtra;
        }

        public async Task<bool> UpdateAsync(HorasExtras horaExtra)
        {
            var existe = await _context.HorasExtras
                .AnyAsync(h => h.IdHoraExtra == horaExtra.IdHoraExtra);

            if (!existe)
                return false;

            horaExtra.FechaModificacion = DateTime.UtcNow;
            _context.HorasExtras.Update(horaExtra);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var horaExtra = await _context.HorasExtras.FindAsync(id);
            if (horaExtra == null)
                return false;

            _context.HorasExtras.Remove(horaExtra);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.HorasExtras.AnyAsync(h => h.IdHoraExtra == id);
        }

        public async Task<bool> TieneSolapamientoAsync(
            int empleadoId,
            DateTime fechaInicio,
            DateTime fechaFin,
            int? excludeId = null)
        {
            var query = _context.HorasExtras
                .Where(h => h.EmpleadoId == empleadoId &&
                           h.EstadoSolicitud != "RECHAZADA" &&
                           ((h.FechaInicio <= fechaFin && h.FechaFin >= fechaInicio)));

            if (excludeId.HasValue)
                query = query.Where(h => h.IdHoraExtra != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}