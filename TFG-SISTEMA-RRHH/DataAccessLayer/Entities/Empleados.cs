namespace DataAccessLayer.Entities;

public partial class Empleados
{
    public int IdEmpleado { get; set; }

    public string CodigoEmpleado { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string PrimerApellido { get; set; } = null!;

    public string? SegundoApellido { get; set; }

    public string Email { get; set; } = null!;

    public string? Telefono { get; set; }

    public DateOnly FechaContratacion { get; set; }

    public int PuestoId { get; set; }

    public int DepartamentoId { get; set; }

    public int? JefeInmediatoId { get; set; }

    public decimal SalarioBase { get; set; }

    public string TipoContrato { get; set; } = null!;

    public string? Estado { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual ICollection<Aguinaldos> Aguinaldos { get; set; } = new List<Aguinaldos>();

    public virtual ICollection<Asistencias> Asistencias { get; set; } = new List<Asistencias>();

    public virtual Departamentos Departamento { get; set; } = null!;

    public virtual ICollection<Departamentos> Departamentos { get; set; } = new List<Departamentos>();

    public virtual ICollection<EvaluacionesRendimiento> EvaluacionesRendimientoEmpleado { get; set; } = new List<EvaluacionesRendimiento>();

    public virtual ICollection<EvaluacionesRendimiento> EvaluacionesRendimientoEvaluador { get; set; } = new List<EvaluacionesRendimiento>();

    public virtual ICollection<HorasExtras> HorasExtrasEmpleado { get; set; } = new List<HorasExtras>();

    public virtual ICollection<HorasExtras> HorasExtrasJefeAprueba { get; set; } = new List<HorasExtras>();

    public virtual ICollection<Incapacidades> Incapacidades { get; set; } = new List<Incapacidades>();

    public virtual ICollection<Empleados> InverseJefeInmediato { get; set; } = new List<Empleados>();

    public virtual Empleados? JefeInmediato { get; set; }

    public virtual ICollection<Liquidaciones> Liquidaciones { get; set; } = new List<Liquidaciones>();

    public virtual ICollection<Nominas> Nominas { get; set; } = new List<Nominas>();

    public virtual ICollection<Permisos> PermisosEmpleado { get; set; } = new List<Permisos>();

    public virtual ICollection<Permisos> PermisosJefeAprueba { get; set; } = new List<Permisos>();

    public virtual Puestos Puesto { get; set; } = null!;

    public virtual ICollection<SaldoVacaciones> SaldoVacaciones { get; set; } = new List<SaldoVacaciones>();

    public virtual Usuarios? Usuarios { get; set; }

    public virtual ICollection<Vacaciones> VacacionesEmpleado { get; set; } = new List<Vacaciones>();

    public virtual ICollection<Vacaciones> VacacionesJefeAprueba { get; set; } = new List<Vacaciones>();
}
