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

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<DepartamentoDTO> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<DepartamentoDTO>> ListAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(DepartamentoDTO dto)
        {
            throw new NotImplementedException();
        }
    }
}