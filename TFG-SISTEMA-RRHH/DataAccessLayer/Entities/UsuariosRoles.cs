using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class UsuariosRoles
{
    public int IdUsuarioRol { get; set; }

    public string? EstadoSolicitud { get; set; }

    public int UsuarioId { get; set; }

    public int RolId { get; set; }

    public DateTime? FechaAsignacion { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Roles Rol { get; set; } = null!;

    public virtual Usuarios Usuario { get; set; } = null!;
}
