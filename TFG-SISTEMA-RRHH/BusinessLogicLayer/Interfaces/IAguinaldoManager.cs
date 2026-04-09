using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Interfaces
{
    /// <summary>
    /// Interfaz para la lógica de negocio de Aguinaldos
    /// Implementa las reglas del Código de Trabajo de Costa Rica
    /// </summary>
    public interface IAguinaldoManager
    {
        Task<AguinaldoDTO?> ObtenerPorIdAsync(int id);

        Task<IEnumerable<AguinaldoDTO>> ObtenerTodosAsync();

        Task<IEnumerable<AguinaldoDTO>> ObtenerPorAnioAsync(int anio);

        Task<IEnumerable<AguinaldoDTO>> ObtenerPorEmpleadoAsync(int empleadoId);

        Task<ResumenAguinaldoDTO> ObtenerResumenPorAnioAsync(int anio);

        Task<AguinaldoDTO> CalcularAguinaldoEmpleadoAsync(CalcularAguinaldoDTO dto);

        Task<AguinaldoDTO> CalcularAguinaldoPresenteAnio(int idEmpleado);

        Task<(int registrados, List<string> errores)> CalcularAguinaldoMasivoV2Async();

        Task<(List<AguinaldoDTO> registrados, List<string> errores)> CalcularAguinaldoMasivoAsync(
            CalcularAguinaldoMasivoDTO dto);

        Task<bool> PagarAguinaldoAsync(int idAguinaldo, DateTime fechaPago);

        Task<(int exitosos, int fallidos, List<string> errores)> PagarAguinaldosMasivoAsync(
            List<int> idsAguinaldos, DateTime fechaPago);

        Task<bool> AnularAguinaldoAsync(int idAguinaldo);
    }
}