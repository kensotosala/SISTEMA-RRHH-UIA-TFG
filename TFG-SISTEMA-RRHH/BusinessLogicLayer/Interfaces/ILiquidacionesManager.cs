using BusinessLogicLayer.DTOs;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Interfaces
{
    public interface ILiquidacionesManager
    {
        // CRUD
        Task<Liquidaciones> CrearLiquidacion(int idEmpleado, DateOnly fechaSalida, string motivo, bool preavisoEntregado = true);

        Task<ResultDTO<bool>> ModificarLiquidacion(Liquidaciones liquidacion);

        Task<Liquidaciones?> ObtenerLiquidacionPorId(int idLiquidacion);

        Task<ResultDTO<bool>> AnularLiquidacion(int idLiquidacion);

        Task<ResultDTO<IEnumerable<LiquidacionDTO>>> ListarLiquidaciones();

        // Cálculos
        Task<decimal> CalcularSalarioPromedio(int idEmpleado);

        Task<ResultadoPreaviso> CalcularPreaviso(int idEmpleado, DateOnly fechaSalida);

        Task<ResultadoAuxilioCesantia> CalcularAuxilioCesantia(int idEmpleado, DateOnly fechaSalida);

        Task<ResultadoVacacionesProporcionales> CalcularVacacionesProporcionales(int idEmpleado, DateOnly fechaSalida);

        Task<ResultadoAguinaldoProporcional> CalcularAguinaldoProporcional(int idEmpleado, DateOnly fechaSalida);

        Task<LiquidacionDTO> CalcularLiquidacion(int idEmpleado, DateOnly fechaSalida);
    }
}