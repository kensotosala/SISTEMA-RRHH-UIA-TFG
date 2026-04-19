using BusinessLayer.DTOs;
using BusinessLogicLayer.DTOs;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Interfaces
{
    public interface IUsuariosManager
    {
        Task<ResultadoOperacion<List<UsuarioDTO>>> ObtenerTodosAsync();

        Task<ResultadoOperacion<UsuarioDTO>> ObtenerPorIdAsync(int idUsuario);

        Task<ResultadoOperacion<UsuarioDTO>> CrearAsync(CrearUsuarioDTO crearUsuarioDTO);

        Task<ResultadoOperacion<UsuarioDTO>> ActualizarAsync(ActualizarUsuarioDTO actualizarDTO);

        Task<ResultadoOperacion<bool>> EliminarAsync(int idUsuario);

        Task<ResultadoOperacion<UsuarioDTO>> AutenticarAsync(LoginDTO loginDTO);

        Task<ResultadoOperacion<bool>> CambiarEstadoAsync(int idUsuario, string nuevoEstado);

        Task<ResultadoOperacion<IEnumerable<UsuarioDTO>>> ListarEmpleadosAdmin();
    }
}