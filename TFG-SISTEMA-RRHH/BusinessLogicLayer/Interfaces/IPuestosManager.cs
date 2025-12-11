
using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Interfaces
{
    public interface IPuestosManager
    {
        Task<IEnumerable<PuestoDTO>> GetAllPuestosAsync();
        Task<PuestoDTO> GetPuestoByIdAsync(int id);
        Task<PuestoDTO> CreatePuestoAsync(PuestoDTO puestoDto);
        Task UpdatePuestoAsync(PuestoDTO puestoDto);
        Task DeletePuestoAsync(int id);
    }
}