namespace BusinessLogicLayer.DTOs
{
    public class DepartamentoDTO
    {
        public int IdDepartamento { get; set; }
        public string NombreDepartamento { get; set; } = null!;

        public string? Descripcion { get; set; }

        public int? IdJefeDepartamento { get; set; }

        public string? Estado { get; set; }
    }
}