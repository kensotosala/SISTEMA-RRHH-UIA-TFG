CREATE DATABASE IF NOT EXISTS SISTEMA_RH;

USE SISTEMA_RH;

CREATE TABLE puestos (
    id_puesto INT PRIMARY KEY AUTO_INCREMENT,
    nombre_puesto VARCHAR(100) NOT NULL,
    descripcion VARCHAR(250),
    nivel_jerarquico TINYINT,
    salario_minimo DECIMAL(10,2),
    salario_maximo DECIMAL(10,2),
    estado BOOLEAN DEFAULT TRUE,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE departamentos (
    id_departamento INT PRIMARY KEY AUTO_INCREMENT,
    nombre_departamento VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255),
    id_jefe_departamento INT,
    estado ENUM('ACTIVO', 'INACTIVO') DEFAULT 'ACTIVO',
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
    -- La FK a empleados se agregará después
);

-- Tabla Principal para Empleados
CREATE TABLE empleados (
    id_empleado INT PRIMARY KEY AUTO_INCREMENT,
    codigo_empleado VARCHAR(20) UNIQUE NOT NULL,
    nombre VARCHAR(20) NOT NULL,
    primer_apellido VARCHAR(20) NOT NULL,
    segundo_apellido VARCHAR(20),
    email VARCHAR(150) UNIQUE NOT NULL,
    telefono VARCHAR(20),
    fecha_contratacion DATE NOT NULL,
    puesto_id INT NOT NULL,
    departamento_id INT NOT NULL,
    jefe_inmediato_id INT,
    salario_base DECIMAL(10,2) NOT NULL,
    tipo_contrato ENUM('FIJO', 'TEMPORAL', 'PRUEBA') NOT NULL,
    estado ENUM('ACTIVO', 'INACTIVO', 'LICENCIA') DEFAULT 'ACTIVO',
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (puesto_id) REFERENCES puestos(id_puesto),
    FOREIGN KEY (departamento_id) REFERENCES departamentos(id_departamento),
    FOREIGN KEY (jefe_inmediato_id) REFERENCES empleados(id_empleado),
    INDEX idx_departamento (departamento_id),
    INDEX idx_puesto (puesto_id)
);

ALTER TABLE departamentos ADD FOREIGN KEY (id_jefe_departamento) REFERENCES empleados(id_empleado);

-- Módulo de Nóminas (CORREGIDO según diccionario)
CREATE TABLE nominas (
    id_nomina INT PRIMARY KEY AUTO_INCREMENT,
    empleado_id INT NOT NULL,
    periodo_nomina DATETIME NOT NULL,
    fecha_pago DATETIME NOT NULL,
    salario_base DECIMAL(10,2) NOT NULL,
    horas_extras DECIMAL(10,2) DEFAULT 0,
    monto_horas_extra DECIMAL(10,2) DEFAULT 0,
    bonificaciones DECIMAL(10,2) DEFAULT 0,
    deducciones DECIMAL(10,2) DEFAULT 0,
    total_bruto DECIMAL(10,2) NOT NULL,
    total_neto DECIMAL(10,2) NOT NULL,
    estado ENUM('PENDIENTE','PAGADA','ANULADA') DEFAULT 'PENDIENTE',
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (empleado_id) REFERENCES empleados(id_empleado),
    INDEX idx_empleado_periodo (empleado_id, periodo_nomina)
);

-- Módulo de Horas Extras
CREATE TABLE horas_extras (
    id_hora_extra INT PRIMARY KEY AUTO_INCREMENT,
    empleado_id INT NOT NULL,
    fecha_solicitud DATETIME NOT NULL,
    fecha_inicio DATETIME NOT NULL,
    fecha_fin DATETIME NOT NULL,
    tipo_hora_extra ENUM('PENDIENTE', 'APROBADA', 'RECHAZADA'),
    motivo VARCHAR(255) NOT NULL,
    estado_solicitud ENUM('PENDIENTE', 'APROBADA', 'RECHAZADA') DEFAULT 'PENDIENTE',
    jefe_aprueba_id INT,
    fecha_aprobacion DATETIME,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (empleado_id) REFERENCES empleados(id_empleado),
    FOREIGN KEY (jefe_aprueba_id) REFERENCES empleados(id_empleado),
    INDEX idx_empleado_fecha (empleado_id, fecha_inicio)
);

