using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.DTOs
{
    public class CrearUsuarioDTO
    {
        [Required]
        public string NombreUsuario { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [Required]
        public int RolId { get; set; }
    }

    public class ActualizarUsuarioDTO
    {
        [Required]
        public string NombreUsuario { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [Required]
        public int RolId { get; set; }
    }

    public class DetalleEmpleadoUsuarioDTO
    {
        public DetalleEmpleadoDTO Empleado { get; set; } = null!;
        public DetalleUsuarioDTO Usuario { get; set; } = null!;
    }

    public class DetalleUsuarioDTO
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public int RolId { get; set; }
    }
}