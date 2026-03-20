using AutoMapper;
using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BusinessLogicLayer.Managers
{
    public class DepartamentoManager : IDepartamentosManager
    {
        private readonly IDepartamentosRepository _repo;
        private readonly IMapper _mapper;
        private readonly IAuditoriaService _auditoria; 


        public DepartamentoManager(IDepartamentosRepository repo, IMapper mapper, IAuditoriaService auditoria)
        {
            _repo = repo;
            _mapper = mapper;
            _auditoria = auditoria;
        }

        public async Task<DepartamentoDTO> CreateAsync(CrearDepartamentoDTO dto)
        {
            var departamento = _mapper.Map<Departamentos>(dto);
            var createdDepartamento = await _repo.CreateAsync(departamento);
            await _auditoria.RegistrarAsync(
                tablaAfectada: "departamentos",
                descripcion: $"Departamento creado: '{createdDepartamento.NombreDepartamento}' " +
                               $"(ID {createdDepartamento.IdDepartamento})."
            );
            return _mapper.Map<DepartamentoDTO>(createdDepartamento);
        }

        public async Task DeleteAsync(int id)
        {
            var departamento = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Departamento con ID {id} no encontrado.");

            var nombre = departamento.NombreDepartamento;

            await _repo.DeleteAsync(id);

            await _auditoria.RegistrarAsync(
                tablaAfectada: "departamentos",
                descripcion: $"Departamento eliminado: '{nombre}' (ID {id})."
            );
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

            var nombreAnterior = departamento.NombreDepartamento;

            _mapper.Map(dto, departamento);
            await _repo.UpdateAsync(departamento);

            await _auditoria.RegistrarAsync(
                tablaAfectada: "departamentos",
                descripcion: $"Departamento ID {id} actualizado. " +
                               $"Nombre anterior: '{nombreAnterior}', " +
                               $"nombre nuevo: '{departamento.NombreDepartamento}'."
            );
        }
    }
}