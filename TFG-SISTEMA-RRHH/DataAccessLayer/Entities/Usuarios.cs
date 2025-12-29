using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class Usuarios
{
    public int IdUsuario { get; set; }

    public int EmpleadoId { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime? UltimoAcceso { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual ICollection<AuditoriaCambios> AuditoriaCambios { get; set; } = new List<AuditoriaCambios>();

    public virtual Empleados Empleado { get; set; } = null!;

    public virtual ICollection<UsuariosRoles> UsuariosRoles { get; set; } = new List<UsuariosRoles>();
}