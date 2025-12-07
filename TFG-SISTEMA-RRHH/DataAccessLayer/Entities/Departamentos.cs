using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class Departamentos
{
    public int IdDepartamento { get; set; }

    public string NombreDepartamento { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int? IdJefeDepartamento { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual ICollection<Empleados> Empleados { get; set; } = new List<Empleados>();

    public virtual Empleados? IdJefeDepartamentoNavigation { get; set; }
}
