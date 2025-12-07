using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.Interfaces
{
    public interface IPuestosRepository
    {
        Task<IEnumerable<Puestos>> GetAllAsync();
        Task<Puestos?> GetByIdAsync(int? id);
        Task<Puestos> CreateAsync(Puestos puesto);
        Task UpdateAsync(Puestos puesto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
