using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Interfaces
{
    public interface IVacacionesManager
    {
        // ========================================
        // OPERACIONES CRUD CON DTOs
        // ========================================

        Task<ResultDTO<ListarVacacionByIdDTO>> CrearSolicitudAsync(CrearVacacionDTO dto);

        Task<ResultDTO<bool>> ActualizarSolicitudAsync(int id, ActualizarVacacionDTO dto);

        Task<ResultDTO<bool>> CancelarSolicitudAsync(int id);

        Task<ResultDTO<ListarVacacionByIdDTO>> ObtenerPorIdAsync(int id);

        Task<ResultDTO<IEnumerable<ListarVacacionesDTO>>> ObtenerTodosAsync();

        Task<ResultDTO<IEnumerable<ListarVacacionesDTO>>> ObtenerPorEmpleadoAsync(int empleadoId);

        Task<ResultDTO<IEnumerable<ListarVacacionesDTO>>> ObtenerPorEstadoAsync(string estado);

        // ========================================
        // OPERACIONES DE APROBACIÓN/RECHAZO
        // ========================================

        Task<ResultDTO<bool>> AprobarSolicitudAsync(int idVacacion, int jefeId);

        Task<ResultDTO<bool>> RechazarSolicitudAsync(int idVacacion, int jefeId, string comentarios);

        // ========================================
        // CONSULTAS DE SALDO Y DISPONIBILIDAD
        // ========================================

        Task<ResultDTO<SaldoVacacionesDTO>> ObtenerSaldoAsync(int empleadoId, int anio);

        Task<ResultDTO<IEnumerable<SaldoVacacionesDTO>>> ObtenerHistorialSaldosAsync(int empleadoId);

        Task<ResultDTO<SaldoVacacionesDTO>> RecalcularSaldoAsync(int empleadoId, int anio);

        // ========================================
        // VALIDACIONES DE NEGOCIO
        // ========================================

        Task<ResultDTO<ValidacionVacacionesDTO>> ValidarSolicitudAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin);
    }
}