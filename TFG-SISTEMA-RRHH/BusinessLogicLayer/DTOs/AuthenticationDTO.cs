namespace BusinessLogicLayer.DTOs
{
    public class ListarEmpleadoUsuarioDto
    {
        public DetalleEmpleadoDTO Empleado { get; set; } = null!;
        public CrearUsuarioDTO? Usuario { get; set; } = null!;
    }

    public class CrearEmpleadoUsuarioDto
    {
        public CrearEmpleadoDTO Empleado { get; set; } = null!;
        public CrearUsuarioDTO Usuario { get; set; } = null!;
    }

    public class ActualizarEmpleadoUsuarioDto
    {
        public CrearEmpleadoDTO Empleado { get; set; } = null!;
        public CrearUsuarioDTO Usuario { get; set; } = null!;
    }
}