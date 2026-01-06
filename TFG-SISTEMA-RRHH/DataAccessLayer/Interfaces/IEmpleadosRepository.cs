using DataAccessLayer.Entities;

namespace DataAccessLayer.Interfaces
{
    public interface IEmpleadosRepository
    {
        Task<IEnumerable<Empleados>> GetAllAsync();

        Task<Empleados?> GetByIdAsync(int id);

        Task<Empleados> CreateAsync(Empleados empleado);

        Task<bool> UpdateAsync(Empleados empleado);

        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);

        Task<bool> ExistsByCodigoAsync(string codigo);

        Task<bool> TieneSubordinadosAsync(int id);

        Task<int> ContarSubordinadosAsync(int id);

        Task<bool> EmaillRegistrado(string email);
    }
}