using DataAccessLayer.Entities;

namespace DataAccessLayer.Interfaces
{
    public interface IAsistenciasRepository
    {
        Task<IEnumerable<Asistencias>> GetAllAsync();

        Task<Asistencias?> GetByIdAsync(int id);

        Task<Asistencias?> GetByEmpleadoYFechaAsync(int empleadoId, DateTime fecha);

        Task<IEnumerable<Asistencias>> GetByFiltrosAsync(
            int? empleadoId,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            string? estado,
            int? departamentoId);

        Task CreateAsync(Asistencias asistencia);

        Task<bool> UpdateAsync(Asistencias asistencia);

        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);

        Task<bool> ExisteRegistroAsync(int empleadoId, DateTime fecha);
    }
}