using ApplicationLayer.Interfaces;
using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop.Infrastructure;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApplicationLayer.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthManager _authManager;
        private readonly IConfiguration _config;

        public AuthService(IAuthManager authManager, IConfiguration config)
        {
            _authManager = authManager;
            _config = config;
        }

        public async Task<string?> LoginAsync(LoginDTO login)
        {
            var user = await _authManager.ValidarCredencialesAsync(login.NombreUsuario, login.PasswordHash);
            if (user == null) return null;

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.NombreUsuario),
                    new Claim("UserId", user.IdUsuario.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(

                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(2),
                signingCredentials: creds

                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public Task<CrearPuestoDTO?> RegisterNewUser(RegistrarUsuarioDTO dto)
        {
            var usuario = new Usuarios
            {
                NombreUsuario = dto.NombreUsuario,
                EmpleadoId = dto.EmpleadoId,
                PasswordHash = dto.Password,
                UltimoAcceso = DateTime.UtcNow,
                Estado = "ACTIVO",
                FechaCreacion = DateTime.UtcNow,
                FechaModificacion = DateTime.UtcNow,
            };
        }
    }
}