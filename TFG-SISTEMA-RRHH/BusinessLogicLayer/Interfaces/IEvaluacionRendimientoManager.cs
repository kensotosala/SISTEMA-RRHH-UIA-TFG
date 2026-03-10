using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Interfaces
{
    public interface IEvaluacionRendimientoManager
    {
        Task<ResultDTO<IEnumerable<EvaluacionResponseDTO>>> GetAllAsync();

        Task<ResultDTO<EvaluacionResponseDTO>> GetByIdAsync(int idEvaluacion);

        Task<ResultDTO<EvaluacionResponseDTO>> CreateAsync(CreateEvaluacionDTO dto);

        Task<ResultDTO<EvaluacionResponseDTO>> UpdateAsync(int idEvaluacion, UpdateEvaluacionDTO dto);

        Task<ResultDTO<bool>> DeleteAsync(int idEvaluacion);
    }
}