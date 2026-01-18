namespace BusinessLogicLayer.DTOs
{
    public class ActualizarPermisoDTO
    {
        public int IdPermiso { get; set; }
        public bool? ConGoceSalario { get; set; }
        public int EmpleadoId { get; set; }
        public string? EstadoSolicitud { get; set; }
        public DateTime FechaPermiso { get; set; }

        public DateTime? FechaAprobacion { get; set; }

        public string Motivo { get; set; } = null!;
    }

    public class CrearPermisoDTO
    {
        public bool? ConGoceSalario { get; set; }
        public int EmpleadoId { get; set; }

        public DateTime FechaPermiso { get; set; }

        public string Motivo { get; set; } = null!;
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