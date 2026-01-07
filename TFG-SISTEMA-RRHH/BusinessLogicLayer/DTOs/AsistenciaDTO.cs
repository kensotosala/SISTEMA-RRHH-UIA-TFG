using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.DTOs
{
    public class MarcarAsistenciaRequest
    {
        [Required]
        public int EmpleadoId { get; set; }
    }

    public class MarcarAsistenciaResponse
    {
        public string Accion { get; set; } = null!;
        public DateTime Hora { get; set; }
        public string Mensaje { get; set; } = null!;
        public DateTime? HoraEntrada { get; set; }
        public DateTime? HoraSalida { get; set; }
        public string? Estado { get; set; }
        public bool Exito { get; set; }
    }

    public class EstadoAsistenciaDTO
    {
        public bool TieneRegistro { get; set; }
        public DateTime? HoraEntrada { get; set; }
        public DateTime? HoraSalida { get; set; }
        public string Estado { get; set; } = null!;
        public bool PuedeMarcarEntrada { get; set; }
        public bool PuedeMarcarSalida { get; set; }
        public string Mensaje { get; set; } = null!;
    }
}