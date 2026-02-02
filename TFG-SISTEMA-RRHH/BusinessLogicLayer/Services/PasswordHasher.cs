using BusinessLogicLayer.Interfaces;

namespace BusinessLogicLayer.Services
{
    /// <summary>
    /// Implementación de hash de contraseñas usando BCrypt
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        /// <summary>
        /// Genera un hash BCrypt de la contraseña
        /// </summary>
        public string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        /// <summary>
        /// Verifica si la contraseña coincide con el hash BCrypt
        /// </summary>
        public bool Verify(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (string.IsNullOrWhiteSpace(hash))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (Exception ex)
            {
                // Log del error para debugging
                Console.WriteLine($"Error al verificar password: {ex.Message}");
                return false;
            }
        }
    }
}