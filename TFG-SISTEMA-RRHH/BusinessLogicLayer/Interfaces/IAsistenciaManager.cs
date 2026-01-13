using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Interfaces
{
    public interface IAsistenciaManager
    {
        // CRUD para administrador
        Task<IEnumerable<AsistenciaDTO>> GetAllAsync();

        Task<AsistenciaDTO?> GetByIdAsync(int id);

        Task<IEnumerable<AsistenciaDTO>> GetByFiltrosAsync(FiltrosAsistenciaDTO filtros);

        Task<AsistenciaDTO> CreateAsync(CrearAsistenciaDTO dto);

        Task<bool> UpdateAsync(int id, ActualizarAsistenciaDTO dto);

        Task<bool> DeleteAsync(int id);

        Task<ReporteAsistenciaDTO> GetReporteEmpleadoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin);
    }
}