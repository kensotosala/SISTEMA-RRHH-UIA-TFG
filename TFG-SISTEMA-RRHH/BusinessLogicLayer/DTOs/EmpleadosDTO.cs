namespace BusinessLogicLayer.DTOs
{
    public class EmpleadosDTO
    {
        public int IdEmpleado { get; set; }
        public string CodigoEmpleado { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string PrimerApellido { get; set; } = null!;
        public string? SegundoApellido { get; set; }
        public string Email { get; set; } = null!;
        public string? Telefono { get; set; }
        public DateOnly FechaContratacion { get; set; }
        public int PuestoId { get; set; }
        public int DepartamentoId { get; set; }
        public int? JefeInmediatoId { get; set; }
        public decimal SalarioBase { get; set; }
        public string TipoContrato { get; set; } = null!;
        public string? Estado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }

    public class EmpleadoCreateDTO
    {
        public string CodigoEmpleado { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string PrimerApellido { get; set; } = null!;
        public string? SegundoApellido { get; set; }
        public string Email { get; set; } = null!;
        public string? Telefono { get; set; }
        public DateOnly FechaContratacion { get; set; }
        public int PuestoId { get; set; }
        public int DepartamentoId { get; set; }
        public int? JefeInmediatoId { get; set; }
        public decimal SalarioBase { get; set; }
        public string TipoContrato { get; set; } = null!;
    }
}