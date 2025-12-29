using BusinessLogicLayer.DTOs;
using DataAccessLayer.Entities;

namespace ApplicationLayer.Interfaces
{
    public interface IAuthService
    {
        Task<string?> LoginAsync(LoginDTO login);

        Task<CrearPuestoDTO?> RegisterNewUser(RegistrarUsuarioDTO dto);
    }
}
