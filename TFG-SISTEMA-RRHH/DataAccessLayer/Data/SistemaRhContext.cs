using System;
using System.Collections.Generic;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace DataAccessLayer.Data;

public partial class SistemaRhContext : DbContext
{
    public SistemaRhContext()
    {
    }

    public SistemaRhContext(DbContextOptions<SistemaRhContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Aguinaldos> Aguinaldos { get; set; }

    public virtual DbSet<Asistencias> Asistencias { get; set; }

    public virtual DbSet<AuditoriaCambios> AuditoriaCambios { get; set; }

    public virtual DbSet<Departamentos> Departamentos { get; set; }

    public virtual DbSet<DetalleEvaluaciones> DetalleEvaluaciones { get; set; }

    public virtual DbSet<Empleados> Empleados { get; set; }

    public virtual DbSet<EvaluacionesRendimiento> EvaluacionesRendimiento { get; set; }

    public virtual DbSet<HorasExtras> HorasExtras { get; set; }

    public virtual DbSet<Incapacidades> Incapacidades { get; set; }

    public virtual DbSet<Liquidaciones> Liquidaciones { get; set; }

    public virtual DbSet<MetricasRendimiento> MetricasRendimiento { get; set; }

    public virtual DbSet<Nominas> Nominas { get; set; }

    public virtual DbSet<Permisos> Permisos { get; set; }

    public virtual DbSet<Puestos> Puestos { get; set; }

    public virtual DbSet<Roles> Roles { get; set; }

    public virtual DbSet<SaldoVacaciones> SaldoVacaciones { get; set; }

    public virtual DbSet<Usuarios> Usuarios { get; set; }

    public virtual DbSet<UsuariosRoles> UsuariosRoles { get; set; }

    public virtual DbSet<Vacaciones> Vacaciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Aguinaldos>(entity =>
        {
            entity.HasKey(e => e.IdAguinaldo).HasName("PRIMARY");

            entity.ToTable("aguinaldos");

            entity.HasIndex(e => new { e.EmpleadoId, e.Estado }, "idx_empleado_estado");

            entity.HasIndex(e => e.FechaPago, "idx_fecha_pago");

            entity.Property(e => e.IdAguinaldo).HasColumnName("id_aguinaldo");
            entity.Property(e => e.DiasTrabajados).HasColumnName("dias_trabajados");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'CALCULADO'")
                .HasColumnType("enum('CALCULADO','PAGADO')")
                .HasColumnName("estado");
            entity.Property(e => e.FechaCalculo)
                .HasColumnType("datetime")
                .HasColumnName("fecha_calculo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_modificacion");
            entity.Property(e => e.FechaPago)
                .HasColumnType("datetime")
                .HasColumnName("fecha_pago");
            entity.Property(e => e.MontoAguinaldo)
                .HasPrecision(10, 2)
                .HasColumnName("monto_aguinaldo");
            entity.Property(e => e.SalarioPromedio)
                .HasPrecision(10, 2)
                .HasColumnName("salario_promedio");

            entity.HasOne(d => d.Empleado).WithMany(p => p.Aguinaldos)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("aguinaldos_ibfk_1");
        });

        modelBuilder.Entity<Asistencias>(entity =>
        {
            entity.HasKey(e => e.IdAsistencia).HasName("PRIMARY");

            entity.ToTable("asistencias");

            entity.HasIndex(e => new { e.FechaRegistro, e.Estado }, "idx_fecha_estado");

            entity.HasIndex(e => new { e.EmpleadoId, e.FechaRegistro }, "unique_empleado_fecha").IsUnique();

            entity.Property(e => e.IdAsistencia).HasColumnName("id_asistencia");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'PRESENTE'")
                .HasColumnType("enum('PRESENTE','AUSENTE','TARDANZA','PERMISO')")
                .HasColumnName("estado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_modificacion");
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.HoraEntrada)
                .HasColumnType("datetime")
                .HasColumnName("hora_entrada");
            entity.Property(e => e.HoraSalida)
                .HasColumnType("datetime")
                .HasColumnName("hora_salida");

            entity.HasOne(d => d.Empleado).WithMany(p => p.Asistencias)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("asistencias_ibfk_1");
        });

