using DataAccessLayer.Entities;

namespace DataAccessLayer.Interfaces
{
    public interface ILiquidacionesRepository
    {
        // CRUD
        Task<IEnumerable<Liquidaciones>> ListarLiquidaciones();

        Task<bool> ModificarLiquidacion(int id);

        Task<bool> AnularLiquidacion(int id);

        Task<Liquidaciones> CalcularLiquidacion();

        Task<Liquidaciones?> ObtenerLiquidacionPorId(int id);

        // Otros métodos específicos
        Task<double> CalcularSalarioPromedio();

        Task<double> CalcularPreaviso();

        Task<double> CalcularAuxilioCesantia();

        Task<double> CalcularVacacionesProporcionales();

        Task<double> CalcularAguinaldoProporcional();
    }
}