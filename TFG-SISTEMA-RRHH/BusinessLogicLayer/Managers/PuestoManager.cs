using AutoMapper;
using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;

namespace BusinessLogicLayer.Managers
{
    public class PuestoManager : IPuestosManager
    {
        private readonly IPuestosRepository _repo;
        private readonly IMapper _mapper;

        public PuestoManager(
            IPuestosRepository puestosRepository,
            IMapper mapper)
        {
            _repo = puestosRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PuestoDTO>> GetAllPuestosAsync()
        {
            var puestos = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<PuestoDTO>>(puestos);
        }

        public async Task<PuestoDTO?> GetPuestoByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            var puesto = await _repo.GetByIdAsync(id);
            return puesto == null ? null : _mapper.Map<PuestoDTO>(puesto);
        }

        public async Task<bool> DeletePuestoAsync(int id)
        {
            if (id <= 0)
                return false;

            var puesto = await _repo.GetByIdAsync(id);
            if (puesto == null)
                return false;

            await _repo.DeleteAsync(puesto.IdPuesto);
            return true;
        }

        public async Task<bool> UpdatePuestoAsync(ActualizarPuestoDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var puesto = await _repo.GetByIdAsync(dto.IdPuesto);
            if (puesto == null)
                return false;

            if (dto.SalarioMinimo > dto.SalarioMaximo)
                throw new ArgumentException("El salario mínimo no puede ser mayor al salario máximo.");

            _mapper.Map(dto, puesto);

            await _repo.UpdateAsync(puesto);

            return true;
        }

        public async Task<int> CreatePuestoAsync(CrearPuestoDTO dto)
        {
            if (dto == null)
                return 0;

            if (dto.SalarioMinimo > dto.SalarioMaximo)
                return 0;

            if (await _repo.ExistsByNameAsync(dto.NombrePuesto))
                return 0;

            var puesto = _mapper.Map<Puestos>(dto);
            puesto.Estado = true;
            puesto.FechaCreacion = DateTime.UtcNow;

            await _repo.CreateAsync(puesto);
            return puesto.IdPuesto;
        }
    }
}