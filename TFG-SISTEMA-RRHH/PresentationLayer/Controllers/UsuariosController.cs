using BusinessLayer.DTOs;
using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuariosManager _manager;

        public UsuariosController(IUsuariosManager manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        /// <returns>Lista de usuarios</returns>
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var resultado = await _manager.ObtenerTodosAsync();

            if (!resultado.Exitoso)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(new
            {
                mensaje = resultado.Mensaje,
                datos = resultado.Datos
            });
        }

        /// <summary>
        /// Obtiene un usuario por ID
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Usuario encontrado</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var resultado = await _manager.ObtenerPorIdAsync(id);

            if (!resultado.Exitoso)
                return NotFound(new { mensaje = resultado.Mensaje });

            return Ok(new
            {
                mensaje = resultado.Mensaje,
                datos = resultado.Datos
            });
        }

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        /// <param name="crearUsuarioDTO">Datos del nuevo usuario</param>
        /// <returns>Usuario creado</returns>
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearUsuarioDTO crearUsuarioDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _manager.CrearAsync(crearUsuarioDTO);

            if (!resultado.Exitoso)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = resultado.Datos?.IdUsuario },
                new
                {
                    mensaje = resultado.Mensaje,
                    datos = resultado.Datos
                }
            );
        }

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        /// <param name="id">ID del usuario a actualizar</param>
        /// <param name="actualizarDTO">Datos a actualizar</param>
        /// <returns>Usuario actualizado</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarUsuarioDTO actualizarDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != actualizarDTO.IdUsuario)
                return BadRequest(new { mensaje = "El ID no coincide" });

            var resultado = await _manager.ActualizarAsync(actualizarDTO);

            if (!resultado.Exitoso)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(new
            {
                mensaje = resultado.Mensaje,
                datos = resultado.Datos
            });
        }

        /// <summary>
        /// Elimina un usuario (eliminación lógica)
        /// </summary>
        /// <param name="id">ID del usuario a eliminar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado = await _manager.EliminarAsync(id);

            if (!resultado.Exitoso)
                return NotFound(new { mensaje = resultado.Mensaje });

            return Ok(new { mensaje = resultado.Mensaje });
        }

        /// <summary>
        /// Autentica un usuario
        /// </summary>
        /// <param name="loginDTO">Credenciales de acceso</param>
        /// <returns>Usuario autenticado</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _manager.AutenticarAsync(loginDTO);

            if (!resultado.Exitoso)
                return Unauthorized(new { mensaje = resultado.Mensaje });

            return Ok(new
            {
                mensaje = resultado.Mensaje,
                datos = resultado.Datos
            });
        }

        /// <summary>
        /// Cambia el estado de un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="nuevoEstado">Nuevo estado (ACTIVO, INACTIVO, BLOQUEADO)</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _manager.CambiarEstadoAsync(id, request.NuevoEstado);

            if (!resultado.Exitoso)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(new { mensaje = resultado.Mensaje });
        }
    }

    /// <summary>
    /// Request para cambiar el estado de un usuario
    /// </summary>
    public class CambiarEstadoRequest
    {
        public string NuevoEstado { get; set; } = null!;
    }
}
