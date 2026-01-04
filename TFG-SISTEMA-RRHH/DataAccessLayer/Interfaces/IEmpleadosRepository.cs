using DataAccessLayer.Entities;

namespace DataAccessLayer.Interfaces
{
    public interface IEmpleadosRepository
    {
        Task<IEnumerable<Empleados>> GetAllAsync();

        Task<Empleados?> GetByIdAsync(int id);

        Task<Empleados> CreateAsync(Empleados empleado);

        Task UpdateAsync(Empleados empleado);

        Task DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);
    }
}