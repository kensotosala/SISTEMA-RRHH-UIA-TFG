using BusinessLogicLayer.Shared;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.DTOs
{
    public class DetalleEmpleadoDTO
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

        public TipoContrato TipoContrato { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }

    public class CrearEmpleadoDTO
    {
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string CodigoEmpleado { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string PrimerApellido { get; set; } = null!;

        [StringLength(50)]
        public string? SegundoApellido { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = null!;

        [Phone]
        [StringLength(20)]
        public string? Telefono { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly FechaContratacion { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "PuestoId debe ser mayor a 0.")]
        public int PuestoId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "DepartamentoId debe ser mayor a 0.")]
        public int DepartamentoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "JefeInmediatoId debe ser mayor a 0.")]
        public int? JefeInmediatoId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El salario no puede ser negativo.")]
        public decimal SalarioBase { get; set; }

        [Required]
        [EnumDataType(typeof(TipoContrato))]
        public TipoContrato TipoContrato { get; set; }
    }

    public class ActualizarEmpleadoDTO
    {
        [StringLength(20)]
        public string? CodigoEmpleado { get; set; }

        [StringLength(50)]
        public string? Nombre { get; set; }

        [StringLength(50)]
        public string? PrimerApellido { get; set; }

        [StringLength(50)]
        public string? SegundoApellido { get; set; }

        [EmailAddress]
        [StringLength(150)]
        public string? Email { get; set; }

        [Phone]
        [StringLength(20)]
        public string? Telefono { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? FechaContratacion { get; set; }

        [Range(1, int.MaxValue)]
        public int? PuestoId { get; set; }

        [Range(1, int.MaxValue)]
        public int? DepartamentoId { get; set; }

        [Range(1, int.MaxValue)]
        public int? JefeInmediatoId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? SalarioBase { get; set; }

        [EnumDataType(typeof(TipoContrato))]
        public TipoContrato? TipoContrato { get; set; }

        public string? Estado { get; set; }
    }
}