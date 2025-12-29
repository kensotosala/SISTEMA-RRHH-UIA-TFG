namespace BusinessLogicLayer.DTOs
{
    public class RegistrarUsuarioDTO
    {
        public int IdDepartamento { get; set; }
        public int EmpleadoId { get; set; }
        public required string NombreUsuario { get; set; }
        public required string Password { get; set; }

        public string CodigoEmpleado { get; set; } = null!;

        public string Nombre { get; set; } = null!;

        public string PrimerApellido { get; set; } = null!;

        public string? SegundoApellido { get; set; }

        public string Email { get; set; } = null!;

        public string? Telefono { get; set; }

        public DateOnly FechaContratacion { get; set; }
    }
}