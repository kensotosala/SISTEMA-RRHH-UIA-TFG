using ApplicationLayer.Interfaces;
using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;

namespace ApplicationLayer.Services
{
    public class DepartamentoService : IDepartamentoService
    {
        private readonly IDepartamentosManager _manager;

        public DepartamentoService(IDepartamentosManager manager)
        {
            _manager = manager;
        }

        public async Task<DepartamentoDTO> CreateAsync(DepartamentoDTO dto)
        {
            return await _manager.CreateAsync(dto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _manager.DeleteAsync(id);
            return true;
        }

        public async Task<IEnumerable<DepartamentoDTO>> GetAllAsync()
        {
            var departamentos = await _manager.ListAsync();
            return departamentos;
        }

        public async Task<DepartamentoDTO?> GetByIdAsync(int id)
        {
            return await _manager.GetByIdAsync(id);
        }

        public async Task<bool> UpdateAsync(DepartamentoDTO dto)
        {
            await _manager.UpdateAsync(dto);
            return true;
        }
    }
}