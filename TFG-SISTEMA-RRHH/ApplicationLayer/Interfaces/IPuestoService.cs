using BusinessLogicLayer.DTOs;

namespace ApplicationLayer.Interfaces
{
    public interface IPuestoService
    {
        Task<IEnumerable<PuestoDto>> GetAllPuestosAsync();
        Task<PuestoDto> GetPuestoByIdAsync(int id);
        Task<PuestoDto> CreatePuestoAsync(PuestoDto puesto);
        Task UpdatePuestoAsync(int id, PuestoDto puesto);
        Task DeletePuestoAsync(int id);
    }
}
