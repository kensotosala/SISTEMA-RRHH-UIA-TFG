using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class HorasExtras
{
    public int IdHoraExtra { get; set; }

    public int EmpleadoId { get; set; }

    public DateTime FechaSolicitud { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public string? TipoHoraExtra { get; set; }

    public string Motivo { get; set; } = null!;

    public string? EstadoSolicitud { get; set; }

    public int? JefeApruebaId { get; set; }

    public DateTime? FechaAprobacion { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Empleados Empleado { get; set; } = null!;

    public virtual Empleados? JefeAprueba { get; set; }
}
