using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class SaldoVacaciones
{
    public int IdSaldo { get; set; }

    public int EmpleadoId { get; set; }

    public int Anio { get; set; }

    public int DiasAcumulados { get; set; }

    public int? DiasDisfrutados { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public virtual Empleados Empleado { get; set; } = null!;
}
