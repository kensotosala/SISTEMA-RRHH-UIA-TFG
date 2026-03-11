using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;

namespace BusinessLogicLayer.Managers
{
    public class MetricasRendimientoManager : IMetricasRendimientoManager
    {
        private readonly IMetricasRendimientoRepository _repository;

        public MetricasRendimientoManager(IMetricasRendimientoRepository repository)
        {
            _repository = repository;
        }

        public async Task<ResultDTO<IEnumerable<MetricasRendimientoDTO>>> GetMetricasRendimientoAsync()
        {
            try
            {
                // 1. Obtener todas las métricas desde el repositorio
                var metricas = await _repository.GetMetricasRendimiento();

                // 2. Mapear entidades a DTOs
                var metricasDTO = metricas.Select(MapToDTO).ToList();

                // 3. Retornar resultado exitoso
                return ResultDTO<IEnumerable<MetricasRendimientoDTO>>.Success(metricasDTO);
            }
            catch (Exception ex)
            {
    
                return ResultDTO<IEnumerable<MetricasRendimientoDTO>>.Failure(
                    "Error al obtener las métricas de rendimiento.",
                    new List<string> { ex.Message }
                );
            }
        }

        private static MetricasRendimientoDTO MapToDTO(MetricasRendimiento metrica)
        {
            return new MetricasRendimientoDTO
            {
                IdMetrica = metrica.IdMetrica,
                NombreMetrica = metrica.NombreMetrica
            };
        }
    }
}