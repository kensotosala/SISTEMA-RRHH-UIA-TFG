using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.DTOs
{
    /// <summary>
    /// Período legal del aguinaldo en Costa Rica: 1 dic año anterior → 30 nov año actual
    /// Fórmula: MontoAguinaldo = (SalarioPromedio × DiasTrabajados) / 365
    /// </summary>
    public class AguinaldoDTO
    {
        public int IdAguinaldo { get; set; }
        public int EmpleadoId { get; set; }
        public string? CodigoEmpleado { get; set; }
        public string? NombreEmpleado { get; set; }
        public string? Departamento { get; set; }
        public string? Puesto { get; set; }

        public DateTime FechaCalculo { get; set; }

        /// <summary>
        /// Días trabajados dentro del período legal (0–365)
        /// </summary>
        public int DiasTrabajados { get; set; }

        /// <summary>
        /// Promedio de salarios ordinarios del período (Art. 229 Código de Trabajo CR)
        /// </summary>
        public decimal SalarioPromedio { get; set; }

        /// <summary>
        /// Monto calculado: (SalarioPromedio × DiasTrabajados) / 365
        /// </summary>
        public decimal MontoAguinaldo { get; set; }

        public DateTime? FechaPago { get; set; }
        public string? Estado { get; set; }

        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }

    /// <summary>
    /// DTO para calcular aguinaldo de un empleado específico.
    /// El período siempre es: 1 dic (Anio-1) → 30 nov (Anio), 
    /// o desde FechaContratacion si el empleado ingresó después del 1 dic.
    /// </summary>
    public class CalcularAguinaldoDTO
    {
        [Required(ErrorMessage = "El ID del empleado es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del empleado debe ser mayor a cero")]
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "El año es obligatorio")]
        [Range(2000, 2100, ErrorMessage = "El año debe estar entre 2000 y 2100")]
        public int Anio { get; set; }

        /// <summary>
        /// Fecha de corte opcional. Si es null se usa el 30 de noviembre del año indicado.
        /// No puede ser posterior al 30 de noviembre del año indicado.
        /// </summary>
        public DateTime? FechaCorte { get; set; }
    }

    /// <summary>
    /// DTO para cálculo masivo de aguinaldos.
    /// </summary>
    public class CalcularAguinaldoMasivoDTO
    {
        [Required(ErrorMessage = "El año es obligatorio")]
        [Range(2000, 2100, ErrorMessage = "El año debe estar entre 2000 y 2100")]
        public int Anio { get; set; }

        /// <summary>
        /// Fecha de corte. Si es null se usa el 30 de noviembre del año indicado.
        /// </summary>
        public DateTime? FechaCorte { get; set; }
    }

    /// <summary>
    /// DTO para registrar el pago de un aguinaldo.
    /// El IdAguinaldo se toma de la URL — NO se incluye aquí para evitar inconsistencias.
    /// </summary>
    public class PagarAguinaldoDTO
    {
        // Incluirlo aquí creaba ambigüedad y el controlador lo ignoraba.

        [Required(ErrorMessage = "La fecha de pago es obligatoria")]
        public DateTime FechaPago { get; set; }
    }

    /// <summary>
    /// Resumen de aguinaldos para un año. TotalGeneral es calculado automáticamente.
    /// </summary>
    public class ResumenAguinaldoDTO
    {
        public int TotalEmpleados { get; set; }
        public int AguinaldosPendientes { get; set; }
        public int AguinaldosPagados { get; set; }
        public decimal TotalPendiente { get; set; }
        public decimal TotalPagado { get; set; }

        /// <summary>
        /// </summary>
        public decimal TotalGeneral => TotalPendiente + TotalPagado;

        public List<AguinaldoDTO> Aguinaldos { get; set; } = new();
    }

    /// <summary>
    /// Resultado del cálculo de aguinaldo para un empleado.
    /// Período legal: FechaInicio debe ser >= 1 dic año anterior,
    ///                FechaFin   debe ser <= 30 nov año actual.
    /// </summary>
    public class ResultadoCalculoAguinaldoDTO
    {
        [Range(1, int.MaxValue)]
        public int EmpleadoId { get; set; }

        public string NombreEmpleado { get; set; } = string.Empty;

        public DateTime FechaContratacion { get; set; }

        /// <summary>
        /// Inicio real del período de cálculo (puede ser FechaContratacion si es posterior al 1 dic)
        /// </summary>
        public DateTime FechaInicio { get; set; }

        /// <summary>
        /// Fin del período (máximo 30 de noviembre del año calculado)
        /// </summary>
        public DateTime FechaFin { get; set; }

        /// <summary>
        /// manualmente con un valor inconsistente respecto a FechaInicio/FechaFin.
        /// Si se necesita override (ej. días reales trabajados con ausencias),
        /// usar DiasTrabajadosEfectivos.
        /// </summary>
        public int DiasTrabajados => FechaFin > FechaInicio
            ? (int)(FechaFin - FechaInicio).TotalDays + 1
            : 0;

        /// <summary>
        /// Días efectivamente trabajados (descuenta ausencias no remuneradas).
        /// Si es null se usa DiasTrabajados calculado de fechas.
        /// La BLL debe priorizar este valor cuando esté presente.
        /// </summary>
        public int? DiasTrabajadosEfectivos { get; set; }

        /// <summary>
        /// Días finales usados para el cálculo (efectivos si existen, sino calculados de fechas)
        /// </summary>
        public int DiasParaCalculo => DiasTrabajadosEfectivos ?? DiasTrabajados;

        [Range(0.01, double.MaxValue, ErrorMessage = "El salario promedio debe ser mayor a cero")]
        public decimal SalarioPromedio { get; set; }

        /// <summary>
        /// Redondeada a 2 decimales para evitar acumulación de error de punto flotante.
        /// </summary>
        public decimal MontoAguinaldo =>
            Math.Round((SalarioPromedio * DiasParaCalculo) / 365m, 2, MidpointRounding.AwayFromZero);

        public string Detalle { get; set; } = string.Empty;
    }
}