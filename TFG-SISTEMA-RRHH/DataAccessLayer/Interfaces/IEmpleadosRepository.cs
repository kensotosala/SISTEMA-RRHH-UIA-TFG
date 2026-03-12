using DataAccessLayer.Entities;

namespace DataAccessLayer.Interfaces
{
    public interface IEmpleadosRepository
    {
        Task<int> ContarSubordinadosAsync(int id);

        Task<Empleados> CreateAsync(Empleados empleado);

        Task<bool> DeleteAsync(int id);

        Task<bool> EmaillRegistrado(string email);

        Task<bool> ExistsAsync(int id);

        Task<bool> ExistsByCodigoAsync(string codigo);

        Task<IEnumerable<Empleados>> GetAllAsync();

        Task<IEnumerable<Empleados>> GetAllIncludingInactiveAsync();

        Task<IEnumerable<Empleados>> GetAllWithUsersAndRolesAsync();

        Task<Empleados?> GetByIdAsync(int id);

        Task<bool> TieneSubordinadosAsync(int id);

        Task<bool> UpdateAsync(Empleados empleado);

        Task<List<Empleados>> GetByIdsAsync(IEnumerable<int> ids);
    }
}