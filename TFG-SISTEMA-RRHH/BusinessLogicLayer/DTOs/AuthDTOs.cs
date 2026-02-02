using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BusinessLogicLayer.DTOs
{
    /// <summary>
    /// DTO para login de usuario
    /// </summary>
    public class LoginDTO
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder los 50 caracteres")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
        public string Password { get; set; } = null!;
    }

    /// <summary>
    /// DTO de respuesta de autenticación con JWT
    /// </summary>
    public class AuthResponseDTO
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public UsuarioInfoDTO? UsuarioInfo { get; set; }
    }

    /// <summary>
    /// DTO con información del usuario autenticado
    /// </summary>
    public class UsuarioInfoDTO
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public string? Email { get; set; }
        public int EmpleadoId { get; set; }
        public string? CodigoEmpleado { get; set; }
        public int DepartamentoId { get; set; }
        public string? NombreDepartamento { get; set; }
        public int PuestoId { get; set; }
        public string? NombrePuesto { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    /// <summary>
    /// DTO para registro de nuevo usuario
    /// </summary>
    public class RegistroUsuarioDTO
    {
        [Required(ErrorMessage = "El ID del empleado es requerido")]
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder los 50 caracteres")]
        public string NombreUsuario { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
        public string Password { get; set; } = null!;

        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = null!;
    }

    /// <summary>
    /// DTO para cambiar contraseña
    /// </summary>
    public class CambiarPasswordDTO
    {
        [Required(ErrorMessage = "La contraseña actual es requerida")]
        public string PasswordActual { get; set; } = null!;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
        public string NuevaPassword { get; set; } = null!;

        [Compare("NuevaPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarNuevaPassword { get; set; } = null!;
    }
}