-- Módulo de Vacaciones
CREATE TABLE vacaciones (
    id_vacacion INT PRIMARY KEY AUTO_INCREMENT,
    empleado_id INT NOT NULL,
    fecha_solicitud DATETIME NOT NULL,
    fecha_inicio DATETIME NOT NULL,
    fecha_fin DATETIME NOT NULL,
    estado_solicitud ENUM('PENDIENTE','APROBADA','RECHAZADA','DISFRUTANDO','COMPLETADA') DEFAULT 'PENDIENTE',
    jefe_aprueba_id INT,
    fecha_aprobacion DATETIME,
    comentarios_rechazo VARCHAR(255),
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (empleado_id) REFERENCES empleados(id_empleado),
    FOREIGN KEY (jefe_aprueba_id) REFERENCES empleados(id_empleado),
    INDEX idx_empleado_estado (empleado_id, estado_solicitud),
    INDEX idx_fechas (fecha_inicio, fecha_fin)
);

CREATE TABLE saldo_vacaciones (
    id_saldo INT PRIMARY KEY AUTO_INCREMENT,
    empleado_id INT NOT NULL,
    anio INT NOT NULL,
    dias_acumulados INT NOT NULL,
    dias_disfrutados INT DEFAULT 0,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY unique_empleado_anio (empleado_id, anio),
    FOREIGN KEY (empleado_id) REFERENCES empleados(id_empleado)
);

-- Módulo de Aguinaldo
CREATE TABLE aguinaldos (
    id_aguinaldo INT PRIMARY KEY AUTO_INCREMENT,
    empleado_id INT NOT NULL,
    fecha_calculo DATETIME NOT NULL,
    dias_trabajados INT NOT NULL,
    salario_promedio DECIMAL(10,2) NOT NULL,
    monto_aguinaldo DECIMAL(10,2) NOT NULL,
    fecha_pago DATETIME,              -- Cambiado de DATE a DATETIME
    estado ENUM('CALCULADO', 'PAGADO') DEFAULT 'CALCULADO',
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (empleado_id) REFERENCES empleados(id_empleado),
    INDEX idx_empleado_estado (empleado_id, estado),
    INDEX idx_fecha_pago (fecha_pago)
);

-- Módulo de Incapacidades
CREATE TABLE incapacidades (
    id_incapacidad INT PRIMARY KEY AUTO_INCREMENT,
    empleado_id INT NOT NULL,
    fecha_inicio DATETIME NOT NULL,
    fecha_fin DATETIME NOT NULL,
    tipo_incapacidad ENUM('ENFERMEDAD', 'ACCIDENTE', 'MATERNIDAD', 'PATERNIDAD') NOT NULL,
    diagnostico VARCHAR(255),
    archivo_adjunto VARCHAR(1000),
    estado ENUM('ACTIVA', 'FINALIZADA') DEFAULT 'ACTIVA',
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (empleado_id) REFERENCES empleados(id_empleado),
    INDEX idx_empleado_fechas (empleado_id, fecha_inicio, fecha_fin)
);

-- Módulo de Permisos
CREATE TABLE permisos (
    id_permiso INT PRIMARY KEY AUTO_INCREMENT,
    empleado_id INT NOT NULL,
    fecha_solicitud DATETIME NOT NULL,
    fecha_permiso DATETIME NOT NULL,
    motivo VARCHAR(255) NOT NULL,
    con_goce_salario BOOLEAN DEFAULT FALSE,
    estado_solicitud ENUM('PENDIENTE', 'APROBADA', 'RECHAZADA') DEFAULT 'PENDIENTE',
    jefe_aprueba_id INT,
    fecha_aprobacion DATETIME,
    comentarios_rechazo VARCHAR(255),
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (empleado_id) REFERENCES empleados(id_empleado),
    FOREIGN KEY (jefe_aprueba_id) REFERENCES empleados(id_empleado),
    INDEX idx_empleado_fecha (empleado_id, fecha_permiso)
);

-- Módulo de Asistencia
CREATE TABLE asistencias (
    id_asistencia INT PRIMARY KEY AUTO_INCREMENT,
    empleado_id INT NOT NULL,
    fecha_registro DATETIME NOT NULL,
    hora_entrada DATETIME,
    hora_salida DATETIME,
    estado ENUM('PRESENTE', 'AUSENTE', 'TARDANZA', 'PERMISO') DEFAULT 'PRESENTE',
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (empleado_id) REFERENCES empleados(id_empleado),
    UNIQUE KEY unique_empleado_fecha (empleado_id, fecha_registro),
    INDEX idx_fecha_estado (fecha_registro, estado)
);

