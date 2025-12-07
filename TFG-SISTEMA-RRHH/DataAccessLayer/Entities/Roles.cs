using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class Roles
{
    public int IdRol { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool? Estado { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual ICollection<UsuariosRoles> UsuariosRoles { get; set; } = new List<UsuariosRoles>();
}
