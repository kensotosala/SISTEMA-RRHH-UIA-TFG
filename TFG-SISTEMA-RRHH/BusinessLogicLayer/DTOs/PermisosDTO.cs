using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.DTOs
{
    public class ActualizarPermisoDTO
    {
        public bool? ConGoceSalario { get; set; }
        public int EmpleadoId { get; set; }
        public string? EstadoSolicitud { get; set; }
        public DateTime FechaPermiso { get; set; }

        public DateTime? FechaAprobacion { get; set; }

        public string Motivo { get; set; } = null!;
    }

    public class CrearPermisoDto
    {
        [Required(ErrorMessage = "El ID del empleado es requerido")]
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "La fecha del permiso es requerida")]
        public DateTime FechaPermiso { get; set; }

        [Required(ErrorMessage = "El motivo es requerido")]
        [MaxLength(500, ErrorMessage = "El motivo no puede exceder 500 caracteres")]
        public string Motivo { get; set; } = null!;

        public bool? ConGoceSalario { get; set; }
    }

    public class ListarPermisoByIdDTO
    {
        public string? ComentariosRechazo { get; set; }
        public bool? ConGoceSalario { get; set; }
        public int EmpleadoId { get; set; }
        public string? EstadoSolicitud { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public DateTime FechaPermiso { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public int IdPermiso { get; set; }
        public int? JefeApruebaId { get; set; }
        public string Motivo { get; set; } = null!;
    }

    public class ListarPermisosDTO
    {
        public string? ComentariosRechazo { get; set; }
        public bool? ConGoceSalario { get; set; }
        public int EmpleadoId { get; set; }
        public string? EstadoSolicitud { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public DateTime FechaPermiso { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public int IdPermiso { get; set; }
        public int? JefeApruebaId { get; set; }
        public string Motivo { get; set; } = null!;
    }
}