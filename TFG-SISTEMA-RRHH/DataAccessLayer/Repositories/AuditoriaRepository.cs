using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class AuditoriaRepository : IAuditoriaRepository
    {
        private readonly SistemaRhContext _context;

        public AuditoriaRepository(SistemaRhContext context)
        {
            _context = context;
        }

        public async Task CrearAsync(AuditoriaCambios auditoria)
        {
            await _context.AuditoriaCambios.AddAsync(auditoria);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditoriaCambios>> ListarLogsAsync()
        {
            return await _context.AuditoriaCambios.ToListAsync();
        }
    }
}