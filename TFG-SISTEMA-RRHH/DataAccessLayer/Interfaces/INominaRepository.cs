using DataAccessLayer.Entities;

namespace DataAccessLayer.Interfaces
{
    public interface INominaRepository
    {
        // CRUD básico
        Task<Nominas> CrearNominaAsync(Nominas nomina);

        Task<Nominas?> ObtenerNominaPorIdAsync(int id);

        Task<List<Nominas>> ListarNominasAsync();

        Task<Nominas> ActualizarNominaAsync(Nominas nomina);

        Task<bool> EliminarNominaAsync(int id);

        // Consultas específicas
        Task<List<Nominas>> ObtenerNominasPorEmpleadoAsync(int empleadoId);

        Task<List<Nominas>> ObtenerNominasPorPeriodoAsync(DateTime periodoInicio, DateTime periodoFin);

        Task<List<Nominas>> ObtenerNominasQuincenaAsync(int quincena, int mes, int anio);

        Task<Nominas?> ObtenerNominaEmpleadoQuincenaAsync(int empleadoId, int quincena, int mes, int anio);

        Task<Nominas?> ObtenerNominaParcialEmpleadoQuincenaAsync(
    int empleadoId, int quincena, int mes, int anio);

        // Validaciones
        Task<bool> ExisteNominaQuincenaAsync(int empleadoId, int quincena, int mes, int anio);

        // Reportes
        Task<List<Nominas>> ObtenerNominasMesAsync(int mes, int anio);

        Task<decimal> ObtenerTotalNominaMesAsync(int mes, int anio);
    }
}