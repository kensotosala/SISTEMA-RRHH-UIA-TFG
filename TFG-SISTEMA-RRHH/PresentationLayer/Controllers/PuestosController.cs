using ApplicationLayer.Interfaces;
using BusinessLogicLayer.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PuestosController : ControllerBase
    {
        private readonly IPuestoService _puestoService;
        private readonly ILogger<PuestosController> _logger;

        public PuestosController(IPuestoService puestoService, ILogger<PuestosController> logger)
        {
            _puestoService = puestoService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var puestos = await _puestoService.GetAllPuestosAsync();
                return Ok(puestos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los puestos");
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var puesto = await _puestoService.GetPuestoByIdAsync(id);
                if (puesto == null)
                {
                    return NotFound($"No se encontró el puesto con ID {id}");
                }

                return Ok(puesto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener el puesto de ID {id}");
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearPuesto([FromBody] CrearPuestoDTO puestoDto)
        {
            try
            {
                if (puestoDto == null)
                    return BadRequest("El puesto no puede ser nulo.");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var creado = await _puestoService.CreatePuestoAsync(puestoDto);
                return CreatedAtAction(nameof(GetById), new { id = creado.IdPuesto }, creado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al crear el puesto: {puestoDto?.NombrePuesto}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarPuesto(int id, [FromBody] ActualizarPuestoDTO puestoDto)
        {
            try
            {
                if (puestoDto == null)
                    return BadRequest("Los datos enviados son inválidos.");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var resultado = await _puestoService.UpdatePuestoAsync(id, puestoDto);

                if (!resultado)
                    return NotFound($"No se encontró el puesto con ID {id}.");

                return Ok($"Puesto {puestoDto.IdPuesto} actualizado correctamente!");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar el puesto con ID {id}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarPuesto(int id)
        {
            try
            {
                var resultado = await _puestoService.DeletePuestoAsync(id);

                if (!resultado)
                    return NotFound($"No se encontró el puesto con ID {id}.");

                return Ok($"Puesto {id} eliminado correctamente!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar el puesto con ID {id}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }
    }
}