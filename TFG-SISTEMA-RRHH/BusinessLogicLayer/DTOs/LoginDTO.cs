namespace BusinessLogicLayer.DTOs
{
    public class LoginDTO
    {
        public string NombreUsuario { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;
    }
}