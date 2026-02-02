namespace BusinessLayer.DTOs
{
    /// <summary>
    /// DTO para crear un nuevo usuario
    /// </summary>
    public class CrearUsuarioDTO
    {
        public int EmpleadoId { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    /// <summary>
    /// DTO para actualizar un usuario
    /// </summary>
    public class ActualizarUsuarioDTO
    {
        public int IdUsuario { get; set; }
        public string? NombreUsuario { get; set; }
        public string? Password { get; set; }
        public string? Estado { get; set; }
    }

    /// <summary>
    /// DTO de respuesta de usuario
    /// </summary>
    public class UsuarioDTO
    {
        public int IdUsuario { get; set; }
        public int EmpleadoId { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public string? Estado { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string? NombreEmpleado { get; set; }
    }

    /// <summary>
    /// Resultado de operación
    /// </summary>
    public class ResultadoOperacion<T>
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public T? Datos { get; set; }

        public static ResultadoOperacion<T> Exito(T datos, string mensaje = "Operación exitosa")
        {
            return new ResultadoOperacion<T>
            {
                Exitoso = true,
                Mensaje = mensaje,
                Datos = datos
            };
        }

        public static ResultadoOperacion<T> Error(string mensaje)
        {
            return new ResultadoOperacion<T>
            {
                Exitoso = false,
                Mensaje = mensaje,
                Datos = default
            };
        }
    }
}