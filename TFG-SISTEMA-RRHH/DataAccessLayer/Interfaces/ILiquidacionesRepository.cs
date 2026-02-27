using DataAccessLayer.Entities;

namespace DataAccessLayer.Interfaces
{
    public interface ILiquidacionesRepository
    {
        // CRUD
        Task<IEnumerable<Liquidaciones>> ListarLiquidaciones();

        Task<bool> ModificarLiquidacion(Liquidaciones liquidacion);

        Task<bool> AnularLiquidacion(int id);

        Task<Liquidaciones> CrearLiquidacion(Liquidaciones liquidacion);

        Task<Liquidaciones?> ObtenerLiquidacionPorId(int id);

        // Otros métodos específicos
        Task<List<Nominas>> ObtenerNominasUltimos6Meses(int idEmpleado);
        Task<List<Nominas>> ObtenerNominasUltimos12Meses(int idEmpleado);

        Task<Empleados?> ObtenerEmpleadoPorId(int idEmpleado);
    }
}