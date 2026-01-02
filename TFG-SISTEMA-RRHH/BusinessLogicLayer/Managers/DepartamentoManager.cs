using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;

namespace BusinessLogicLayer.Managers
{
    public class DepartamentoManager : IDepartamentosManager
    {
        private readonly IDepartamentosRepository _repo;

        public DepartamentoManager(IDepartamentosRepository repo)
        {
            _repo = repo;
        }

        public async Task<DepartamentoDTO> CreateAsync(DepartamentoDTO dto)
        {
            var departamento = new Departamentos
            {
                NombreDepartamento = dto.NombreDepartamento,
                Descripcion = dto.Descripcion,
                IdJefeDepartamento = dto.IdJefeDepartamento,
                Estado = dto.Estado,
                FechaCreacion = DateTime.UtcNow,
                FechaModificacion = DateTime.UtcNow
            };

            var createdDepartamento = await _repo.CreateAsync(departamento);

            var createdDepartamentoDTO = new DepartamentoDTO
            {
                IdDepartamento = createdDepartamento.IdDepartamento,
                NombreDepartamento = createdDepartamento.NombreDepartamento,
                Descripcion = createdDepartamento.Descripcion,
                IdJefeDepartamento = createdDepartamento.IdJefeDepartamento,
                Estado = createdDepartamento.Estado,
            };

            return createdDepartamentoDTO;
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

            return new DepartamentoDTO
            {
                IdDepartamento = departamento.IdDepartamento,
                NombreDepartamento = departamento.NombreDepartamento,
                Descripcion = departamento.Descripcion,
                IdJefeDepartamento = departamento.IdJefeDepartamento,
                Estado = departamento.Estado
            };
        }

        public async Task<IEnumerable<DepartamentoDTO>> ListAsync()
        {
            var departamentos = await _repo.GetAllAsync();

            if (departamentos == null)
                return Enumerable.Empty<DepartamentoDTO>();

            var result = departamentos.Select(d => new DepartamentoDTO
            {
                IdDepartamento = d.IdDepartamento,
                NombreDepartamento = d.NombreDepartamento,
                Descripcion = d.Descripcion,
                IdJefeDepartamento = d.IdJefeDepartamento,
                Estado = d.Estado
            });

            return result;
        }

        public async Task UpdateAsync(DepartamentoDTO dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto), "Datos del departamento no pueden ser nulos.");

            var departamento = await _repo.GetByIdAsync(dto.IdDepartamento);

            if (departamento == null)
                throw new KeyNotFoundException($"El departamento con ID {dto.IdDepartamento} no encontrado.");

            departamento.NombreDepartamento = dto.NombreDepartamento;
            departamento.Descripcion = dto.Descripcion;
            departamento.IdJefeDepartamento = dto.IdJefeDepartamento;
            departamento.Estado = dto.Estado;
            departamento.FechaModificacion = DateTime.UtcNow;

            await _repo.UpdateAsync(departamento);
        }
    }
}