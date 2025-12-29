using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.Repositories
{
    public class UsuarioRepository : IUsuarioRespository
    {
        private readonly SistemaRhContext _context;

        public UsuarioRepository(SistemaRhContext context)
        {
            _context = context;
        }

        public async Task<Usuarios?> CreateAsync(Usuarios usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<Usuarios?> GetByUsernameAsync(string username)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == username);
        }
    }
}