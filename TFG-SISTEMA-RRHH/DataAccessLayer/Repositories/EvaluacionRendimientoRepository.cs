using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class EvaluacionRendimientoRepository : IEvaluacionRendimientoRepository
    {
        private readonly SistemaRhContext _context;

        public EvaluacionRendimientoRepository(SistemaRhContext context)
        {
            _context = context;
        }

        // Evaluaciones
        public async Task<IEnumerable<EvaluacionesRendimiento>> GetAllAsync()
        {
            return await _context.EvaluacionesRendimiento
                .Include(e => e.Empleado)
                .Include(e => e.Evaluador)
                .Include(e => e.DetalleEvaluaciones)
                    .ThenInclude(d => d.IdMetricaNavigation)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<EvaluacionesRendimiento?> GetByIdAsync(int idEvaluacion)
        {
            return await _context.EvaluacionesRendimiento
                .Include(e => e.Empleado)
                .Include(e => e.Evaluador)
                .Include(e => e.DetalleEvaluaciones)
                    .ThenInclude(d => d.IdMetricaNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEvaluacion == idEvaluacion);
        }

        public async Task<EvaluacionesRendimiento> CreateAsync(EvaluacionesRendimiento evaluacion)
        {
            evaluacion.FechaCreacion = DateTime.Now;
            evaluacion.FechaModificacion = DateTime.Now;

            _context.EvaluacionesRendimiento.Add(evaluacion);
            await _context.SaveChangesAsync();
            return evaluacion;
        }

        public async Task<bool> ExisteEvaluacionEnAnioAsync(int empleadoId, int anio, int? excluirIdEvaluacion = null)
        {
            return await _context.EvaluacionesRendimiento
                .AnyAsync(e =>
                    e.EmpleadoId == empleadoId &&
                    e.FechaInicio.Year == anio &&
                    (excluirIdEvaluacion == null || e.IdEvaluacion != excluirIdEvaluacion) &&
                    e.Estado != "ANULADA");
        }

        public async Task<EvaluacionesRendimiento> UpdateAsync(EvaluacionesRendimiento evaluacion)
        {
            evaluacion.FechaModificacion = DateTime.Now;
            _context.EvaluacionesRendimiento.Update(evaluacion);
            await _context.SaveChangesAsync();
            return evaluacion;
        }

        public async Task<bool> DeleteAsync(int idEvaluacion)
        {
            var evaluacion = await _context.EvaluacionesRendimiento
                .Include(e => e.DetalleEvaluaciones)
                .FirstOrDefaultAsync(e => e.IdEvaluacion == idEvaluacion);

            if (evaluacion is null) return false;

            _context.DetalleEvaluaciones.RemoveRange(evaluacion.DetalleEvaluaciones);
            _context.EvaluacionesRendimiento.Remove(evaluacion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int idEvaluacion)
        {
            return await _context.EvaluacionesRendimiento
                .AnyAsync(e => e.IdEvaluacion == idEvaluacion);
        }

        // Detalles
        public async Task<IEnumerable<DetalleEvaluaciones>> GetDetallesByEvaluacionIdAsync(int idEvaluacion)
        {
            return await _context.DetalleEvaluaciones
                .Include(d => d.IdMetricaNavigation)
                .Where(d => d.IdEvaluacion == idEvaluacion)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<DetalleEvaluaciones?> GetDetalleByIdAsync(int idDetalle)
        {
            return await _context.DetalleEvaluaciones
                .Include(d => d.IdMetricaNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.IdDetalle == idDetalle);
        }

        public async Task<DetalleEvaluaciones> CreateDetalleAsync(DetalleEvaluaciones detalle)
        {
            detalle.FechaCreacion = DateTime.Now;
            detalle.FechaModificacion = DateTime.Now;
            _context.DetalleEvaluaciones.Add(detalle);
            await _context.SaveChangesAsync();
            return detalle;
        }

        public async Task<DetalleEvaluaciones> UpdateDetalleAsync(DetalleEvaluaciones detalle)
        {
            detalle.FechaModificacion = DateTime.Now;
            _context.DetalleEvaluaciones.Update(detalle);
            await _context.SaveChangesAsync();
            return detalle;
        }

        public async Task<bool> DeleteDetalleAsync(int idDetalle)
        {
            var detalle = await _context.DetalleEvaluaciones.FindAsync(idDetalle);
            if (detalle is null) return false;

            _context.DetalleEvaluaciones.Remove(detalle);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteDetallesByEvaluacionIdAsync(int idEvaluacion)
        {
            var detalles = await _context.DetalleEvaluaciones
                .Where(d => d.IdEvaluacion == idEvaluacion)
                .ToListAsync();

            _context.DetalleEvaluaciones.RemoveRange(detalles);
            await _context.SaveChangesAsync();
        }
    }
}