using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.DTOs;
using DataAccessLayer.Entities;
using System.Security.Claims;

namespace PresentationLayer.Controllers
{
    /// <summary>
    /// Controlador para autenticación y gestión de usuarios
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthManager _authManager;

        public AuthController(IAuthManager authManager)
        {
            _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
        }

        /// <summary>
        /// Login de usuario - Genera token JWT
        /// </summary>
        /// <param name="loginDTO">Credenciales de usuario</param>
        /// <returns>Token JWT y datos del usuario</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { mensaje = "Datos inválidos", errores = ModelState });

                var result = await _authManager.LoginAsync(loginDTO);

                if (result == null)
                    return Unauthorized(new { mensaje = "Credenciales inválidas" });

                return Ok(new
                {
                    mensaje = "Login exitoso",
                    token = result.Token,
                    expiracion = result.Expiration,
                    usuario = result.UsuarioInfo
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema
        /// </summary>
        /// <param name="registroDTO">Datos del nuevo usuario</param>
        /// <returns>Usuario creado</returns>
        [HttpPost("registro")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Registro([FromBody] RegistroUsuarioDTO registroDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { mensaje = "Datos inválidos", errores = ModelState });

                var nuevoUsuario = new Usuarios
                {
                    EmpleadoId = registroDTO.EmpleadoId,
                    NombreUsuario = registroDTO.NombreUsuario,
                    PasswordHash = registroDTO.Password // Se hasheará en el manager
                };

                var usuario = await _authManager.RegistrarNuevoUsuario(nuevoUsuario);

                if (usuario == null)
                    return BadRequest(new { mensaje = "El nombre de usuario ya existe" });

                return CreatedAtAction(
                    nameof(ObtenerPerfil),
                    new { },
                    new
                    {
                        mensaje = "Usuario registrado exitosamente",
                        datos = new
                        {
                            idUsuario = usuario.IdUsuario,
                            nombreUsuario = usuario.NombreUsuario,
                            empleadoId = usuario.EmpleadoId,
                            estado = usuario.Estado,
                            fechaCreacion = usuario.FechaCreacion
                        }
                    }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al registrar usuario", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el perfil del usuario autenticado
        /// </summary>
        /// <returns>Información del perfil</returns>
        [HttpGet("perfil")]
        [Authorize]
        public async Task<IActionResult> ObtenerPerfil()
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var perfilInfo = new
                {
                    userId = User.FindFirst("UserId")?.Value,
                    username = User.FindFirst("Username")?.Value,
                    fullName = User.FindFirst("FullName")?.Value,
                    email = User.FindFirst("Email")?.Value,
                    employeeId = User.FindFirst("EmployeeId")?.Value,
                    employeeCode = User.FindFirst("EmployeeCode")?.Value,
                    departmentId = User.FindFirst("DepartmentId")?.Value,
                    positionId = User.FindFirst("PositionId")?.Value,
                    managerId = User.FindFirst("ManagerId")?.Value,
                    roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
                };

                return Ok(new
                {
                    mensaje = "Perfil obtenido exitosamente",
                    datos = perfilInfo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener perfil", error = ex.Message });
            }
        }

        /// <summary>
        /// Valida las credenciales del usuario sin generar token
        /// </summary>
        /// <param name="loginDTO">Credenciales</param>
        /// <returns>Resultado de validación</returns>
        [HttpPost("validar")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidarCredenciales([FromBody] LoginDTO loginDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { mensaje = "Datos inválidos" });

                var usuario = await _authManager.ValidarCredencialesAsync(
                    loginDTO.Username,
                    loginDTO.Password
                );

                if (usuario == null)
                    return Unauthorized(new { mensaje = "Credenciales inválidas" });

                return Ok(new
                {
                    mensaje = "Credenciales válidas",
                    datos = new
                    {
                        idUsuario = usuario.IdUsuario,
                        nombreUsuario = usuario.NombreUsuario,
                        estado = usuario.Estado
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al validar credenciales", error = ex.Message });
            }
        }

        /// <summary>
        /// Cambia la contraseña del usuario autenticado
        /// </summary>
        /// <param name="cambiarPasswordDTO">Datos para cambio de contraseña</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("cambiar-password")]
        [Authorize]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDTO cambiarPasswordDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { mensaje = "Datos inválidos", errores = ModelState });

                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var resultado = await _authManager.CambiarPasswordAsync(userId, cambiarPasswordDTO);

                if (!resultado)
                    return BadRequest(new { mensaje = "La contraseña actual es incorrecta" });

                return Ok(new { mensaje = "Contraseña cambiada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al cambiar contraseña", error = ex.Message });
            }
        }

        /// <summary>
        /// Valida un token JWT
        /// </summary>
        /// <param name="token">Token a validar</param>
        /// <returns>Resultado de validación</returns>
        [HttpPost("validar-token")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidarToken([FromBody] ValidarTokenRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Token))
                    return BadRequest(new { mensaje = "Token requerido" });

                var esValido = await _authManager.ValidarTokenAsync(request.Token);

                return Ok(new
                {
                    mensaje = esValido ? "Token válido" : "Token inválido",
                    esValido = esValido
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al validar token", error = ex.Message });
            }
        }

        /// <summary>
        /// Logout (cliente debe eliminar el token)
        /// </summary>
        /// <returns>Mensaje de confirmación</returns>
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // En JWT stateless, el logout se maneja en el cliente eliminando el token
            return Ok(new { mensaje = "Logout exitoso. Elimine el token del cliente." });
        }
    }

    /// <summary>
    /// Request para validar token
    /// </summary>
    public class ValidarTokenRequest
    {
        public string Token { get; set; } = null!;
    }
}