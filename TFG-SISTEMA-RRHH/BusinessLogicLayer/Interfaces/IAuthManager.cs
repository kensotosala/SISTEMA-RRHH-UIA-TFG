using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Interfaces
{
    public interface IAuthManager
    {
        Task<Usuarios?> ValidarCredencialesAsync(string username, string password);
        Task<Usuarios?> RegistrarNuevoUsuario(Usuarios usuario);
    }
}
