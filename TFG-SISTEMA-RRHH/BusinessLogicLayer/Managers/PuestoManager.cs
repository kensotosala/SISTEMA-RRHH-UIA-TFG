namespace BusinessLogicLayer.Managers
{
    using AutoMapper;
    using BusinessLogicLayer.DTOs;
    using BusinessLogicLayer.Interfaces;
    using DataAccessLayer.Entities;
    using DataAccessLayer.Interfaces;
    using Microsoft.Extensions.Logging;

    public class PuestoManager : IPuestosManager
    {
        private readonly IPuestosRepository _puestoRepository;

        private readonly IMapper _mapper;

        private readonly ILogger<PuestoManager> _logger;

        public PuestoManager(IPuestosRepository puestosRepository, IMapper mapper, ILogger<PuestoManager> logger)
        {
            _puestoRepository = puestosRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<PuestoDto>> GetAllPuestosAsync()
        {
            try
            {
                return _mapper.Map<List<PuestoDto>>(await _puestoRepository.GetAllAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los puestos");
                throw;
            }
        }

        public async Task<PuestoDto> GetPuestoByIdAsync(int id)
        {
            var puesto = await _puestoRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Puesto con ID {id} no encontrado.");

            return _mapper.Map<PuestoDto>(puesto);
        }

        public async Task<PuestoDto> CreatePuestoAsync(PuestoDto puestoDto)
        {
            var puesto = _mapper.Map<Puestos>(puestoDto);
            var creado = await _puestoRepository.CreateAsync(puesto);
            return _mapper.Map<PuestoDto>(creado);
        }

        public async Task UpdatePuestoAsync(PuestoDto puestoDto)
        {
            if (!await _puestoRepository.ExistsAsync(puestoDto.IdPuesto))
                throw new KeyNotFoundException($"Puesto con ID {puestoDto.IdPuesto} no encontrado");

            var puesto = _mapper.Map<Puestos>(puestoDto);
            await _puestoRepository.UpdateAsync(puesto);
        }

        public async Task DeletePuestoAsync(int id)
        {
            if(!await _puestoRepository.ExistsAsync(id))
                throw new KeyNotFoundException($"Puesto con ID {id} no encontrado");

            await _puestoRepository.DeleteAsync(id);
        }
    }
}
