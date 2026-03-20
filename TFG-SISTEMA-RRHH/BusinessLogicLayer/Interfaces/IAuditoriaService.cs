using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Interfaces
{
    public interface IAuditoriaService
    {
        Task<IEnumerable<AuditoriaCambios>> ListarAsync();

        Task RegistrarAsync(string tablaAfectada, string descripcion);

    }
}
