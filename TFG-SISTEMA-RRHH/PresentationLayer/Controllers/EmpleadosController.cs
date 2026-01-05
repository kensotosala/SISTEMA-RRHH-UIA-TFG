using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpleadosController : ControllerBase
    {
        private readonly IEmpleadosManager _manager;

        public EmpleadosController(IEmpleadosManager manager)
        {
            _manager = manager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var empleados = await _manager.ListAsync();

            if (empleados == null || !empleados.Any())
            {
                return Ok(new
                {
                    message = "No hay registros para mostrar",
                    data = new List<object>()
                });
            }

            return Ok(empleados);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var empleado = await _manager.GetByIdAsync(id);

            if (empleado is null)
                return NotFound($"Departamento con ID {id} no encontrado.");

            return Ok(empleado);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearEmpleadoUsuarioDto dto)
        {
            if (dto is null)
                return BadRequest("El cuerpo de la solicitud no puede ser nulo.");

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var creado = await _manager.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = creado.Usuario.NombreUsuario },
                creado
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarEmpleadoUsuarioDto dto)
        {
            if (id <= 0)
                return BadRequest("El ID no es válido.");

            if (dto is null)
                return BadRequest("El cuerpo de la solicitud no puede ser nulo.");

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            await _manager.UpdateAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _manager.DeleteAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    error = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    error = ex.Message
                });
            }
        }
    }
}