        modelBuilder.Entity<AuditoriaCambios>(entity =>
        {
            entity.HasKey(e => e.IdAuditoria).HasName("PRIMARY");

            entity.ToTable("auditoria_cambios");

            entity.HasIndex(e => new { e.TablaAfectada, e.FechaCreacion }, "idx_tabla_fecha");

            entity.HasIndex(e => e.UsuarioId, "usuario_id");

            entity.Property(e => e.IdAuditoria).HasColumnName("id_auditoria");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_modificacion");
            entity.Property(e => e.TablaAfectada)
                .HasMaxLength(50)
                .HasColumnName("tabla_afectada");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Usuario).WithMany(p => p.AuditoriaCambios)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("auditoria_cambios_ibfk_1");
        });

        modelBuilder.Entity<Departamentos>(entity =>
        {
            entity.HasKey(e => e.IdDepartamento).HasName("PRIMARY");

            entity.ToTable("departamentos");

            entity.HasIndex(e => e.IdJefeDepartamento, "id_jefe_departamento");

            entity.Property(e => e.IdDepartamento).HasColumnName("id_departamento");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'ACTIVO'")
                .HasColumnType("enum('ACTIVO','INACTIVO')")
                .HasColumnName("estado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdJefeDepartamento).HasColumnName("id_jefe_departamento");
            entity.Property(e => e.NombreDepartamento)
                .HasMaxLength(100)
                .HasColumnName("nombre_departamento");

            entity.HasOne(d => d.IdJefeDepartamentoNavigation).WithMany(p => p.Departamentos)
                .HasForeignKey(d => d.IdJefeDepartamento)
                .HasConstraintName("departamentos_ibfk_1");
        });

        modelBuilder.Entity<DetalleEvaluaciones>(entity =>
        {
            entity.HasKey(e => e.IdDetalle).HasName("PRIMARY");

            entity.ToTable("detalle_evaluaciones");

            entity.HasIndex(e => e.IdMetrica, "id_metrica");

            entity.HasIndex(e => new { e.IdEvaluacion, e.IdMetrica }, "unique_evaluacion_metrica").IsUnique();

            entity.Property(e => e.IdDetalle).HasColumnName("id_detalle");
            entity.Property(e => e.Comentarios)
                .HasMaxLength(255)
                .HasColumnName("comentarios");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_modificacion");
            entity.Property(e => e.IdEvaluacion).HasColumnName("id_evaluacion");
            entity.Property(e => e.IdMetrica).HasColumnName("id_metrica");
            entity.Property(e => e.Puntuacion).HasColumnName("puntuacion");

            entity.HasOne(d => d.IdEvaluacionNavigation).WithMany(p => p.DetalleEvaluaciones)
                .HasForeignKey(d => d.IdEvaluacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("detalle_evaluaciones_ibfk_1");

            entity.HasOne(d => d.IdMetricaNavigation).WithMany(p => p.DetalleEvaluaciones)
                .HasForeignKey(d => d.IdMetrica)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("detalle_evaluaciones_ibfk_2");
        });

        modelBuilder.Entity<Empleados>(entity =>
        {
            entity.HasKey(e => e.IdEmpleado).HasName("PRIMARY");

            entity.ToTable("empleados");

            entity.HasIndex(e => e.CodigoEmpleado, "codigo_empleado").IsUnique();

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.HasIndex(e => e.DepartamentoId, "idx_departamento");

            entity.HasIndex(e => e.PuestoId, "idx_puesto");

            entity.HasIndex(e => e.JefeInmediatoId, "jefe_inmediato_id");

            entity.Property(e => e.IdEmpleado).HasColumnName("id_empleado");
            entity.Property(e => e.CodigoEmpleado)
                .HasMaxLength(20)
                .HasColumnName("codigo_empleado");
            entity.Property(e => e.DepartamentoId).HasColumnName("departamento_id");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'ACTIVO'")
                .HasColumnType("enum('ACTIVO','INACTIVO','LICENCIA')")
                .HasColumnName("estado");
            entity.Property(e => e.FechaContratacion).HasColumnName("fecha_contratacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_modificacion");
            entity.Property(e => e.JefeInmediatoId).HasColumnName("jefe_inmediato_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(20)
                .HasColumnName("nombre");
            entity.Property(e => e.PrimerApellido)
                .HasMaxLength(20)
                .HasColumnName("primer_apellido");
            entity.Property(e => e.PuestoId).HasColumnName("puesto_id");
            entity.Property(e => e.SalarioBase)
                .HasPrecision(10, 2)
                .HasColumnName("salario_base");
            entity.Property(e => e.SegundoApellido)
                .HasMaxLength(20)
                .HasColumnName("segundo_apellido");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");
            entity.Property(e => e.TipoContrato)
                .HasColumnType("enum('FIJO','TEMPORAL','PRUEBA')")
                .HasColumnName("tipo_contrato");

            entity.HasOne(d => d.Departamento).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.DepartamentoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("empleados_ibfk_2");

            entity.HasOne(d => d.JefeInmediato).WithMany(p => p.InverseJefeInmediato)
                .HasForeignKey(d => d.JefeInmediatoId)
                .HasConstraintName("empleados_ibfk_3");

            entity.HasOne(d => d.Puesto).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.PuestoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("empleados_ibfk_1");
        });

        modelBuilder.Entity<EvaluacionesRendimiento>(entity =>
        {
            entity.HasKey(e => e.IdEvaluacion).HasName("PRIMARY");

            entity.ToTable("evaluaciones_rendimiento");

            entity.HasIndex(e => e.EvaluadorId, "evaluador_id");

            entity.HasIndex(e => new { e.EmpleadoId, e.FechaInicio, e.FechaFin }, "idx_empleado_periodo");

            entity.Property(e => e.IdEvaluacion).HasColumnName("id_evaluacion");
            entity.Property(e => e.Comentarios)
                .HasMaxLength(255)
                .HasColumnName("comentarios");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'BORRADOR'")
                .HasColumnType("enum('BORRADOR','COMPLETADA')")
                .HasColumnName("estado");
            entity.Property(e => e.EvaluadorId).HasColumnName("evaluador_id");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio)
                .HasColumnType("datetime")
                .HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_modificacion");
            entity.Property(e => e.PuntuacionTotal).HasColumnName("puntuacion_total");

            entity.HasOne(d => d.Empleado).WithMany(p => p.EvaluacionesRendimientoEmpleado)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("evaluaciones_rendimiento_ibfk_1");

            entity.HasOne(d => d.Evaluador).WithMany(p => p.EvaluacionesRendimientoEvaluador)
                .HasForeignKey(d => d.EvaluadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("evaluaciones_rendimiento_ibfk_2");
        });

        modelBuilder.Entity<HorasExtras>(entity =>
        {
            entity.HasKey(e => e.IdHoraExtra).HasName("PRIMARY");

            entity.ToTable("horas_extras");

            entity.HasIndex(e => new { e.EmpleadoId, e.FechaInicio }, "idx_empleado_fecha");

            entity.HasIndex(e => e.JefeApruebaId, "jefe_aprueba_id");

            entity.Property(e => e.IdHoraExtra).HasColumnName("id_hora_extra");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.EstadoSolicitud)
                .HasDefaultValueSql("'PENDIENTE'")
                .HasColumnType("enum('PENDIENTE','APROBADA','RECHAZADA')")
                .HasColumnName("estado_solicitud");
            entity.Property(e => e.FechaAprobacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_aprobacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio)
                .HasColumnType("datetime")
                .HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_modificacion");
            entity.Property(e => e.FechaSolicitud)
                .HasColumnType("datetime")
                .HasColumnName("fecha_solicitud");
            entity.Property(e => e.JefeApruebaId).HasColumnName("jefe_aprueba_id");
            entity.Property(e => e.Motivo)
                .HasMaxLength(255)
                .HasColumnName("motivo");
            entity.Property(e => e.TipoHoraExtra)
                .HasColumnType("enum('PENDIENTE','APROBADA','RECHAZADA')")
                .HasColumnName("tipo_hora_extra");

            entity.HasOne(d => d.Empleado).WithMany(p => p.HorasExtrasEmpleado)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("horas_extras_ibfk_1");

            entity.HasOne(d => d.JefeAprueba).WithMany(p => p.HorasExtrasJefeAprueba)
                .HasForeignKey(d => d.JefeApruebaId)
                .HasConstraintName("horas_extras_ibfk_2");
        });

        modelBuilder.Entity<Incapacidades>(entity =>
        {
            entity.HasKey(e => e.IdIncapacidad).HasName("PRIMARY");

            entity.ToTable("incapacidades");

            entity.HasIndex(e => new { e.EmpleadoId, e.FechaInicio, e.FechaFin }, "idx_empleado_fechas");

            entity.Property(e => e.IdIncapacidad).HasColumnName("id_incapacidad");
            entity.Property(e => e.ArchivoAdjunto)
                .HasMaxLength(1000)
                .HasColumnName("archivo_adjunto");
            entity.Property(e => e.Diagnostico)
                .HasMaxLength(255)
                .HasColumnName("diagnostico");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'ACTIVA'")
                .HasColumnType("enum('ACTIVA','FINALIZADA')")
                .HasColumnName("estado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio)
                .HasColumnType("datetime")
                .HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_modificacion");
            entity.Property(e => e.TipoIncapacidad)
                .HasColumnType("enum('ENFERMEDAD','ACCIDENTE','MATERNIDAD','PATERNIDAD')")
                .HasColumnName("tipo_incapacidad");

            entity.HasOne(d => d.Empleado).WithMany(p => p.Incapacidades)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("incapacidades_ibfk_1");
        });

        modelBuilder.Entity<Liquidaciones>(entity =>
        {
            entity.HasKey(e => e.IdLiquidacion).HasName("PRIMARY");

            entity.ToTable("liquidaciones");

            entity.HasIndex(e => new { e.EmpleadoId, e.FechaLiquidacion }, "idx_empleado_fecha");

            entity.Property(e => e.IdLiquidacion).HasColumnName("id_liquidacion");
            entity.Property(e => e.AguinaldoProporcional)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("aguinaldo_proporcional");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'CALCULADA'")
                .HasColumnType("enum('CALCULADA','PAGADA')")
                .HasColumnName("estado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaLiquidacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_liquidacion");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_modificacion");
            entity.Property(e => e.Indemnizacion)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("indemnizacion");
            entity.Property(e => e.MotivoLiquidacion)
                .HasColumnType("enum('RENUNCIA','DESPIDO','DESPIDO_JUSTIFICADO','JUBILACION')")
                .HasColumnName("motivo_liquidacion");
            entity.Property(e => e.OtrosConceptos)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("otros_conceptos");
            entity.Property(e => e.SalarioBase)
                .HasPrecision(10, 2)
                .HasColumnName("salario_base");
            entity.Property(e => e.TotalLiquidacion)
                .HasPrecision(10, 2)
                .HasColumnName("total_liquidacion");
            entity.Property(e => e.VacacionesPendientes)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("vacaciones_pendientes");

            entity.HasOne(d => d.Empleado).WithMany(p => p.Liquidaciones)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("liquidaciones_ibfk_1");
        });

        modelBuilder.Entity<MetricasRendimiento>(entity =>
        {
            entity.HasKey(e => e.IdMetrica).HasName("PRIMARY");

            entity.ToTable("metricas_rendimiento");

            entity.HasIndex(e => e.Estado, "idx_estado");

            entity.Property(e => e.IdMetrica).HasColumnName("id_metrica");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'1'")
                .HasColumnName("estado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_modificacion");
            entity.Property(e => e.NombreMetrica)
                .HasMaxLength(100)
                .HasColumnName("nombre_metrica");
            entity.Property(e => e.Peso)
                .HasPrecision(3, 2)
                .HasColumnName("peso");
        });

        modelBuilder.Entity<Nominas>(entity =>
        {
            entity.HasKey(e => e.IdNomina).HasName("PRIMARY");

            entity.ToTable("nominas");

            entity.HasIndex(e => new { e.EmpleadoId, e.PeriodoNomina }, "idx_empleado_periodo");

            entity.Property(e => e.IdNomina).HasColumnName("id_nomina");
            entity.Property(e => e.Bonificaciones)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("bonificaciones");
            entity.Property(e => e.Deducciones)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("deducciones");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'PENDIENTE'")
                .HasColumnType("enum('PENDIENTE','PAGADA','ANULADA')")
                .HasColumnName("estado");
            entity.Property(e => e.FechaActualizacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaPago)
                .HasColumnType("datetime")
                .HasColumnName("fecha_pago");
            entity.Property(e => e.HorasExtras)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("horas_extras");
            entity.Property(e => e.MontoHorasExtra)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("monto_horas_extra");
            entity.Property(e => e.PeriodoNomina)
                .HasColumnType("datetime")
                .HasColumnName("periodo_nomina");
            entity.Property(e => e.SalarioBase)
                .HasPrecision(10, 2)
                .HasColumnName("salario_base");
            entity.Property(e => e.TotalBruto)
                .HasPrecision(10, 2)
                .HasColumnName("total_bruto");
            entity.Property(e => e.TotalNeto)
                .HasPrecision(10, 2)
                .HasColumnName("total_neto");

            entity.HasOne(d => d.Empleado).WithMany(p => p.Nominas)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("nominas_ibfk_1");
        });

        modelBuilder.Entity<Permisos>(entity =>
        {
            entity.HasKey(e => e.IdPermiso).HasName("PRIMARY");

            entity.ToTable("permisos");

            entity.HasIndex(e => new { e.EmpleadoId, e.FechaPermiso }, "idx_empleado_fecha");

            entity.HasIndex(e => e.JefeApruebaId, "jefe_aprueba_id");

            entity.Property(e => e.IdPermiso).HasColumnName("id_permiso");
            entity.Property(e => e.ComentariosRechazo)
                .HasMaxLength(255)
                .HasColumnName("comentarios_rechazo");
            entity.Property(e => e.ConGoceSalario)
                .HasDefaultValueSql("'0'")
                .HasColumnName("con_goce_salario");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.EstadoSolicitud)
                .HasDefaultValueSql("'PENDIENTE'")
                .HasColumnType("enum('PENDIENTE','APROBADA','RECHAZADA')")
                .HasColumnName("estado_solicitud");
            entity.Property(e => e.FechaAprobacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_aprobacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_modificacion");
            entity.Property(e => e.FechaPermiso)
                .HasColumnType("datetime")
                .HasColumnName("fecha_permiso");
            entity.Property(e => e.FechaSolicitud)
                .HasColumnType("datetime")
                .HasColumnName("fecha_solicitud");
            entity.Property(e => e.JefeApruebaId).HasColumnName("jefe_aprueba_id");
            entity.Property(e => e.Motivo)
                .HasMaxLength(255)
                .HasColumnName("motivo");

            entity.HasOne(d => d.Empleado).WithMany(p => p.PermisosEmpleado)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("permisos_ibfk_1");

            entity.HasOne(d => d.JefeAprueba).WithMany(p => p.PermisosJefeAprueba)
                .HasForeignKey(d => d.JefeApruebaId)
                .HasConstraintName("permisos_ibfk_2");
        });

        modelBuilder.Entity<Puestos>(entity =>
        {
            entity.HasKey(e => e.IdPuesto).HasName("PRIMARY");

            entity.ToTable("puestos");

            entity.Property(e => e.IdPuesto).HasColumnName("id_puesto");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(250)
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'1'")
                .HasColumnName("estado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_modificacion");
            entity.Property(e => e.NivelJerarquico).HasColumnName("nivel_jerarquico");
            entity.Property(e => e.NombrePuesto)
                .HasMaxLength(100)
                .HasColumnName("nombre_puesto");
            entity.Property(e => e.SalarioMaximo)
                .HasPrecision(10, 2)
                .HasColumnName("salario_maximo");
            entity.Property(e => e.SalarioMinimo)
                .HasPrecision(10, 2)
                .HasColumnName("salario_minimo");
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PRIMARY");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Nombre, "nombre").IsUnique();

            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'1'")
                .HasColumnName("estado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_modificacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<SaldoVacaciones>(entity =>
        {
            entity.HasKey(e => e.IdSaldo).HasName("PRIMARY");

            entity.ToTable("saldo_vacaciones");

            entity.HasIndex(e => new { e.EmpleadoId, e.Anio }, "unique_empleado_anio").IsUnique();

            entity.Property(e => e.IdSaldo).HasColumnName("id_saldo");
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.DiasAcumulados).HasColumnName("dias_acumulados");
            entity.Property(e => e.DiasDisfrutados)
                .HasDefaultValueSql("'0'")
                .HasColumnName("dias_disfrutados");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.FechaActualizacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");

            entity.HasOne(d => d.Empleado).WithMany(p => p.SaldoVacaciones)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("saldo_vacaciones_ibfk_1");
        });

        modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PRIMARY");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.EmpleadoId, "empleado_id").IsUnique();

            entity.HasIndex(e => e.Estado, "idx_estado");

            entity.HasIndex(e => e.NombreUsuario, "nombre_usuario").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'ACTIVO'")
                .HasColumnType("enum('ACTIVO','INACTIVO','BLOQUEADO')")
                .HasColumnName("estado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_modificacion");
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(50)
                .HasColumnName("nombre_usuario");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.UltimoAcceso)
                .HasColumnType("datetime")
                .HasColumnName("ultimo_acceso");

            entity.HasOne(d => d.Empleado).WithOne(p => p.Usuarios)
                .HasForeignKey<Usuarios>(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("usuarios_ibfk_1");
        });

        modelBuilder.Entity<UsuariosRoles>(entity =>
        {
            entity.HasKey(e => e.IdUsuarioRol).HasName("PRIMARY");

            entity.ToTable("usuarios_roles");

            entity.HasIndex(e => e.RolId, "rol_id");

            entity.HasIndex(e => new { e.UsuarioId, e.RolId }, "unique_usuario_rol").IsUnique();

            entity.Property(e => e.IdUsuarioRol).HasColumnName("id_usuario_rol");
            entity.Property(e => e.EstadoSolicitud)
                .HasDefaultValueSql("'PENDIENTE'")
                .HasColumnType("enum('PENDIENTE','APROBADA','RECHAZADA')")
                .HasColumnName("estado_solicitud");
            entity.Property(e => e.FechaAsignacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_asignacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_modificacion");
            entity.Property(e => e.RolId).HasColumnName("rol_id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Rol).WithMany(p => p.UsuariosRoles)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("usuarios_roles_ibfk_2");

            entity.HasOne(d => d.Usuario).WithMany(p => p.UsuariosRoles)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("usuarios_roles_ibfk_1");
        });

        modelBuilder.Entity<Vacaciones>(entity =>
        {
            entity.HasKey(e => e.IdVacacion).HasName("PRIMARY");

            entity.ToTable("vacaciones");

            entity.HasIndex(e => new { e.EmpleadoId, e.EstadoSolicitud }, "idx_empleado_estado");

            entity.HasIndex(e => new { e.FechaInicio, e.FechaFin }, "idx_fechas");

            entity.HasIndex(e => e.JefeApruebaId, "jefe_aprueba_id");

            entity.Property(e => e.IdVacacion).HasColumnName("id_vacacion");
            entity.Property(e => e.ComentariosRechazo)
                .HasMaxLength(255)
                .HasColumnName("comentarios_rechazo");
            entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
            entity.Property(e => e.EstadoSolicitud)
                .HasDefaultValueSql("'PENDIENTE'")
                .HasColumnType("enum('PENDIENTE','APROBADA','RECHAZADA','DISFRUTANDO','COMPLETADA')")
                .HasColumnName("estado_solicitud");
            entity.Property(e => e.FechaAprobacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_aprobacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio)
                .HasColumnType("datetime")
                .HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_modificacion");
            entity.Property(e => e.FechaSolicitud)
                .HasColumnType("datetime")
                .HasColumnName("fecha_solicitud");
            entity.Property(e => e.JefeApruebaId).HasColumnName("jefe_aprueba_id");

            entity.HasOne(d => d.Empleado).WithMany(p => p.VacacionesEmpleado)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("vacaciones_ibfk_1");

            entity.HasOne(d => d.JefeAprueba).WithMany(p => p.VacacionesJefeAprueba)
                .HasForeignKey(d => d.JefeApruebaId)
                .HasConstraintName("vacaciones_ibfk_2");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
