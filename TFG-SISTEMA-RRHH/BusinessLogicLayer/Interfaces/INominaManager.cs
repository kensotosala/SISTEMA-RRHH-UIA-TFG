using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Interfaces
{
    public interface INominaManager
    {
        // Generar nómina quincenal
        Task<List<DetalleNominaDTO>> GenerarNominaQuincenalAsync(GenerarNominaQuincenalDTO dto);

        // Consultar nóminas
        Task<NominaDTO?> ObtenerNominaPorIdAsync(int id);
        Task<List<NominaDTO>> ListarNominasAsync();
        Task<List<NominaDTO>> ObtenerNominasPorEmpleadoAsync(int empleadoId);
        Task<List<NominaDTO>> ObtenerNominasQuincenaAsync(int quincena, int mes, int anio);

        // Aprobar y pagar
        Task<bool> AprobarNominaAsync(int nominaId);
        Task<bool> PagarNominaAsync(int nominaId);
        Task<bool> AnularNominaAsync(int nominaId);

        // Reportes
        Task<ResumenNominaQuincenalDTO> ObtenerResumenQuincenaAsync(int quincena, int mes, int anio);
        Task<PlanillaCCSSDTO> GenerarPlanillaCCSSAsync(int mes, int anio);
        Task<DeclaracionD151DTO> GenerarDeclaracionD151Async(int mes, int anio);
    }
}