using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.DTOs
{
    public class ActualizarIncapacidadDto
    {
        [StringLength(500, ErrorMessage = "La ruta del archivo no puede exceder 500 caracteres")]
        public string? ArchivoAdjunto { get; set; }

        [StringLength(500, ErrorMessage = "El diagnóstico no puede exceder 500 caracteres")]
        public string? Diagnostico { get; set; }

        [Required(ErrorMessage = "El empleado es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del empleado debe ser válido")]
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "La fecha fin es requerida")]
        public DateTime FechaFin { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "El ID de la incapacidad es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID debe ser válido")]
        public int IncapacidadId { get; set; }
        [Required(ErrorMessage = "El tipo de incapacidad es requerido")]
        public string TipoIncapacidad { get; set; } = null!;
    }

    public class IncapacidadDto
    {
        public string? ArchivoAdjunto { get; set; }
        public string? Diagnostico { get; set; }
        public int EmpleadoId { get; set; }
        public string Estado { get; set; } = null!;
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaFin { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public int IdIncapacidad { get; set; }
        public string TipoIncapacidad { get; set; } = null!;
    }
    public class RegistrarIncapacidadDto
    {
        [StringLength(500, ErrorMessage = "La ruta del archivo no puede exceder 500 caracteres")]
        public string? ArchivoAdjunto { get; set; }

        [StringLength(500, ErrorMessage = "El diagnóstico no puede exceder 500 caracteres")]
        public string? Diagnostico { get; set; }

        [Required(ErrorMessage = "El empleado es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del empleado debe ser válido")]
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "La fecha fin es requerida")]
        public DateTime FechaFin { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        public DateTime FechaInicio { get; set; }
        [Required(ErrorMessage = "El tipo de incapacidad es requerido")]
        public string TipoIncapacidad { get; set; } = null!;
    }
}