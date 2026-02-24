using BusinessLogicLayer.DTOs;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Interfaces
{
    public interface IHorasExtrasManager
    {
        Task<IEnumerable<HoraExtraDTO>> GetAllAsync();

        Task<HoraExtraDTO?> GetByIdAsync(int id);

        Task<IEnumerable<HoraExtraDTO>> GetByFiltrosAsync(FiltrosHorasExtrasDTO filtros);

        Task<IEnumerable<HoraExtraDTO>> GetByEmpleadoAsync(int empleadoId);

        Task<IEnumerable<HoraExtraDTO>> GetPendientesByJefeAsync(int jefeId);

        Task<HoraExtraDTO> CreateAsync(CrearHoraExtraDTO dto);

        Task<bool> UpdateAsync(int id, ActualizarHoraExtraDTO dto);

        Task<bool> DeleteAsync(int id);

        Task<bool> AprobarRechazarAsync(int id, AprobarRechazarHoraExtraDTO dto);

        Task<ReporteHorasExtrasDTO> GetReporteEmpleadoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin);

        Task<HoraExtraHoyDTO> ObtenerHoraExtraHoyAsync(int empleadoId);
        Task<HoraExtraDTO?> ObtenerHoraExtraActivaAsync(int empleadoId, DateTime fechaHora);

    }
}