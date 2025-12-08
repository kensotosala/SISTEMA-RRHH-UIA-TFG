using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Interfaces
{
    public interface IPuestosManager
    {
        Task<IEnumerable<PuestoDto>> GetAllPuestosAsync();
        Task<PuestoDto> GetPuestoByIdAsync(int id);
        Task<PuestoDto> CreatePuestoAsync(PuestoDto puestoDto);
        Task UpdatePuestoAsync(PuestoDto puestoDto);
        Task DeletePuestoAsync(int id);
    }
}
