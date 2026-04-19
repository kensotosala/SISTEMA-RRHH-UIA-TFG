using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Interfaces
{
    public interface IEmpleadosManager
    {
        Task<IEnumerable<DetalleEmpleadoDTO>> ListAsync();

        Task<IEnumerable<DetalleEmpleadoDTO>> ListarEmpleadoSinHorasExtraEnProcesoAsync();

        Task<DetalleEmpleadoDTO?> GetByIdAsync(int id);

        Task<DetalleEmpleadoDTO> CreateAsync(CrearEmpleadoYUsuarioDTO dto);

        Task UpdateAsync(int id, ActualizarEmpleadoDTO dto);

        Task DeleteAsync(int id);

        Task ReactivarAsync(int id);
    }
}