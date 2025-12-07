using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class Asistencias
{
    public int IdAsistencia { get; set; }

    public int EmpleadoId { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime? HoraEntrada { get; set; }

    public DateTime? HoraSalida { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Empleados Empleado { get; set; } = null!;
}
