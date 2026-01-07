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
    }
}