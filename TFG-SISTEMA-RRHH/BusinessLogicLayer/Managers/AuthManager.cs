using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BusinessLogicLayer.Managers
{
    public class AuthManager : IAuthManager
    {
        private readonly IUsuarioRepository _repo;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IConfiguration _config;

        public AuthManager(IUsuarioRepository repo, IPasswordHasher passwordHasher, IConfiguration config)
        {
            _repo = repo;
            _passwordHasher = passwordHasher;
            _config = config;
        }

        public async Task<AuthResponseDTO?> LoginAsync(LoginDTO dto)
        {
            var user = await _repo.GetByUsernameWithDetailsAsync(dto.Username);

            if (user == null)
                return null;

            if (user.Estado != "ACTIVO")
                return null;

            if (!_passwordHasher.Verify(dto.Password, user.PasswordHash))
                return null;

            user.UltimoAcceso = DateTime.UtcNow;
            await _repo.UpdateAsync(user);

            return GenerateJwt(user);
        }

        public async Task<Usuarios?> RegistrarNuevoUsuario(Usuarios usuario)
        {
            var exists = await _repo.ExistsByUsernameAsync(usuario.NombreUsuario);
            if (exists)
                return null;

            usuario.PasswordHash = _passwordHasher.Hash(usuario.PasswordHash);
            usuario.Estado = "Activo";
            usuario.FechaCreacion = DateTime.UtcNow;

            return await _repo.CreateAsync(usuario);
        }

        public async Task<Usuarios?> ValidarCredencialesAsync(string username, string password)
        {
            var user = await _repo.GetByUsernameAsync(username);
            if (user == null)
                return null;

            return _passwordHasher.Verify(password, user.PasswordHash)
                ? user
                : null;
        }

        private AuthResponseDTO GenerateJwt(Usuarios user)
        {
            var jwtSettings = _config.GetSection("Jwt");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, user.NombreUsuario)
            };

            foreach (var role in user.UsuariosRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Rol.Nombre));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!)
            );

            var creds = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256
            );

            var expiration = DateTime.UtcNow.AddMinutes(
                double.Parse(jwtSettings["ExpireMinutes"]!)
            );

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            return new AuthResponseDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }
    }
}