-- Módulo de Liquidación
CREATE TABLE liquidaciones (
    id_liquidacion INT PRIMARY KEY AUTO_INCREMENT,
    empleado_id INT NOT NULL,
    fecha_liquidacion DATETIME NOT NULL,  -- Cambiado de DATE a DATETIME
    motivo_liquidacion ENUM('RENUNCIA', 'DESPIDO', 'DESPIDO_JUSTIFICADO', 'JUBILACION') NOT NULL,
    salario_base DECIMAL(10,2) NOT NULL,
    vacaciones_pendientes DECIMAL(10,2) DEFAULT 0,
    aguinaldo_proporcional DECIMAL(10,2) DEFAULT 0,
    indemnizacion DECIMAL(10,2) DEFAULT 0,
    otros_conceptos DECIMAL(10,2) DEFAULT 0,
    total_liquidacion DECIMAL(10,2) NOT NULL,
    estado ENUM('CALCULADA', 'PAGADA') DEFAULT 'CALCULADA',
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (empleado_id) REFERENCES empleados(id_empleado),
    INDEX idx_empleado_fecha (empleado_id, fecha_liquidacion)
);

-- Módulo de Evaluación de Rendimiento
CREATE TABLE metricas_rendimiento (
    id_metrica INT PRIMARY KEY AUTO_INCREMENT,
    nombre_metrica VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255),
    peso DECIMAL(3,2) NOT NULL,
    estado BOOLEAN DEFAULT TRUE,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_estado (estado)
);

CREATE TABLE evaluaciones_rendimiento (
    id_evaluacion INT PRIMARY KEY AUTO_INCREMENT,
    empleado_id INT NOT NULL,
    fecha_inicio DATETIME NOT NULL,
    fecha_fin DATETIME NOT NULL,
    evaluador_id INT NOT NULL,
    puntuacion_total TINYINT NOT NULL,
    comentarios VARCHAR(255),
    estado ENUM('BORRADOR', 'COMPLETADA') DEFAULT 'BORRADOR',
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (empleado_id) REFERENCES empleados(id_empleado),
    FOREIGN KEY (evaluador_id) REFERENCES empleados(id_empleado),
    INDEX idx_empleado_periodo (empleado_id, fecha_inicio, fecha_fin)
);

CREATE TABLE detalle_evaluaciones (
    id_detalle INT PRIMARY KEY AUTO_INCREMENT,
    id_evaluacion INT NOT NULL,
    id_metrica INT NOT NULL,
    puntuacion TINYINT NOT NULL,
    comentarios VARCHAR(255),
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (id_evaluacion) REFERENCES evaluaciones_rendimiento(id_evaluacion),
    FOREIGN KEY (id_metrica) REFERENCES metricas_rendimiento(id_metrica),
    UNIQUE KEY unique_evaluacion_metrica (id_evaluacion, id_metrica)
);

-- Módulo de Seguridad
CREATE TABLE usuarios (
    id_usuario INT PRIMARY KEY AUTO_INCREMENT,
    empleado_id INT UNIQUE NOT NULL,
    nombre_usuario VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    ultimo_acceso DATETIME,
    estado ENUM('ACTIVO','INACTIVO','BLOQUEADO') DEFAULT 'ACTIVO',
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (empleado_id) REFERENCES empleados(id_empleado),
    INDEX idx_estado (estado)
);

CREATE TABLE roles (
    id_rol INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(50) UNIQUE NOT NULL,
    descripcion VARCHAR(255),
    estado BOOLEAN DEFAULT TRUE,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE usuarios_roles (
    id_usuario_rol INT PRIMARY KEY AUTO_INCREMENT,
    estado_solicitud ENUM('PENDIENTE', 'APROBADA', 'RECHAZADA') DEFAULT 'PENDIENTE',
    usuario_id INT NOT NULL,
    rol_id INT NOT NULL,
    fecha_asignacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id_usuario),
    FOREIGN KEY (rol_id) REFERENCES roles(id_rol),
    UNIQUE KEY unique_usuario_rol (usuario_id, rol_id)
);

-- Tablas de auditoría
CREATE TABLE auditoria_cambios (
    id_auditoria INT PRIMARY KEY AUTO_INCREMENT,
    tabla_afectada VARCHAR(50) NOT NULL,
    descripcion VARCHAR(255) NOT NULL,
    usuario_id INT NOT NULL,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id_usuario),
    INDEX idx_tabla_fecha (tabla_afectada, fecha_creacion)
);