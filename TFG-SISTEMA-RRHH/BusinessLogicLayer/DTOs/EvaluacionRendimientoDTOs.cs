namespace BusinessLogicLayer.DTOs
{
    public class CreateDetalleEvaluacionDTO
    {
        public int IdMetrica { get; set; }
        public sbyte Puntuacion { get; set; }
        public string? Comentarios { get; set; }
    }

    public class UpdateDetalleEvaluacionDTO
    {
        public int IdDetalle { get; set; }
        public int IdMetrica { get; set; }
        public sbyte Puntuacion { get; set; }
        public string? Comentarios { get; set; }
    }

    public class DetalleEvaluacionResponseDTO
    {
        public int IdDetalle { get; set; }
        public int IdEvaluacion { get; set; }
        public int IdMetrica { get; set; }
        public string NombreMetrica { get; set; } = string.Empty;
        public sbyte Puntuacion { get; set; }
        public string? Comentarios { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }

    public class CreateEvaluacionDTO
    {
        public int EmpleadoId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int EvaluadorId { get; set; }
        public string? Comentarios { get; set; }
        public string Estado { get; set; } = "Pendiente";

        public List<CreateDetalleEvaluacionDTO> Detalles { get; set; } = new();
    }

    public class UpdateEvaluacionDTO
    {
        public int IdEvaluacion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int EvaluadorId { get; set; }
        public string? Comentarios { get; set; }
        public string Estado { get; set; } = string.Empty;

        public List<UpdateDetalleEvaluacionDTO> Detalles { get; set; } = new();
    }

    public class EvaluacionResponseDTO
    {
        public int IdEvaluacion { get; set; }
        public int EmpleadoId { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int EvaluadorId { get; set; }
        public string NombreEvaluador { get; set; } = string.Empty;
        public sbyte PuntuacionTotal { get; set; }
        public string? Comentarios { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public List<DetalleEvaluacionResponseDTO> Detalles { get; set; } = new();
    }
}