using DataAccessLayer.Entities;

namespace DataAccessLayer.Interfaces
{
    public interface IHorasExtrasRepository
    {
        Task<IEnumerable<HorasExtras>> GetAllAsync();

        Task<HorasExtras?> GetByIdAsync(int id);

        Task<IEnumerable<HorasExtras>> GetByFiltrosAsync(
            int? empleadoId,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            string? estadoSolicitud,
            int? departamentoId,
            int? jefeApruebaId);

        Task<IEnumerable<HorasExtras>> GetByEmpleadoAsync(int empleadoId);

        Task<IEnumerable<HorasExtras>> GetPendientesByJefeAsync(int jefeId);

        Task<HorasExtras> CreateAsync(HorasExtras horaExtra);

        Task<bool> UpdateAsync(HorasExtras horaExtra);

        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);

        Task<bool> TieneSolapamientoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin, int? excludeId = null);

        
    }
}