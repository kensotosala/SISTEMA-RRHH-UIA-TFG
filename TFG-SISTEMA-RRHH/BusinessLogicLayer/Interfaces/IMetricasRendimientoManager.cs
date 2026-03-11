using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Interfaces
{
    public interface IMetricasRendimientoManager
    {
        public Task<ResultDTO<IEnumerable<MetricasRendimientoDTO>>> GetMetricasRendimientoAsync();
    }
}
