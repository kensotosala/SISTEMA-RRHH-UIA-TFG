using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.DTOs
{
    // DTO para actualizar asistencia
    public class ActualizarAsistenciaDTO
    {
        [Required(ErrorMessage = "El ID del empleado es requerido")]
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        [RegularExpression("^(PRESENTE|AUSENTE|TARDANZA|PERMISO)$",
            ErrorMessage = "Estado inválido. Debe ser: PRESENTE, AUSENTE, TARDANZA o PERMISO")]
        public string Estado { get; set; } = null!;

        [Required(ErrorMessage = "La fecha de registro es requerida")]
        public DateTime FechaRegistro { get; set; }

        public DateTime? HoraEntrada { get; set; }

        public DateTime? HoraSalida { get; set; }
    }

    // DTO para listar asistencias
    public class AsistenciaDTO
    {
        public string CodigoEmpleado { get; set; } = null!;
        public int EmpleadoId { get; set; }
        public string Estado { get; set; } = null!;
        public DateTime FechaRegistro { get; set; }
        public DateTime? HoraEntrada { get; set; }
        public DateTime? HoraSalida { get; set; }
        public TimeSpan? HorasTrabajadas { get; set; }
        public int IdAsistencia { get; set; }
        public string NombreEmpleado { get; set; } = null!;
    }

    // DTO para crear asistencia
    public class CrearAsistenciaDTO
    {
        [Required(ErrorMessage = "El ID del empleado es requerido")]
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        [RegularExpression("^(PRESENTE|AUSENTE|TARDANZA|PERMISO)$",
            ErrorMessage = "Estado inválido. Debe ser: PRESENTE, AUSENTE, TARDANZA o PERMISO")]
        public string Estado { get; set; } = null!;

        [Required(ErrorMessage = "La fecha de registro es requerida")]
        public DateTime FechaRegistro { get; set; }

        public DateTime? HoraEntrada { get; set; }

        public DateTime? HoraSalida { get; set; }
    }
    // DTO para estado de asistencia del día
    public class EstadoAsistenciaDTO
    {
        public string Estado { get; set; } = null!;
        public DateTime? HoraEntrada { get; set; }
        public DateTime? HoraSalida { get; set; }
        public string Mensaje { get; set; } = null!;
        public bool PuedeMarcarEntrada { get; set; }
        public bool PuedeMarcarSalida { get; set; }
        public bool TieneRegistro { get; set; }
    }

    // DTO para filtros de búsqueda
    public class FiltrosAsistenciaDTO
    {
        public int? DepartamentoId { get; set; }
        public int? EmpleadoId { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaFin { get; set; }
        public DateTime? FechaInicio { get; set; }
    }

    // DTO de respuesta al marcar asistencia
    public class MarcarAsistenciaRequest
    {
        [Required(ErrorMessage = "El ID del empleado es requerido")]
        public int EmpleadoId { get; set; }
    }

    // DTO de respuesta al marcar asistencia
    public class MarcarAsistenciaResponse
    {
        public string Accion { get; set; } = null!; // "ENTRADA", "SALIDA", "NINGUNA"
        public string Estado { get; set; } = null!;
        public bool Exito { get; set; }
        public DateTime Hora { get; set; }
        public DateTime? HoraEntrada { get; set; }
        public DateTime? HoraSalida { get; set; }
        public string Mensaje { get; set; } = null!;
    }

    // DTO para reporte de asistencias
    public class ReporteAsistenciaDTO
    {
        public string Departamento { get; set; } = null!;
        public int DiasAusente { get; set; }
        public int DiasPermiso { get; set; }
        public int DiasPresente { get; set; }
        public int DiasTardanza { get; set; }
        public int EmpleadoId { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public decimal PorcentajeAsistencia { get; set; }
        public int TotalDias { get; set; }
    }
}