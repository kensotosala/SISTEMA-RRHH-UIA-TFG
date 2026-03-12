using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Shared;

namespace BusinessLogicLayer.Interfaces
{
    public interface INominaManager
    {
        Task<List<DetalleNominaDTO>> GenerarNominaQuincenalAsync(GenerarNominaQuincenalDTO dto);

        Task<NominaDTO?> ObtenerNominaPorIdAsync(int id);
        Task<List<NominaDTO>> ListarNominasAsync();
        Task<List<NominaDTO>> ObtenerNominasPorEmpleadoAsync(int empleadoId);
        Task<List<NominaDTO>> ObtenerNominasQuincenaAsync(int quincena, int mes, int anio);

        Task<AprobarNominaResultado> AprobarNominaAsync(int nominaId);
        Task<PagarNominaResultado> PagarNominaAsync(int nominaId);
        Task<AnularNominaResultado> AnularNominaAsync(int nominaId);

        Task<ResumenNominaQuincenalDTO> ObtenerResumenQuincenaAsync(int quincena, int mes, int anio);
        Task<PlanillaCCSSDTO> GenerarPlanillaCCSSAsync(int mes, int anio);
        Task<DeclaracionD151DTO> GenerarDeclaracionD151Async(int mes, int anio);
    }
}