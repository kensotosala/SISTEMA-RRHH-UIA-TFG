using AutoMapper;
using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;

namespace BusinessLogicLayer.Managers
{
    public class DepartamentoManager : IDepartamentosManager
    {
        private readonly IDepartamentosRepository _repo;
        private readonly IMapper _mapper;

        public DepartamentoManager(IDepartamentosRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<DepartamentoDTO> CreateAsync(CrearDepartamentoDTO dto)
        {
            var departamento = _mapper.Map<Departamentos>(dto);
            var createdDepartamento = await _repo.CreateAsync(departamento);
            return _mapper.Map<DepartamentoDTO>(createdDepartamento);
        }

        public async Task DeleteAsync(int id)
        {
            var puesto = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Puesto con ID {id} no encontado");
            await _repo.DeleteAsync(id);
        }

        public async Task<DepartamentoDTO> GetByIdAsync(int id)
        {
            var departamento = await _repo.GetByIdAsync(id);

            if (departamento is null)
                return null;

            return _mapper.Map<DepartamentoDTO>(departamento);
        }

        public async Task<IEnumerable<DepartamentoDTO>> ListAsync()
        {
            var departamentos = await _repo.GetAllAsync();

            if (departamentos == null)
                return Enumerable.Empty<DepartamentoDTO>();

            return _mapper.Map<IEnumerable<DepartamentoDTO>>(departamentos);
        }

        public async Task UpdateAsync(int id, ActualizarDepartamentoDTO dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            var departamento = await _repo.GetByIdAsync(id);

            if (departamento == null)
                throw new KeyNotFoundException($"El departamento con ID {id} no encontrado.");

            _mapper.Map(dto, departamento);

            await _repo.UpdateAsync(departamento);
        }
    }
}