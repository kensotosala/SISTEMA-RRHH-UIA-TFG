using DataAccessLayer.Entities;

namespace DataAccessLayer.Interfaces
{
    public interface IDepartamentosRepository
    {
        Task<IEnumerable<Departamentos>> GetAllAsync();

        Task<Departamentos?> GetByIdAsync(int id);

        Task<Departamentos> CreateAsync(Departamentos departamento);

        Task<bool> UpdateAsync(Departamentos departamento);

        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);
    }
}