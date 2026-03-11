using DataAccessLayer.Entities;

namespace DataAccessLayer.Interfaces
{
    public interface IMetricasRendimientoRepository
    {
        public Task<IEnumerable<MetricasRendimiento>> GetMetricasRendimiento();
    }
}