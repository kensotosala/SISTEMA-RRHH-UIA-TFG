using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.DTOs
{
    public class ListarEmpleadoUsuarioDto
    {
        [Required]
        public DetalleEmpleadoDTO Empleado { get; set; } = null!;

        [Required]
        public CrearUsuarioDTO? Usuario { get; set; } = null!;
    }

    public class CrearEmpleadoUsuarioDto
    {
        [Required]
        public CrearEmpleadoDTO Empleado { get; set; } = null!;

        [Required]
        public CrearUsuarioDTO Usuario { get; set; } = null!;
    }

    public class ActualizarEmpleadoUsuarioDto
    {
        public CrearEmpleadoDTO Empleado { get; set; } = null!;
        public CrearUsuarioDTO Usuario { get; set; } = null!;
    }

    public class LoginDTO
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class AuthResponseDTO
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
    }
}