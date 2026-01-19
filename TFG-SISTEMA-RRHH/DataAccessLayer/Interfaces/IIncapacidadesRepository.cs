using DataAccessLayer.Entities;

namespace DataAccessLayer.Interfaces
{
    public interface IIncapacidadesRepository
    {
        Task<bool> ActualizarIncapacidadAsync(Incapacidades incapacidad);

        Task<bool> EliminarIncapacidadAsync(int idIncapacidad);

        Task<IEnumerable<Incapacidades>> ListarIncapacidadesAsync();

        Task<Incapacidades?> ListarIncapacidadPorId(int idIncapacidad);

        Task<Incapacidades?> RegistarIncapacidadesAsync(Incapacidades incapacidad);
    }
}