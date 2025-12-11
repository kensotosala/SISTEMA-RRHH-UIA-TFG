using BusinessLogicLayer.DTOs;

namespace ApplicationLayer.Interfaces
{
    public interface IPuestoService
    {
        Task<IEnumerable<PuestoDTO>> GetAllPuestosAsync();

        Task<PuestoDTO?> GetPuestoByIdAsync(int id);

        Task<PuestoDTO> CreatePuestoAsync(CrearPuestoDTO puestoDto);

        Task<bool> UpdatePuestoAsync(int id, ActualizarPuestoDTO puestoDto);

        Task<bool> DeletePuestoAsync(int id);
    }
}