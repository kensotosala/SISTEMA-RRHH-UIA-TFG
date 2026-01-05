namespace BusinessLogicLayer.DTOs
{
    public class CrearUsuarioDTO
    {
        public string NombreUsuario { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class ActualizarUsuarioDTO
    {
        public string NombreUsuario { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}