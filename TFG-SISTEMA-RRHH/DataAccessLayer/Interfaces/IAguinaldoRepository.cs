using DataAccessLayer.Entities;

namespace DataAccessLayer.Interfaces
{
    /// <summary>
    /// Interfaz del repositorio de Aguinaldos
    /// </summary>
    public interface IAguinaldoRepository
    {
        /// <summary>
        /// Obtiene un aguinaldo por ID
        /// </summary>
        Task<Aguinaldos?> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene todos los aguinaldos
        /// </summary>
        Task<IEnumerable<Aguinaldos>> GetAllAsync();

        /// <summary>
        /// Obtiene aguinaldos por año
        /// </summary>
        Task<IEnumerable<Aguinaldos>> GetByAnioAsync(int anio);

        /// <summary>
        /// Obtiene aguinaldos por empleado
        /// </summary>
        Task<IEnumerable<Aguinaldos>> GetByEmpleadoAsync(int empleadoId);

        /// <summary>
        /// Obtiene aguinaldo de un empleado en un año específico
        /// </summary>
        Task<Aguinaldos?> GetByEmpleadoYAnioAsync(int empleadoId, int anio);

        /// <summary>
        /// Obtiene aguinaldos por estado
        /// </summary>
        Task<IEnumerable<Aguinaldos>> GetByEstadoAsync(string estado);

        /// <summary>
        /// Obtiene aguinaldos por departamento y año
        /// </summary>
        Task<IEnumerable<Aguinaldos>> GetByDepartamentoYAnioAsync(int departamentoId, int anio);

        /// <summary>
        /// Crea un nuevo aguinaldo
        /// </summary>
        Task<Aguinaldos> CreateAsync(Aguinaldos aguinaldo);

        /// <summary>
        /// Actualiza un aguinaldo existente
        /// </summary>
        Task<bool> UpdateAsync(Aguinaldos aguinaldo);

        /// <summary>
        /// Elimina un aguinaldo (soft delete)
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Verifica si existe un aguinaldo para un empleado en un año
        /// </summary>
        Task<bool> ExisteAguinaldoAsync(int empleadoId, int anio);

        /// <summary>
        /// Obtiene nóminas de un empleado en un rango de fechas
        /// </summary>
        Task<IEnumerable<Nominas>> GetNominasPorPeriodoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene empleado con sus relaciones
        /// </summary>
        Task<Empleados?> GetEmpleadoConDetallesAsync(int empleadoId);
    }
}