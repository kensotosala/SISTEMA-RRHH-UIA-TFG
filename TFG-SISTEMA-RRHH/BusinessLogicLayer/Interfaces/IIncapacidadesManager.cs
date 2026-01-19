using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Interfaces
{
    public interface IIncapacidadesManager
    {
        Task<IncapacidadDto> ActualizarIncapacidadAsync(ActualizarIncapacidadDto dto);

        Task<bool> EliminarIncapacidad(int id);

        Task<IEnumerable<IncapacidadDto>> ListarIncapacidadesAsync();

        Task<IncapacidadDto?> ObtenerIncapacidadPorIdAsync(int id);

        Task<IncapacidadDto> RegistrarIncapacidad(RegistrarIncapacidadDto dto);
    }
}