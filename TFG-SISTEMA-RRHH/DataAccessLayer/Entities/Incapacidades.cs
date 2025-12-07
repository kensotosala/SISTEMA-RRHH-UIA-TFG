using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class Incapacidades
{
    public int IdIncapacidad { get; set; }

    public int EmpleadoId { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public string TipoIncapacidad { get; set; } = null!;

    public string? Diagnostico { get; set; }

    public string? ArchivoAdjunto { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Empleados Empleado { get; set; } = null!;
}
