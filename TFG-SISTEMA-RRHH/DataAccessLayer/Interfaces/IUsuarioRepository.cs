using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<Usuarios>> GetAllAsync();

        Task<Usuarios?> GetByIdAsync(int id);

        Task<Usuarios?> GetByUsernameAsync(string username);

        Task<Usuarios?> CreateAsync(Usuarios usuario);

        Task<bool> UpdateAsync(Usuarios usuario);

        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);

        Task<bool> ExistsByUsernameAsync(string username);
    }
}