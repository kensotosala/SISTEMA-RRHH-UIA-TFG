using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class Permisos
{
    public int IdPermiso { get; set; }

    public int EmpleadoId { get; set; }

    public DateTime FechaSolicitud { get; set; }

    public DateTime FechaPermiso { get; set; }

    public string Motivo { get; set; } = null!;

    public bool? ConGoceSalario { get; set; }

    public string? EstadoSolicitud { get; set; }

    public int? JefeApruebaId { get; set; }

    public DateTime? FechaAprobacion { get; set; }

    public string? ComentariosRechazo { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Empleados Empleado { get; set; } = null!;

    public virtual Empleados? JefeAprueba { get; set; }
}
