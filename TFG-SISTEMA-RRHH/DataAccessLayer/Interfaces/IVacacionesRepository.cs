using DataAccessLayer.Entities;

namespace DataAccessLayer.Interfaces
{
    public interface IVacacionesRepository
    {
        /*
         * CRUD Básico
         */

        Task<Vacaciones> CrearAsync(Vacaciones vacacion);

        Task<bool> ActualizarAsync(Vacaciones vacacion);

        Task<bool> EliminarAsync(int id);

        Task<Vacaciones> ObtenerPorIdAsync(int id);

        Task<IEnumerable<Vacaciones>> ObtenerTodosAsync();

        /*
         * CONSULTAS ESPECÍFICAS DEL NEGOCIO
         */

        Task<IEnumerable<Vacaciones>> ObtenerPorEmpleadoIdAsync(int empleadoId);

        Task<IEnumerable<Vacaciones>> ObtenerPorEstadoAsync(string estado);

        Task<IEnumerable<Vacaciones>> ObtenerPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);

        Task<bool> TieneVacacionesEnRangoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin);

        Task<int> ContarDiasVacacionesUsadosAsync(int empleadoId, int anio);

        /*
         * MÉTODOS RELACIONADOS CON SALDOS
         */

        Task<SaldoVacaciones?> ObtenerSaldoVacacionesAsync(int empleadoId, int anio);

        Task<IEnumerable<SaldoVacaciones>> ObtenerHistorialSaldosAsync(int empleadoId);

        Task<SaldoVacaciones> ActualizarSaldoVacacionesAsync(SaldoVacaciones saldo);

        Task<SaldoVacaciones> CalcularYGuardarSaldoAsync(int empleadoId, int anio);

        Task<bool> DescontarDiasVacacionesAsync(int empleadoId, int anio, int dias);
    }
}