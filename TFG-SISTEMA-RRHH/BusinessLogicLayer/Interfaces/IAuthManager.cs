using BusinessLogicLayer.DTOs;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Interfaces
{
    /// <summary>
    /// Interfaz para el manager de autenticación
    /// </summary>
    public interface IAuthManager
    {
        /// <summary>
        /// Autentica un usuario y genera un token JWT
        /// </summary>
        Task<AuthResponseDTO?> LoginAsync(LoginDTO dto);

        /// <summary>
        /// Registra un nuevo usuario en el sistema
        /// </summary>
        Task<Usuarios?> RegistrarNuevoUsuario(Usuarios usuario);

        /// <summary>
        /// Valida las credenciales de un usuario
        /// </summary>
        Task<Usuarios?> ValidarCredencialesAsync(string username, string password);

        /// <summary>
        /// Cambia la contraseña de un usuario
        /// </summary>
        Task<bool> CambiarPasswordAsync(int userId, CambiarPasswordDTO dto);

        /// <summary>
        /// Verifica si un token JWT es válido
        /// </summary>
        Task<bool> ValidarTokenAsync(string token);
    }
}