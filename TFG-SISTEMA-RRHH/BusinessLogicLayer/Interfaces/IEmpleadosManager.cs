using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Interfaces
{
    public interface IEmpleadosManager
    {
        Task<IEnumerable<EmpleadosDTO>> ListAsync();

        Task<EmpleadosDTO?> GetByIdAsync(int id);

        Task<EmpleadosDTO> CreateAsync(EmpleadoCreateDTO dto);

        Task UpdateAsync(int id, EmpleadosDTO dto);

        Task DeleteAsync(int id);
    }
}
