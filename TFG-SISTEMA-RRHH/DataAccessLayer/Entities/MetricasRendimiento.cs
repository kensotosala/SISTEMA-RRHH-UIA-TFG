using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class MetricasRendimiento
{
    public int IdMetrica { get; set; }

    public string NombreMetrica { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal Peso { get; set; }

    public bool? Estado { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual ICollection<DetalleEvaluaciones> DetalleEvaluaciones { get; set; } = new List<DetalleEvaluaciones>();
}
