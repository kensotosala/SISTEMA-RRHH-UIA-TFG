using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "metricas_rendimiento",
                columns: table => new
                {
                    id_metrica = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre_metrica = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descripcion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    peso = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    estado = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'1'"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_metrica);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "puestos",
                columns: table => new
                {
                    id_puesto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre_puesto = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descripcion = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nivel_jerarquico = table.Column<sbyte>(type: "tinyint", nullable: true),
                    salario_minimo = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    salario_maximo = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    estado = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'1'"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_puesto);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descripcion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    estado = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'1'"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_rol);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "aguinaldos",
                columns: table => new
                {
                    id_aguinaldo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empleado_id = table.Column<int>(type: "int", nullable: false),
                    fecha_calculo = table.Column<DateTime>(type: "datetime", nullable: false),
                    dias_trabajados = table.Column<int>(type: "int", nullable: false),
                    salario_promedio = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    monto_aguinaldo = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    fecha_pago = table.Column<DateTime>(type: "datetime", nullable: true),
                    estado = table.Column<string>(type: "enum('CALCULADO','PAGADO')", nullable: true, defaultValueSql: "'CALCULADO'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_aguinaldo);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "asistencias",
                columns: table => new
                {
                    id_asistencia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empleado_id = table.Column<int>(type: "int", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "datetime", nullable: false),
                    hora_entrada = table.Column<DateTime>(type: "datetime", nullable: true),
                    hora_salida = table.Column<DateTime>(type: "datetime", nullable: true),
                    estado = table.Column<string>(type: "enum('PRESENTE','AUSENTE','TARDANZA','PERMISO')", nullable: true, defaultValueSql: "'PRESENTE'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_asistencia);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "auditoria_cambios",
                columns: table => new
                {
                    id_auditoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    tabla_afectada = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descripcion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_auditoria);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "departamentos",
                columns: table => new
                {
                    id_departamento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre_departamento = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descripcion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    id_jefe_departamento = table.Column<int>(type: "int", nullable: true),
                    estado = table.Column<string>(type: "enum('ACTIVO','INACTIVO')", nullable: true, defaultValueSql: "'ACTIVO'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_departamento);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "empleados",
                columns: table => new
                {
                    id_empleado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    codigo_empleado = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nombre = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    primer_apellido = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    segundo_apellido = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    telefono = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_contratacion = table.Column<DateOnly>(type: "date", nullable: false),
                    puesto_id = table.Column<int>(type: "int", nullable: false),
                    departamento_id = table.Column<int>(type: "int", nullable: false),
                    jefe_inmediato_id = table.Column<int>(type: "int", nullable: true),
                    salario_base = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    tipo_contrato = table.Column<string>(type: "enum('FIJO','TEMPORAL','PRUEBA')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    estado = table.Column<string>(type: "enum('ACTIVO','INACTIVO','LICENCIA')", nullable: true, defaultValueSql: "'ACTIVO'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_empleado);
                    table.ForeignKey(
                        name: "empleados_ibfk_1",
                        column: x => x.puesto_id,
                        principalTable: "puestos",
                        principalColumn: "id_puesto");
                    table.ForeignKey(
                        name: "empleados_ibfk_2",
                        column: x => x.departamento_id,
                        principalTable: "departamentos",
                        principalColumn: "id_departamento");
                    table.ForeignKey(
                        name: "empleados_ibfk_3",
                        column: x => x.jefe_inmediato_id,
                        principalTable: "empleados",
                        principalColumn: "id_empleado");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "evaluaciones_rendimiento",
                columns: table => new
                {
                    id_evaluacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empleado_id = table.Column<int>(type: "int", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "datetime", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "datetime", nullable: false),
                    evaluador_id = table.Column<int>(type: "int", nullable: false),
                    puntuacion_total = table.Column<sbyte>(type: "tinyint", nullable: false),
                    comentarios = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    estado = table.Column<string>(type: "enum('BORRADOR','COMPLETADA')", nullable: true, defaultValueSql: "'BORRADOR'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_evaluacion);
                    table.ForeignKey(
                        name: "evaluaciones_rendimiento_ibfk_1",
                        column: x => x.empleado_id,
                        principalTable: "empleados",
                        principalColumn: "id_empleado");
                    table.ForeignKey(
                        name: "evaluaciones_rendimiento_ibfk_2",
                        column: x => x.evaluador_id,
                        principalTable: "empleados",
                        principalColumn: "id_empleado");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "horas_extras",
                columns: table => new
                {
                    id_hora_extra = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empleado_id = table.Column<int>(type: "int", nullable: false),
                    fecha_solicitud = table.Column<DateTime>(type: "datetime", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "datetime", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "datetime", nullable: false),
                    tipo_hora_extra = table.Column<string>(type: "enum('PENDIENTE','APROBADA','RECHAZADA')", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    motivo = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    estado_solicitud = table.Column<string>(type: "enum('PENDIENTE','APROBADA','RECHAZADA')", nullable: true, defaultValueSql: "'PENDIENTE'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    jefe_aprueba_id = table.Column<int>(type: "int", nullable: true),
                    fecha_aprobacion = table.Column<DateTime>(type: "datetime", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_hora_extra);
                    table.ForeignKey(
                        name: "horas_extras_ibfk_1",
                        column: x => x.empleado_id,
                        principalTable: "empleados",
                        principalColumn: "id_empleado");
                    table.ForeignKey(
                        name: "horas_extras_ibfk_2",
                        column: x => x.jefe_aprueba_id,
                        principalTable: "empleados",
                        principalColumn: "id_empleado");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "incapacidades",
                columns: table => new
                {
                    id_incapacidad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empleado_id = table.Column<int>(type: "int", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "datetime", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "datetime", nullable: false),
                    tipo_incapacidad = table.Column<string>(type: "enum('ENFERMEDAD','ACCIDENTE','MATERNIDAD','PATERNIDAD')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    diagnostico = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    archivo_adjunto = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    estado = table.Column<string>(type: "enum('ACTIVA','FINALIZADA')", nullable: true, defaultValueSql: "'ACTIVA'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_incapacidad);
                    table.ForeignKey(
                        name: "incapacidades_ibfk_1",
                        column: x => x.empleado_id,
                        principalTable: "empleados",
                        principalColumn: "id_empleado");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "liquidaciones",
                columns: table => new
                {
                    id_liquidacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empleado_id = table.Column<int>(type: "int", nullable: false),
                    fecha_liquidacion = table.Column<DateTime>(type: "datetime", nullable: false),
                    motivo_liquidacion = table.Column<string>(type: "enum('RENUNCIA','DESPIDO','DESPIDO_JUSTIFICADO','JUBILACION')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    salario_base = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    vacaciones_pendientes = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    aguinaldo_proporcional = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    indemnizacion = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    otros_conceptos = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    total_liquidacion = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    estado = table.Column<string>(type: "enum('CALCULADA','PAGADA')", nullable: true, defaultValueSql: "'CALCULADA'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_liquidacion);
                    table.ForeignKey(
                        name: "liquidaciones_ibfk_1",
                        column: x => x.empleado_id,
                        principalTable: "empleados",
                        principalColumn: "id_empleado");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "nominas",
                columns: table => new
                {
                    id_nomina = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empleado_id = table.Column<int>(type: "int", nullable: false),
                    periodo_nomina = table.Column<DateTime>(type: "datetime", nullable: false),
                    fecha_pago = table.Column<DateTime>(type: "datetime", nullable: false),
                    salario_base = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    horas_extras = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    monto_horas_extra = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    bonificaciones = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    deducciones = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    total_bruto = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    total_neto = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    estado = table.Column<string>(type: "enum('PENDIENTE','PAGADA','ANULADA')", nullable: true, defaultValueSql: "'PENDIENTE'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_actualizacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_nomina);
                    table.ForeignKey(
                        name: "nominas_ibfk_1",
                        column: x => x.empleado_id,
                        principalTable: "empleados",
                        principalColumn: "id_empleado");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "permisos",
                columns: table => new
                {
                    id_permiso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empleado_id = table.Column<int>(type: "int", nullable: false),
                    fecha_solicitud = table.Column<DateTime>(type: "datetime", nullable: false),
                    fecha_permiso = table.Column<DateTime>(type: "datetime", nullable: false),
                    motivo = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    con_goce_salario = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'0'"),
                    estado_solicitud = table.Column<string>(type: "enum('PENDIENTE','APROBADA','RECHAZADA')", nullable: true, defaultValueSql: "'PENDIENTE'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    jefe_aprueba_id = table.Column<int>(type: "int", nullable: true),
                    fecha_aprobacion = table.Column<DateTime>(type: "datetime", nullable: true),
                    comentarios_rechazo = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_permiso);
                    table.ForeignKey(
                        name: "permisos_ibfk_1",
                        column: x => x.empleado_id,
                        principalTable: "empleados",
                        principalColumn: "id_empleado");
                    table.ForeignKey(
                        name: "permisos_ibfk_2",
                        column: x => x.jefe_aprueba_id,
                        principalTable: "empleados",
                        principalColumn: "id_empleado");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "saldo_vacaciones",
                columns: table => new
                {
                    id_saldo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empleado_id = table.Column<int>(type: "int", nullable: false),
                    anio = table.Column<int>(type: "int", nullable: false),
                    dias_acumulados = table.Column<int>(type: "int", nullable: false),
                    dias_disfrutados = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_actualizacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_saldo);
                    table.ForeignKey(
                        name: "saldo_vacaciones_ibfk_1",
                        column: x => x.empleado_id,
                        principalTable: "empleados",
                        principalColumn: "id_empleado");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empleado_id = table.Column<int>(type: "int", nullable: false),
                    nombre_usuario = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ultimo_acceso = table.Column<DateTime>(type: "datetime", nullable: true),
                    estado = table.Column<string>(type: "enum('ACTIVO','INACTIVO','BLOQUEADO')", nullable: true, defaultValueSql: "'ACTIVO'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_usuario);
                    table.ForeignKey(
                        name: "usuarios_ibfk_1",
                        column: x => x.empleado_id,
                        principalTable: "empleados",
                        principalColumn: "id_empleado");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "vacaciones",
                columns: table => new
                {
                    id_vacacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    empleado_id = table.Column<int>(type: "int", nullable: false),
                    fecha_solicitud = table.Column<DateTime>(type: "datetime", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "datetime", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "datetime", nullable: false),
                    estado_solicitud = table.Column<string>(type: "enum('PENDIENTE','APROBADA','RECHAZADA','DISFRUTANDO','COMPLETADA', 'CANCELADA')", nullable: true, defaultValueSql: "'PENDIENTE'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    jefe_aprueba_id = table.Column<int>(type: "int", nullable: true),
                    fecha_aprobacion = table.Column<DateTime>(type: "datetime", nullable: true),
                    comentarios_rechazo = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_vacacion);
                    table.ForeignKey(
                        name: "vacaciones_ibfk_1",
                        column: x => x.empleado_id,
                        principalTable: "empleados",
                        principalColumn: "id_empleado");
                    table.ForeignKey(
                        name: "vacaciones_ibfk_2",
                        column: x => x.jefe_aprueba_id,
                        principalTable: "empleados",
                        principalColumn: "id_empleado");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "detalle_evaluaciones",
                columns: table => new
                {
                    id_detalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_evaluacion = table.Column<int>(type: "int", nullable: false),
                    id_metrica = table.Column<int>(type: "int", nullable: false),
                    puntuacion = table.Column<sbyte>(type: "tinyint", nullable: false),
                    comentarios = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_detalle);
                    table.ForeignKey(
                        name: "detalle_evaluaciones_ibfk_1",
                        column: x => x.id_evaluacion,
                        principalTable: "evaluaciones_rendimiento",
                        principalColumn: "id_evaluacion");
                    table.ForeignKey(
                        name: "detalle_evaluaciones_ibfk_2",
                        column: x => x.id_metrica,
                        principalTable: "metricas_rendimiento",
                        principalColumn: "id_metrica");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "usuarios_roles",
                columns: table => new
                {
                    id_usuario_rol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    estado_solicitud = table.Column<string>(type: "enum('PENDIENTE','APROBADA','RECHAZADA')", nullable: true, defaultValueSql: "'PENDIENTE'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    rol_id = table.Column<int>(type: "int", nullable: false),
                    fecha_asignacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id_usuario_rol);
                    table.ForeignKey(
                        name: "usuarios_roles_ibfk_1",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario");
                    table.ForeignKey(
                        name: "usuarios_roles_ibfk_2",
                        column: x => x.rol_id,
                        principalTable: "roles",
                        principalColumn: "id_rol");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateIndex(
                name: "idx_empleado_estado",
                table: "aguinaldos",
                columns: new[] { "empleado_id", "estado" });

            migrationBuilder.CreateIndex(
                name: "idx_fecha_pago",
                table: "aguinaldos",
                column: "fecha_pago");

            migrationBuilder.CreateIndex(
                name: "idx_fecha_estado",
                table: "asistencias",
                columns: new[] { "fecha_registro", "estado" });

            migrationBuilder.CreateIndex(
                name: "unique_empleado_fecha",
                table: "asistencias",
                columns: new[] { "empleado_id", "fecha_registro" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_tabla_fecha",
                table: "auditoria_cambios",
                columns: new[] { "tabla_afectada", "fecha_creacion" });

            migrationBuilder.CreateIndex(
                name: "usuario_id",
                table: "auditoria_cambios",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "id_jefe_departamento",
                table: "departamentos",
                column: "id_jefe_departamento");

            migrationBuilder.CreateIndex(
                name: "id_metrica",
                table: "detalle_evaluaciones",
                column: "id_metrica");

            migrationBuilder.CreateIndex(
                name: "unique_evaluacion_metrica",
                table: "detalle_evaluaciones",
                columns: new[] { "id_evaluacion", "id_metrica" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "codigo_empleado",
                table: "empleados",
                column: "codigo_empleado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "email",
                table: "empleados",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_departamento",
                table: "empleados",
                column: "departamento_id");

            migrationBuilder.CreateIndex(
                name: "idx_puesto",
                table: "empleados",
                column: "puesto_id");

            migrationBuilder.CreateIndex(
                name: "jefe_inmediato_id",
                table: "empleados",
                column: "jefe_inmediato_id");

            migrationBuilder.CreateIndex(
                name: "evaluador_id",
                table: "evaluaciones_rendimiento",
                column: "evaluador_id");

            migrationBuilder.CreateIndex(
                name: "idx_empleado_periodo",
                table: "evaluaciones_rendimiento",
                columns: new[] { "empleado_id", "fecha_inicio", "fecha_fin" });

            migrationBuilder.CreateIndex(
                name: "idx_empleado_fecha",
                table: "horas_extras",
                columns: new[] { "empleado_id", "fecha_inicio" });

            migrationBuilder.CreateIndex(
                name: "jefe_aprueba_id",
                table: "horas_extras",
                column: "jefe_aprueba_id");

            migrationBuilder.CreateIndex(
                name: "idx_empleado_fechas",
                table: "incapacidades",
                columns: new[] { "empleado_id", "fecha_inicio", "fecha_fin" });

            migrationBuilder.CreateIndex(
                name: "idx_empleado_fecha1",
                table: "liquidaciones",
                columns: new[] { "empleado_id", "fecha_liquidacion" });

            migrationBuilder.CreateIndex(
                name: "idx_estado",
                table: "metricas_rendimiento",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "idx_empleado_periodo1",
                table: "nominas",
                columns: new[] { "empleado_id", "periodo_nomina" });

            migrationBuilder.CreateIndex(
                name: "idx_empleado_fecha2",
                table: "permisos",
                columns: new[] { "empleado_id", "fecha_permiso" });

            migrationBuilder.CreateIndex(
                name: "jefe_aprueba_id1",
                table: "permisos",
                column: "jefe_aprueba_id");

            migrationBuilder.CreateIndex(
                name: "nombre",
                table: "roles",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "unique_empleado_anio",
                table: "saldo_vacaciones",
                columns: new[] { "empleado_id", "anio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "empleado_id",
                table: "usuarios",
                column: "empleado_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_estado1",
                table: "usuarios",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "nombre_usuario",
                table: "usuarios",
                column: "nombre_usuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "rol_id",
                table: "usuarios_roles",
                column: "rol_id");

            migrationBuilder.CreateIndex(
                name: "unique_usuario_rol",
                table: "usuarios_roles",
                columns: new[] { "usuario_id", "rol_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_empleado_estado1",
                table: "vacaciones",
                columns: new[] { "empleado_id", "estado_solicitud" });

            migrationBuilder.CreateIndex(
                name: "idx_fechas",
                table: "vacaciones",
                columns: new[] { "fecha_inicio", "fecha_fin" });

            migrationBuilder.CreateIndex(
                name: "jefe_aprueba_id2",
                table: "vacaciones",
                column: "jefe_aprueba_id");

            migrationBuilder.AddForeignKey(
                name: "aguinaldos_ibfk_1",
                table: "aguinaldos",
                column: "empleado_id",
                principalTable: "empleados",
                principalColumn: "id_empleado");

            migrationBuilder.AddForeignKey(
                name: "asistencias_ibfk_1",
                table: "asistencias",
                column: "empleado_id",
                principalTable: "empleados",
                principalColumn: "id_empleado");

            migrationBuilder.AddForeignKey(
                name: "auditoria_cambios_ibfk_1",
                table: "auditoria_cambios",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id_usuario");

            migrationBuilder.AddForeignKey(
                name: "departamentos_ibfk_1",
                table: "departamentos",
                column: "id_jefe_departamento",
                principalTable: "empleados",
                principalColumn: "id_empleado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "departamentos_ibfk_1",
                table: "departamentos");

            migrationBuilder.DropTable(
                name: "aguinaldos");

            migrationBuilder.DropTable(
                name: "asistencias");

            migrationBuilder.DropTable(
                name: "auditoria_cambios");

            migrationBuilder.DropTable(
                name: "detalle_evaluaciones");

            migrationBuilder.DropTable(
                name: "horas_extras");

            migrationBuilder.DropTable(
                name: "incapacidades");

            migrationBuilder.DropTable(
                name: "liquidaciones");

            migrationBuilder.DropTable(
                name: "nominas");

            migrationBuilder.DropTable(
                name: "permisos");

            migrationBuilder.DropTable(
                name: "saldo_vacaciones");

            migrationBuilder.DropTable(
                name: "usuarios_roles");

            migrationBuilder.DropTable(
                name: "vacaciones");

            migrationBuilder.DropTable(
                name: "evaluaciones_rendimiento");

            migrationBuilder.DropTable(
                name: "metricas_rendimiento");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "empleados");

            migrationBuilder.DropTable(
                name: "puestos");

            migrationBuilder.DropTable(
                name: "departamentos");
        }
    }
}
