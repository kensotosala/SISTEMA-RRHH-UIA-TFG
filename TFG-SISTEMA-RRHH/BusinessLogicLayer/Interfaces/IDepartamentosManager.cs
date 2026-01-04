using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Interfaces
{
    public interface IDepartamentosManager
    {
        Task<IEnumerable<DepartamentoDTO>> ListAsync();

        Task<DepartamentoDTO> GetByIdAsync(int id);

        Task<DepartamentoDTO> CreateAsync(CrearDepartamentoDTO dto);

        Task UpdateAsync(int id, ActualizarDepartamentoDTO dto);

        Task DeleteAsync(int id);
    }
}