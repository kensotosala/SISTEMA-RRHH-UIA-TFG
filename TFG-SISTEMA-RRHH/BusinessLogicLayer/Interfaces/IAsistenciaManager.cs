using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Interfaces
{
    public interface IAsistenciaManager
    {
        Task<AsistenciaDTO> CreateAsync(CrearAsistenciaDTO dto);

        Task<bool> DeleteAsync(int id);

        Task<IEnumerable<AsistenciaDTO>> GetAllAsync();

        Task<IEnumerable<AsistenciaDTO>> GetByFiltrosAsync(FiltrosAsistenciaDTO filtros);

        Task<AsistenciaDTO?> GetByIdAsync(int id);

        Task<ReporteAsistenciaDTO> GetReporteEmpleadoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin);

        Task<MarcarAsistenciaResponse> MarcarAsistenciaAsync(int empleadoId);

        Task<EstadoAsistenciaDTO> ObtenerEstadoAsistenciaAsync(int empleadoId);

        Task<bool> UpdateAsync(int id, ActualizarAsistenciaDTO dto);
    }
}