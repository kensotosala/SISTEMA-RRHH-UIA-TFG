using DataAccessLayer.Entities;

namespace DataAccessLayer.Interfaces
{
    public interface IAuditoriaRepository
    {
        Task<IEnumerable<AuditoriaCambios>> ListarLogsAsync();

        Task CrearAsync(AuditoriaCambios auditoria);
    }
}