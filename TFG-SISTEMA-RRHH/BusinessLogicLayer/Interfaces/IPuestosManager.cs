
using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Interfaces
{
    public interface IPuestosManager
    {
        Task<IEnumerable<PuestoDTO>> GetAllPuestosAsync();
        Task<PuestoDTO> GetPuestoByIdAsync(int id);
        Task<PuestoDTO> CreatePuestoAsync(PuestoDTO dto);
        Task UpdatePuestoAsync(PuestoDTO dto);
        Task DeletePuestoAsync(int id);
    }
}