using BusinessLogicLayer.DTOs;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Interfaces
{
    public interface IAuthManager
    {
        Task<Usuarios?> ValidarCredencialesAsync(string username, string password);
        Task<Usuarios?> RegistrarNuevoUsuario(Usuarios usuario);
        Task<AuthResponseDTO?> LoginAsync(LoginDTO dto);
    }
}
