namespace BusinessLogicLayer.DTOs
{
    public class PuestoDto
    {
        public int IdPuesto { get; set; }
        public string NombrePuesto { get; set; } = null!;
        public string? Descripcion { get; set; }
        public int? NivelJerarquico { get; set; }
        public decimal? SalarioMinimo { get; set; }
        public decimal? SalarioMaximo { get; set; }
        public bool? Estado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}
