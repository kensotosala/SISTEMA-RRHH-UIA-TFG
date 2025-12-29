using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Interfaces
{
    public interface IDepartamentosManager
    {
        Task<IEnumerable<DepartamentoDTO>> ListAsync();

        Task<DepartamentoDTO> GetByIdAsync(int id);

        Task<DepartamentoDTO> CreateAsync(DepartamentoDTO dto);

        Task UpdateAsync(DepartamentoDTO dto);

        Task DeleteAsync(int id);
    }
}