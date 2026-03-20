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
    /// <summary>
    /// Manager de autenticación con JWT incluyendo roles completos
    /// </summary>
    public class AuthManager : IAuthManager
    {
        private readonly IUsuariosRepository _repo;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IConfiguration _config;

        public AuthManager(IUsuariosRepository repo, IPasswordHasher passwordHasher, IConfiguration config)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Login de usuario con migración automática de hash
        /// </summary>
        public async Task<AuthResponseDTO?> LoginAsync(LoginDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            try
            {
                var user = await _repo.GetByUsernameWithDetailsAsync(dto.Username);

                if (user == null)
                    return null; // Esto se convierte en 401

                if (user.Estado != "ACTIVO")
                    throw new InvalidOperationException("Su cuenta está inactiva. Contacte al administrador."); // Esto debe ser 401 también

                if (user.Empleado?.Estado != "ACTIVO")
                    throw new InvalidOperationException("Su cuenta de empleado está inactiva."); // Esto debe ser 401 también

                if (!_passwordHasher.Verify(dto.Password, user.PasswordHash))
                    return null; // Esto se convierte en 401

                user.UltimoAcceso = DateTime.UtcNow;
                await _repo.UpdateAsync(user);

                return GenerateJwt(user);
            }
            catch (InvalidOperationException)
            {
                throw; // Re-lanzar, pero el controlador debe convertirlo a 401
            }
            catch (Exception ex)
            {
                throw new Exception($"Error durante el login: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Registra un nuevo usuario (siempre usa BCrypt)
        /// </summary>
        public async Task<Usuarios?> RegistrarNuevoUsuario(Usuarios usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            try
            {
                // Verificar si el nombre de usuario ya existe
                var exists = await _repo.ExistsByUsernameAsync(usuario.NombreUsuario);
                if (exists)
                    return null;

                // Hash de la contraseña con BCrypt
                usuario.PasswordHash = _passwordHasher.Hash(usuario.PasswordHash);
                usuario.Estado = "ACTIVO";
                usuario.FechaCreacion = DateTime.UtcNow;

                return await _repo.CreateAsync(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al registrar usuario: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Valida las credenciales de un usuario
        /// </summary>
        public async Task<Usuarios?> ValidarCredencialesAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("El nombre de usuario no puede estar vacío", nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));

            try
            {
                var usuario = await _repo.GetByUsernameWithDetailsAsync(username);

                if (usuario == null)
                    return null;

                // Verificar que el usuario esté activo
                if (usuario.Estado != "ACTIVO")
                {
                    throw new InvalidOperationException(
                        "El usuario está inactivo. Contacte al administrador."
                    );
                }

                // Verificar que el empleado asociado esté activo
                if (usuario.Empleado?.Estado != "ACTIVO")
                {
                    throw new InvalidOperationException(
                        "Su cuenta de empleado está inactiva. Contacte al administrador."
                    );
                }

                // Verificar contraseña
                if (!_passwordHasher.Verify(password, usuario.PasswordHash))
                    return null;

                return usuario;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al validar credenciales: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cambia la contraseña de un usuario
        /// </summary>
        public async Task<bool> CambiarPasswordAsync(int userId, CambiarPasswordDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            try
            {
                var usuario = await _repo.GetByIdAsync(userId);
                if (usuario == null)
                    return false;

                // Verificar contraseña actual
                if (!_passwordHasher.Verify(dto.PasswordActual, usuario.PasswordHash))
                    return false;

                // Establecer nueva contraseña (siempre con BCrypt)
                usuario.PasswordHash = _passwordHasher.Hash(dto.NuevaPassword);
                usuario.FechaModificacion = DateTime.UtcNow;

                return await _repo.UpdateAsync(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al cambiar contraseña: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Valida un token JWT
        /// </summary>
        public async Task<bool> ValidarTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            try
            {
                var jwtSettings = _config.GetSection("Jwt");
                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings["Key"]!)
                );

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = key
                };

                tokenHandler.ValidateToken(token, validationParameters, out _);
                return await Task.FromResult(true);
            }
            catch
            {
                return false;
            }
        }

        #region Métodos Privados

        /// <summary>
        /// Genera un JWT con toda la información del usuario INCLUYENDO ROLES
        /// </summary>
        private AuthResponseDTO GenerateJwt(Usuarios user)
        {
            var jwtSettings = _config.GetSection("Jwt");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, user.NombreUsuario),
                new Claim("UserId", user.IdUsuario.ToString()),
                new Claim("Username", user.NombreUsuario)
            };

            UsuarioInfoDTO? usuarioInfo = null;
            if (user.Empleado != null)
            {
                var nombreCompleto = $"{user.Empleado.Nombre} {user.Empleado.PrimerApellido} {user.Empleado.SegundoApellido}".Trim();

                claims.Add(new Claim("EmployeeId", user.Empleado.IdEmpleado.ToString()));
                claims.Add(new Claim("EmployeeCode", user.Empleado.CodigoEmpleado ?? ""));
                claims.Add(new Claim("FullName", nombreCompleto));
                claims.Add(new Claim("Email", user.Empleado.Email ?? ""));
                claims.Add(new Claim("DepartmentId", user.Empleado.DepartamentoId.ToString()));
                claims.Add(new Claim("PositionId", user.Empleado.PuestoId.ToString()));

                if (user.Empleado.JefeInmediatoId.HasValue)
                {
                    claims.Add(new Claim("ManagerId", user.Empleado.JefeInmediatoId.Value.ToString()));
                }

                usuarioInfo = new UsuarioInfoDTO
                {
                    IdUsuario = user.IdUsuario,
                    NombreUsuario = user.NombreUsuario,
                    NombreCompleto = nombreCompleto,
                    Email = user.Empleado.Email,
                    EmpleadoId = user.Empleado.IdEmpleado,
                    CodigoEmpleado = user.Empleado.CodigoEmpleado,
                    DepartamentoId = user.Empleado.DepartamentoId,
                    NombreDepartamento = user.Empleado.Departamento?.NombreDepartamento,
                    PuestoId = user.Empleado.PuestoId,
                    NombrePuesto = user.Empleado.Puesto?.NombrePuesto
                };
            }

            var rolesList = new List<string>();

            if (user.UsuariosRoles != null && user.UsuariosRoles.Any())
            {
                foreach (var usuarioRol in user.UsuariosRoles)
                {
                    if (usuarioRol?.Rol != null && !string.IsNullOrWhiteSpace(usuarioRol.Rol.Nombre))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, usuarioRol.Rol.Nombre));

                        claims.Add(new Claim("RoleId", usuarioRol.RolId.ToString()));

                        rolesList.Add(usuarioRol.Rol.Nombre);
                    }
                }
            }

            if (rolesList.Any())
            {
                claims.Add(new Claim("Roles", string.Join(",", rolesList)));
            }

            if (usuarioInfo != null)
            {
                usuarioInfo.Roles = rolesList;
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!)
            );

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var expiration = DateTime.UtcNow.AddMinutes(
                double.Parse(jwtSettings["ExpireMinutes"]!)
            );

            // Crear token
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
                Expiration = expiration,
                UsuarioInfo = usuarioInfo
            };
        }

        #endregion Métodos Privados
    }
}