using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.DTOs
{
    // DTO para listar horas extras
    public class HoraExtraDTO
    {
        public int IdHoraExtra { get; set; }
        public int EmpleadoId { get; set; }
        public string CodigoEmpleado { get; set; } = null!;
        public string NombreEmpleado { get; set; } = null!;
        public DateTime FechaSolicitud { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public TimeSpan HorasTotales { get; set; }
        public string? TipoHoraExtra { get; set; }
        public string Motivo { get; set; } = null!;
        public string? EstadoSolicitud { get; set; }
        public int? JefeApruebaId { get; set; }
        public string? NombreJefe { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public DateTime? FechaCreacion { get; set; }
    }

    // DTO para crear hora extra (MODIFICADO PARA RECIBIR STRINGS)
    public class CrearHoraExtraDTO
    {
        [Required(ErrorMessage = "El ID del empleado es requerido")]
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        public string FechaInicio { get; set; } = null!;

        [Required(ErrorMessage = "La fecha de fin es requerida")]
        public string FechaFin { get; set; } = null!;

        // NO SE USA - La columna tipo_hora_extra en BD tiene valores de estado
        // Se mantiene para compatibilidad con frontend pero no se guarda
        public string? TipoHoraExtra { get; set; }

        [Required(ErrorMessage = "El motivo es requerido")]
        [StringLength(255, ErrorMessage = "El motivo no puede exceder 255 caracteres")]
        public string Motivo { get; set; } = null!;

        public int? JefeApruebaId { get; set; }
    }

    // DTO para actualizar hora extra (MODIFICADO PARA RECIBIR STRINGS)
    public class ActualizarHoraExtraDTO
    {
        [Required(ErrorMessage = "El ID del empleado es requerido")]
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        public string FechaInicio { get; set; } = null!;

        [Required(ErrorMessage = "La fecha de fin es requerida")]
        public string FechaFin { get; set; } = null!;

        // NO SE USA - Ver comentario en CrearHoraExtraDTO
        public string? TipoHoraExtra { get; set; }

        [Required(ErrorMessage = "El motivo es requerido")]
        [StringLength(255, ErrorMessage = "El motivo no puede exceder 255 caracteres")]
        public string Motivo { get; set; } = null!;

        [RegularExpression("^(PENDIENTE|APROBADA|RECHAZADA)$",
            ErrorMessage = "Estado inválido. Debe ser: PENDIENTE, APROBADA o RECHAZADA")]
        public string? EstadoSolicitud { get; set; }

        public int? JefeApruebaId { get; set; }
    }

    // DTO para aprobar/rechazar hora extra
    public class AprobarRechazarHoraExtraDTO
    {
        [Required(ErrorMessage = "El ID del jefe que aprueba es requerido")]
        public int JefeApruebaId { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        [RegularExpression("^(APROBADA|RECHAZADA)$",
            ErrorMessage = "Estado inválido. Debe ser: APROBADA o RECHAZADA")]
        public string EstadoSolicitud { get; set; } = null!;
    }

    // DTO para filtros de búsqueda
    public class FiltrosHorasExtrasDTO
    {
        public int? EmpleadoId { get; set; }
        public int? DepartamentoId { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? EstadoSolicitud { get; set; }
        public int? JefeApruebaId { get; set; }
    }

    // DTO para reporte de horas extras
    public class ReporteHorasExtrasDTO
    {
        public int EmpleadoId { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public string Departamento { get; set; } = null!;
        public int TotalSolicitudes { get; set; }
        public int SolicitudesPendientes { get; set; }
        public int SolicitudesAprobadas { get; set; }
        public int SolicitudesRechazadas { get; set; }
        public TimeSpan TotalHorasAprobadas { get; set; }
        public TimeSpan TotalHorasSolicitadas { get; set; }
    }
}