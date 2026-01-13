using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.DTOs
{
    // DTO para listar asistencias
    public class AsistenciaDTO
    {
        public int IdAsistencia { get; set; }
        public int EmpleadoId { get; set; }
        public string NombreEmpleado { get; set; } = null!;
        public string CodigoEmpleado { get; set; } = null!;
        public DateTime FechaRegistro { get; set; }
        public DateTime? HoraEntrada { get; set; }
        public DateTime? HoraSalida { get; set; }
        public string Estado { get; set; } = null!;
        public TimeSpan? HorasTrabajadas { get; set; }
    }

    // DTO para crear asistencia
    public class CrearAsistenciaDTO
    {
        [Required(ErrorMessage = "El ID del empleado es requerido")]
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "La fecha de registro es requerida")]
        public DateTime FechaRegistro { get; set; }

        public DateTime? HoraEntrada { get; set; }

        public DateTime? HoraSalida { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        [RegularExpression("^(PRESENTE|AUSENTE|TARDANZA|PERMISO)$",
            ErrorMessage = "Estado inválido. Debe ser: PRESENTE, AUSENTE, TARDANZA o PERMISO")]
        public string Estado { get; set; } = null!;
    }

    // DTO para actualizar asistencia
    public class ActualizarAsistenciaDTO
    {
        [Required(ErrorMessage = "El ID del empleado es requerido")]
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "La fecha de registro es requerida")]
        public DateTime FechaRegistro { get; set; }

        public DateTime? HoraEntrada { get; set; }

        public DateTime? HoraSalida { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        [RegularExpression("^(PRESENTE|AUSENTE|TARDANZA|PERMISO)$",
            ErrorMessage = "Estado inválido. Debe ser: PRESENTE, AUSENTE, TARDANZA o PERMISO")]
        public string Estado { get; set; } = null!;
    }

    // DTO para filtros de búsqueda
    public class FiltrosAsistenciaDTO
    {
        public int? EmpleadoId { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? Estado { get; set; }
        public int? DepartamentoId { get; set; }
    }

    // DTO para reporte de asistencias
    public class ReporteAsistenciaDTO
    {
        public int EmpleadoId { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public string Departamento { get; set; } = null!;
        public int TotalDias { get; set; }
        public int DiasPresente { get; set; }
        public int DiasAusente { get; set; }
        public int DiasTardanza { get; set; }
        public int DiasPermiso { get; set; }
        public decimal PorcentajeAsistencia { get; set; }
    }
}