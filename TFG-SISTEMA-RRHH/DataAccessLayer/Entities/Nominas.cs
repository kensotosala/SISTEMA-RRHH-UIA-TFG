using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class Nominas
{
    public int IdNomina { get; set; }

    public int EmpleadoId { get; set; }

    public DateTime PeriodoNomina { get; set; }

    public DateTime FechaPago { get; set; }

    public decimal SalarioBase { get; set; }

    public decimal? HorasExtras { get; set; }

    public decimal? MontoHorasExtra { get; set; }

    public decimal? Bonificaciones { get; set; }

    public decimal? Deducciones { get; set; }

    public decimal TotalBruto { get; set; }

    public decimal TotalNeto { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public virtual Empleados Empleado { get; set; } = null!;
}
