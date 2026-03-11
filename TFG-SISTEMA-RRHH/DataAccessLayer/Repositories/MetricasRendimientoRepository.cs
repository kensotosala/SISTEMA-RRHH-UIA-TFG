using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class MetricasRendimientoRepository : IMetricasRendimientoRepository
    {

        private readonly SistemaRhContext _context;

        public MetricasRendimientoRepository(SistemaRhContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MetricasRendimiento>> GetMetricasRendimiento()
        {
            return await _context.MetricasRendimiento.ToListAsync();
        }
    }
}
