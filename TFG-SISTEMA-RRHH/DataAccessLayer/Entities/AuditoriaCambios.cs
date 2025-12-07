using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class AuditoriaCambios
{
    public int IdAuditoria { get; set; }

    public string TablaAfectada { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public int UsuarioId { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Usuarios Usuario { get; set; } = null!;
}
