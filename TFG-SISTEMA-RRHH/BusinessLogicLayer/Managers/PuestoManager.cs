using AutoMapper;
using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;

namespace BusinessLogicLayer.Managers
{
    public class PuestoManager : IPuestosManager
    {

        private readonly IPuestosRepository _puestoRepository;
        private readonly IMapper _mapper;

        public PuestoManager(IPuestosRepository puestosRepository, IMapper mapper)
        {
            _puestoRepository = puestosRepository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<PuestoDTO>> GetAllPuestosAsync()
        {
           var puestos = await _puestoRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<PuestoDTO>>(puestos);
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
            if(!await _puestoRepository.ExistsAsync(id))
                throw new KeyNotFoundException($"Puesto con ID {id} no encontrado");

            await _puestoRepository.DeleteAsync(id);
        }
    }
}
