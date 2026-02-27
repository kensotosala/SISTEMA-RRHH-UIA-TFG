namespace BusinessLogicLayer.DTOs
{
    public class LiquidacionDTO
    {
        public int IdEmpleado { get; set; }
        public decimal MontoPreaviso { get; set; }
        public decimal MontoVacaciones { get; set; }
        public decimal MontoAguinaldo { get; set; }
        public decimal MontoCesantia { get; set; }
        public decimal MontoTotal => MontoPreaviso + MontoVacaciones + MontoAguinaldo + MontoCesantia;
    }

    public class ResultadoPreaviso
    {
        public int DiasPreaviso { get; set; }
        public decimal MontoPreaviso { get; set; }
    }

    public class ResultadoAuxilioCesantia
    {
        public int MesesLaborados { get; set; }
        public decimal MontoAuxilioCesantia { get; set; }
    }

    public class ResultadoVacacionesProporcionales
    {
        public int DiasVacacionesProporcionales { get; set; }
        public decimal MontoVacacionesProporcionales { get; set; }
    }

    public class ResultadoAguinaldoProporcional
    {
        public decimal MontoAguinaldoProporcional { get; set; }
    }
}