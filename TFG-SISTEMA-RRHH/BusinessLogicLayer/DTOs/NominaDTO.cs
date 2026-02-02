namespace BusinessLogicLayer.DTOs
{
    // ============================================
    // DTOs PRINCIPALES DE NÓMINA
    // ============================================

    public class NominaDTO
    {
        public int IdNomina { get; set; }
        public int EmpleadoId { get; set; }
        public DateTime PeriodoNomina { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal SalarioBase { get; set; }
        public decimal? HorasExtras { get; set; }
        public decimal? MontoHorasExtra { get; set; }
        public decimal? Bonificaciones { get; set; }
        public decimal? Deducciones { get; set; }
        public decimal TotalBruto { get; set; }
        public decimal TotalNeto { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }

        // Información del empleado
        public string? NombreEmpleado { get; set; }

        public string? CodigoEmpleado { get; set; }
        public string? Puesto { get; set; }
        public string? Departamento { get; set; }
    }

    public class GenerarNominaQuincenalDTO
    {
        public int Quincena { get; set; } // 1 o 2
        public int Mes { get; set; }      // 1-12
        public int Anio { get; set; }     // 2026
        public DateTime FechaPago { get; set; }
        public List<int>? EmpleadosIds { get; set; } // null = todos los empleados activos
    }

    public class DetalleNominaDTO
    {
        public int EmpleadoId { get; set; }
        public string CodigoEmpleado { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Puesto { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;

        // INGRESOS
        public decimal SalarioBaseQuincenal { get; set; }

        public decimal HorasExtraDiurnas { get; set; }
        public decimal HorasExtraNocturnas { get; set; }
        public decimal HorasExtraFeriados { get; set; }
        public decimal TotalHorasExtra { get; set; }
        public decimal Bonificaciones { get; set; }
        public decimal TotalBruto { get; set; }

        // DEDUCCIONES CCSS
        public DeduccionesCCSSDTO DeduccionesCCSS { get; set; } = new();

        public decimal TotalCCSS { get; set; }

        // IMPUESTO SOBRE LA RENTA
        public ImpuestoRentaDTO ImpuestoRenta { get; set; } = new();

        // OTRAS DEDUCCIONES
        public decimal PensionAlimenticia { get; set; }

        public decimal Prestamos { get; set; }
        public decimal Embargos { get; set; }
        public decimal OtrasDeducciones { get; set; }
        public decimal TotalDeducciones { get; set; }

        // AJUSTES POR AUSENCIAS
        public AjustesAusenciasDTO AjustesAusencias { get; set; } = new();

        // TOTAL
        public decimal TotalNeto { get; set; }
    }

    public class DeduccionesCCSSDTO
    {
        public decimal SEM { get; set; }             // 10.50%
        public decimal IVM { get; set; }             // 4.17%
        public decimal BancoPopular { get; set; }    // 1.00%
        public decimal ANP { get; set; }             // 1.00%
        public decimal Total { get; set; }           // 16.67%
    }

    public class ImpuestoRentaDTO
    {
        public decimal BaseImponible { get; set; }
        public decimal ProyeccionMensual { get; set; }
        public decimal ImpuestoMensual { get; set; }
        public decimal ImpuestoQuincenal { get; set; }
        public string TramoAplicado { get; set; } = string.Empty;
    }

    public class AjustesAusenciasDTO
    {
        public int DiasIncapacidad { get; set; }
        public decimal MontoIncapacidad { get; set; }
        public int DiasPermisoSinGoce { get; set; }
        public decimal MontoPermisoSinGoce { get; set; }
        public int DiasVacaciones { get; set; }
        public decimal MontoVacaciones { get; set; }
        public decimal TotalAjustes { get; set; }
    }

    // ============================================
    // DTOs PARA HORAS EXTRA
    // ============================================

    public class HorasExtraCalculoDTO
    {
        public int EmpleadoId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public TipoHoraExtra Tipo { get; set; }
        public decimal HorasTrabajadas { get; set; }
        public decimal SalarioPorHora { get; set; }
        public decimal Recargo { get; set; } // 1.5 o 2.0
        public decimal MontoTotal { get; set; }
    }

    public enum TipoHoraExtra
    {
        DIURNA,      // 1.5x
        NOCTURNA,    // 1.5x
        FERIADO,     // 2.0x
        DOMINGO      // 2.0x
    }

    // ============================================
    // DTOs PARA REPORTES
    // ============================================

    public class ResumenNominaQuincenalDTO
    {
        public int Quincena { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public int TotalEmpleados { get; set; }
        public decimal TotalBruto { get; set; }
        public decimal TotalCCSS { get; set; }
        public decimal TotalImpuestoRenta { get; set; }
        public decimal TotalDeducciones { get; set; }
        public decimal TotalNeto { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class PlanillaCCSSDTO
    {
        public int Mes { get; set; }
        public int Anio { get; set; }
        public List<DetalleCCSSEmpleadoDTO> Empleados { get; set; } = new();
        public decimal TotalSalariosReportados { get; set; }
        public decimal TotalCuotaObrera { get; set; }
        public decimal TotalCuotaPatronal { get; set; }
    }

    public class DetalleCCSSEmpleadoDTO
    {
        public string Cedula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public decimal SalarioReportado { get; set; }
        public decimal CuotaObrera { get; set; }    // 16.67%
        public decimal CuotaPatronal { get; set; }  // 26.33% (SEM + IVM + BP + RT + FCL + ANP)
    }

    public class DeclaracionD151DTO
    {
        public int Mes { get; set; }
        public int Anio { get; set; }
        public List<DetalleImpuestoEmpleadoDTO> Empleados { get; set; } = new();
        public decimal TotalRetenido { get; set; }
    }

    public class DetalleImpuestoEmpleadoDTO
    {
        public string Cedula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public decimal SalarioBruto { get; set; }
        public decimal DeduccionCCSS { get; set; }
        public decimal BaseImponible { get; set; }
        public decimal ImpuestoRetenido { get; set; }
    }

    // ============================================
    // DTOs PARA AGUINALDO
    // ============================================

    public class CalculoAguinaldoDTO
    {
        public int EmpleadoId { get; set; }
        public int Anio { get; set; }
        public DateTime FechaInicio { get; set; }  // 1 dic año anterior
        public DateTime FechaFin { get; set; }      // 30 nov año actual
        public decimal TotalSalariosAnuales { get; set; }
        public decimal MontoAguinaldo { get; set; } // Total / 12
        public bool Pagado { get; set; }
        public DateTime? FechaPago { get; set; }
    }
}