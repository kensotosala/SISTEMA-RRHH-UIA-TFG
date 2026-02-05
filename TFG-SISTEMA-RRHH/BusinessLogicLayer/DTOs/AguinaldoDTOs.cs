namespace BusinessLogicLayer.DTOs
{
    public class AguinaldoDTO
    {
        public int IdAguinaldo { get; set; }
        public int EmpleadoId { get; set; }
        public string? CodigoEmpleado { get; set; }
        public string? NombreEmpleado { get; set; }
        public string? Departamento { get; set; }
        public string? Puesto { get; set; }

        public DateTime FechaCalculo { get; set; }
        public int DiasTrabajados { get; set; }
        public decimal SalarioPromedio { get; set; }
        public decimal MontoAguinaldo { get; set; }

        public DateTime? FechaPago { get; set; }
        public string? Estado { get; set; }

        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }

    /// <summary>
    /// DTO para calcular aguinaldo de un empleado
    /// </summary>
    public class CalcularAguinaldoDTO
    {
        public int EmpleadoId { get; set; }
        public int Anio { get; set; }
        public DateTime? FechaCorte { get; set; }
    }

    /// <summary>
    /// DTO para calcular aguinaldo masivo (todos los empleados)
    /// </summary>
    public class CalcularAguinaldoMasivoDTO
    {
        public int Anio { get; set; }
        public DateTime? FechaCorte { get; set; }
        public int? DepartamentoId { get; set; }
    }

    /// <summary>
    /// DTO para pagar aguinaldo
    /// </summary>
    public class PagarAguinaldoDTO
    {
        public int IdAguinaldo { get; set; }
        public DateTime FechaPago { get; set; }
    }

    /// <summary>
    /// DTO con resumen de aguinaldos
    /// </summary>
    public class ResumenAguinaldoDTO
    {
        public int TotalEmpleados { get; set; }
        public int AguinaldosPendientes { get; set; }
        public int AguinaldosPagados { get; set; }
        public decimal TotalPendiente { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal TotalGeneral { get; set; }
        public List<AguinaldoDTO> Aguinaldos { get; set; } = new();
    }

    /// <summary>
    /// DTO para resultado de cálculo
    /// </summary>
    public class ResultadoCalculoAguinaldoDTO
    {
        public int EmpleadoId { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public DateTime FechaContratacion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int DiasTrabajados { get; set; }
        public decimal SalarioPromedio { get; set; }
        public decimal MontoAguinaldo { get; set; }
        public string Detalle { get; set; } = string.Empty;
    }
}
