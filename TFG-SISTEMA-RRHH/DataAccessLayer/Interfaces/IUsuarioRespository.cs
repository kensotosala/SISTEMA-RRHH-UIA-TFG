using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.Interfaces
{
    public interface IUsuarioRespository
    {
        Task<Usuarios?> GetByUsernameAsync(string username);
        Task<Usuarios?> CreateAsync(Usuarios usuario);
    }
}
