using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class Liquidaciones
{
    public int IdLiquidacion { get; set; }

    public int EmpleadoId { get; set; }

    public DateTime FechaLiquidacion { get; set; }

    public string MotivoLiquidacion { get; set; } = null!;

    public decimal SalarioBase { get; set; }

    public decimal? VacacionesPendientes { get; set; }

    public decimal? AguinaldoProporcional { get; set; }

    public decimal? Indemnizacion { get; set; }

    public decimal? OtrosConceptos { get; set; }

    public decimal TotalLiquidacion { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Empleados Empleado { get; set; } = null!;
}
