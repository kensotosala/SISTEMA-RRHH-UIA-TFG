using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class Aguinaldos
{
    public int IdAguinaldo { get; set; }

    public int EmpleadoId { get; set; }

    public DateTime FechaCalculo { get; set; }

    public int DiasTrabajados { get; set; }

    public decimal SalarioPromedio { get; set; }

    public decimal MontoAguinaldo { get; set; }

    public DateTime? FechaPago { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Empleados Empleado { get; set; } = null!;
}
