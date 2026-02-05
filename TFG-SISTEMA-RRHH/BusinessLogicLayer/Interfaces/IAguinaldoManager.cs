using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Interfaces
{
    /// <summary>
    /// Interfaz para la lógica de negocio de Aguinaldos
    /// Implementa las reglas del Código de Trabajo de Costa Rica
    /// </summary>
    public interface IAguinaldoManager
    {
        /// <summary>
        /// Obtiene un aguinaldo por ID
        /// </summary>
        Task<AguinaldoDTO?> ObtenerPorIdAsync(int id);

        /// <summary>
        /// Obtiene todos los aguinaldos
        /// </summary>
        Task<IEnumerable<AguinaldoDTO>> ObtenerTodosAsync();

        /// <summary>
        /// Obtiene aguinaldos por año
        /// </summary>
        Task<IEnumerable<AguinaldoDTO>> ObtenerPorAnioAsync(int anio);

        /// <summary>
        /// Obtiene aguinaldos de un empleado
        /// </summary>
        Task<IEnumerable<AguinaldoDTO>> ObtenerPorEmpleadoAsync(int empleadoId);

        /// <summary>
        /// Obtiene resumen de aguinaldos por año
        /// </summary>
        Task<ResumenAguinaldoDTO> ObtenerResumenPorAnioAsync(int anio);

        /// <summary>
        /// Calcula aguinaldo para un empleado específico
        /// Según Art. 229 del Código de Trabajo de Costa Rica
        /// </summary>
        Task<ResultadoCalculoAguinaldoDTO> CalcularAguinaldoEmpleadoAsync(CalcularAguinaldoDTO dto);

        /// <summary>
        /// Calcula aguinaldos para todos los empleados activos
        /// </summary>
        Task<List<ResultadoCalculoAguinaldoDTO>> CalcularAguinaldoMasivoAsync(CalcularAguinaldoMasivoDTO dto);

        /// <summary>
        /// Registra un aguinaldo calculado en la base de datos
        /// </summary>
        Task<AguinaldoDTO> RegistrarAguinaldoAsync(ResultadoCalculoAguinaldoDTO calculo, int anio);

        /// <summary>
        /// Paga un aguinaldo
        /// </summary>
        Task<bool> PagarAguinaldoAsync(int idAguinaldo, DateTime fechaPago);

        /// <summary>
        /// Paga múltiples aguinaldos
        /// </summary>
        Task<(int exitosos, int fallidos, List<string> errores)> PagarAguinaldosMasivoAsync(
            List<int> idsAguinaldos,
            DateTime fechaPago);

        /// <summary>
        /// Anula un aguinaldo
        /// </summary>
        Task<bool> AnularAguinaldoAsync(int idAguinaldo);

        /// <summary>
        /// Calcula el promedio salarial según legislación CR
        /// </summary>
        Task<decimal> CalcularSalarioPromedioAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Calcula días trabajados en el período
        /// </summary>
        Task<int> CalcularDiasLaboradosAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin);
    }
}