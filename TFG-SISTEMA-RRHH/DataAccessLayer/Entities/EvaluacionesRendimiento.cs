using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class EvaluacionesRendimiento
{
    public int IdEvaluacion { get; set; }

    public int EmpleadoId { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public int EvaluadorId { get; set; }

    public sbyte PuntuacionTotal { get; set; }

    public string? Comentarios { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual ICollection<DetalleEvaluaciones> DetalleEvaluaciones { get; set; } = new List<DetalleEvaluaciones>();

    public virtual Empleados Empleado { get; set; } = null!;

    public virtual Empleados Evaluador { get; set; } = null!;
}
