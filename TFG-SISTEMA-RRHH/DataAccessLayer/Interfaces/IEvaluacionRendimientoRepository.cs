using DataAccessLayer.Entities;

namespace DataAccessLayer.Interfaces
{
    public interface IEvaluacionRendimientoRepository
    {
        // Evaluaciones
        Task<IEnumerable<EvaluacionesRendimiento>> GetAllAsync();

        Task<EvaluacionesRendimiento?> GetByIdAsync(int idEvaluacion);

        Task<EvaluacionesRendimiento> CreateAsync(EvaluacionesRendimiento evaluacion);

        Task<EvaluacionesRendimiento> UpdateAsync(EvaluacionesRendimiento evaluacion);

        Task<bool> DeleteAsync(int idEvaluacion);

        Task<bool> ExistsAsync(int idEvaluacion);

        Task<bool> ExisteEvaluacionEnAnioAsync(int empleadoId, int anio, int? excluirIdEvaluacion = null);


        // Detalles
        Task<IEnumerable<DetalleEvaluaciones>> GetDetallesByEvaluacionIdAsync(int idEvaluacion);

        Task<DetalleEvaluaciones?> GetDetalleByIdAsync(int idDetalle);

        Task<DetalleEvaluaciones> CreateDetalleAsync(DetalleEvaluaciones detalle);

        Task<DetalleEvaluaciones> UpdateDetalleAsync(DetalleEvaluaciones detalle);

        Task<bool> DeleteDetalleAsync(int idDetalle);

        Task DeleteDetallesByEvaluacionIdAsync(int idEvaluacion);
    }
}