using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.DTOs
{
    /*
     * DTO para Registrar (POST)
     */

    public class CrearVacacionDTO
    {
        [Required(ErrorMessage = "El ID del empleado es requerido")]
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es requerida")]
        public DateTime FechaFin { get; set; }
    }

    /*
     * DTO para Actualizar (PUT/PATCH)
     */

    public class ActualizarVacacionDTO
    {
        public int EmpleadoId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string? EstadoSolicitud { get; set; }

        public DateTime? FechaAprobacon { get; set; }

        [MaxLength(1000, ErrorMessage = "Los comentarios no pueden exceder 1000 caracteres")]
        public string? ComentariosRechazo { get; set; }
    }

    /*
     * DTO para listar (GET ALL)
     */

    public class ListarVacacionesDTO
    {
        public int IdVacacion { get; set; }
        public int EmpleadoId { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string? EstadoSolicitud { get; set; }
        public int? JefeApruebaId { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public string? ComentariosRechazo { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }

    /*
     * DTO PARA OBTENER UNO (GET BY ID)
     */

    public class ListarVacacionByIdDTO
    {
        public int IdVacacion { get; set; }
        public int EmpleadoId { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string? EstadoSolicitud { get; set; }
        public int? JefeApruebaId { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public string? ComentariosRechazo { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public int DiasVacaciones => (FechaFin - FechaInicio).Days + 1;
    }

    /*
     * DTO PARA CONSULTAS Y SALDOS
     */

    public class ResultDTO<T>
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public T? Datos { get; set; }
        public List<string> Errores { get; set; } = new List<string>();

        public static ResultDTO<T> Success(T datos, string mensaje = "Operación exitosa")
        {
            return new ResultDTO<T>
            {
                Exitoso = true,
                Mensaje = mensaje,
                Datos = datos
            };
        }

        public static ResultDTO<T> Failure(string mensaje, List<string>? errores = null)
        {
            return new ResultDTO<T>
            {
                Exitoso = false,
                Mensaje = mensaje,
                Errores = errores ?? new List<string>()
            };
        }
    }

    // DTO para mostrar el saldo de vacaciones
    public class SaldoVacacionesDTO
    {
        public int EmpleadoId { get; set; }
        public string NombreEmpleado { get; set; } = null!;
        public int Anio { get; set; }
        public int DiasAcumulados { get; set; }
        public int DiasDisfrutados { get; set; }
        public int DiasDisponibles => DiasAcumulados - DiasDisfrutados;
        public int DiasPendientesAprobacion { get; set; }
        public string Mensaje { get; set; } = null!;
    }

    // DTO para resultado de validación de solicitud
    public class ValidacionVacacionesDTO
    {
        public bool EsValida { get; set; }
        public List<string> Errores { get; set; } = new List<string>();
        public List<string> Advertencias { get; set; } = new List<string>();
        public int DiasDisponibles { get; set; }
        public int DiasSolicitados { get; set; }
    }
}