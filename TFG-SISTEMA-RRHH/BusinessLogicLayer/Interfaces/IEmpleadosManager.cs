using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Interfaces
{
    public interface IEmpleadosManager
    {
        Task<IEnumerable<ListarEmpleadoUsuarioDto>> ListAsync();

        Task<ListarEmpleadoUsuarioDto?> GetByIdAsync(int id);

        Task<CrearEmpleadoUsuarioDto> CreateAsync(CrearEmpleadoUsuarioDto dto);

        Task UpdateAsync(int id, ActualizarEmpleadoUsuarioDto dto);

        Task DeleteAsync(int id);
    }
}