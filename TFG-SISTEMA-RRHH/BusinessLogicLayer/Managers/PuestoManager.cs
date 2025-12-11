using AutoMapper;
using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogicLayer.Managers
{
    public class PuestoManager : IPuestosManager
    {
        private readonly IPuestosRepository _puestoRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PuestoManager> _logger;

        public PuestoManager(
            IPuestosRepository puestosRepository,
            IMapper mapper,
            ILogger<PuestoManager> logger)
        {
            _puestoRepository = puestosRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<PuestoDTO>> GetAllPuestosAsync()
        {
            try
            {
                var puestos = await _puestoRepository.GetAllAsync();
                var puestosList = puestos?.ToList() ?? new List<Puestos>();

                _logger.LogInformation($"Retrieved {puestosList.Count} puestos from repository");

                // Mapea a PuestoDTO (de ApplicationLayer)
                var result = _mapper.Map<List<PuestoDTO>>(puestosList);

                _logger.LogInformation($"Mapped to {result.Count} DTOs");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllPuestosAsync");
                throw;
            }
        }

        public async Task<PuestoDTO> GetPuestoByIdAsync(int id)
        {
            var puesto = await _puestoRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Puesto con ID {id} no encontrado.");

            return _mapper.Map<PuestoDTO>(puesto);
        }

        public async Task<PuestoDTO> CreatePuestoAsync(PuestoDTO puestoDto)
        {
            var puesto = _mapper.Map<Puestos>(puestoDto);
            var creado = await _puestoRepository.CreateAsync(puesto);
            return _mapper.Map<PuestoDTO>(creado);
        }

        public async Task UpdatePuestoAsync(PuestoDTO puestoDto)
        {
            if (!await _puestoRepository.ExistsAsync(puestoDto.IdPuesto))
                throw new KeyNotFoundException($"Puesto con ID {puestoDto.IdPuesto} no encontrado");

            var puesto = _mapper.Map<Puestos>(puestoDto);
            await _puestoRepository.UpdateAsync(puesto);
        }

        public async Task DeletePuestoAsync(int id)
        {
            if (!await _puestoRepository.ExistsAsync(id))
                throw new KeyNotFoundException($"Puesto con ID {id} no encontrado");

            await _puestoRepository.DeleteAsync(id);
        }
    }
}