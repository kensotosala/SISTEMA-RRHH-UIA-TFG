using BusinessLogicLayer.Interfaces;

namespace BusinessLogicLayer.Managers
{
    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}