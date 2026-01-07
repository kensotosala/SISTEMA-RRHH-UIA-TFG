using DataAccessLayer.Entities;

namespace DataAccessLayer.Interfaces
{
    public interface IAsistenciasRepository
    {
        Task CreateAsync(Asistencias asistencia);

        Task UpdateAsync(Asistencias asistencia);

        Task<Asistencias?> GetByEmpleadoYFechaAsync(int empleadoId, DateTime fecha);
    }
}