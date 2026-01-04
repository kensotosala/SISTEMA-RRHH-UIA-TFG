using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Interfaces
{
    public interface IPuestosManager
    {
        Task<IEnumerable<PuestoDTO>> GetAllPuestosAsync();

        Task<PuestoDTO?> GetPuestoByIdAsync(int id);

        Task<int> CreatePuestoAsync(CrearPuestoDTO dto);

        Task<bool> UpdatePuestoAsync(ActualizarPuestoDTO dto);

        Task<bool> DeletePuestoAsync(int id);
    }
}