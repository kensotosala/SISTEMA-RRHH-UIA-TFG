using BusinessLogicLayer.DTOs;

namespace ApplicationLayer.Interfaces
{
    public interface IDepartamentoService
    {
        Task<IEnumerable<DepartamentoDTO>> GetAllAsync();

        Task<DepartamentoDTO?> GetByIdAsync(int id);

        Task<DepartamentoDTO> CreateAsync(DepartamentoDTO dto);

        Task<bool> UpdateAsync(DepartamentoDTO dto);

        Task<bool> DeleteAsync(int id);
    }
}