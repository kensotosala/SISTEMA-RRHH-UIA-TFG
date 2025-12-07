using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class DetalleEvaluaciones
{
    public int IdDetalle { get; set; }

    public int IdEvaluacion { get; set; }

    public int IdMetrica { get; set; }

    public sbyte Puntuacion { get; set; }

    public string? Comentarios { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual EvaluacionesRendimiento IdEvaluacionNavigation { get; set; } = null!;

    public virtual MetricasRendimiento IdMetricaNavigation { get; set; } = null!;
}
