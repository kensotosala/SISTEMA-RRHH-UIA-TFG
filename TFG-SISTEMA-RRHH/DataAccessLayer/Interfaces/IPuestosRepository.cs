namespace DataAccessLayer.Interfaces
{
    using DataAccessLayer.Entities;

    public interface IPuestosRepository
    {
        Task<IEnumerable<Puestos>> GetAllAsync();

        Task<Puestos?> GetByIdAsync(int id);

        Task<Puestos> CreateAsync(Puestos puesto);

        Task<bool> UpdateAsync(Puestos puesto);

        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);
    }
}
