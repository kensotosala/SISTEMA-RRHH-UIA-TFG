using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Mappers
{
    public static class UsuarioMapper
    {
        public static Usuarios ToEntity(CrearUsuarioDTO dto, int empleadoId, IPasswordHasher hasher)
        {
            return new Usuarios
            {
                NombreUsuario = dto.NombreUsuario,
                PasswordHash = hasher.Hash(dto.Password),
                EmpleadoId = empleadoId,
                Estado = "ACTIVO",
                FechaCreacion = DateTime.UtcNow
            };
        }
    }
}