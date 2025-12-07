using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class Puestos
{
    public int IdPuesto { get; set; }

    public string NombrePuesto { get; set; } = null!;

    public string? Descripcion { get; set; }

    public sbyte? NivelJerarquico { get; set; }

    public decimal? SalarioMinimo { get; set; }

    public decimal? SalarioMaximo { get; set; }

    public bool? Estado { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual ICollection<Empleados> Empleados { get; set; } = new List<Empleados>();
}
