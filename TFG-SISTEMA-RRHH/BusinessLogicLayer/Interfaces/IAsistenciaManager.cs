using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Interfaces
{
    public interface IAsistenciaManager
    {
        Task<MarcarAsistenciaResponse> MarcarAsistenciaAsync(int empleadoId);

        Task<EstadoAsistenciaDTO> ObtenerEstadoAsistenciaAsync(int empleadoId);
    }
}