using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class IncapacidadController : ControllerBase
    {
        private readonly ILogger<IncapacidadController> _logger;
        private readonly IIncapacidadesManager _managerIncapacidades;
        public IncapacidadController(IIncapacidadesManager managerIncapacidades, ILogger<IncapacidadController> logger)
        {
            _managerIncapacidades = managerIncapacidades;
            _logger = logger;
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(IncapacidadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IncapacidadDto>> ActualizarIncapacidad(int id, [FromBody] ActualizarIncapacidadDto dto)
        {
            if (id != dto.IncapacidadId)
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID del body" });

            try
            {
                var incapacidadActualizada = await _managerIncapacidades.ActualizarIncapacidadAsync(dto);
                return Ok(incapacidadActualizada);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                if (id != dto.IncapacidadId)
                    return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID del body" });
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EliminarIncapacidad(int id)
        {
            try
            {
                var eliminado = await _managerIncapacidades.EliminarIncapacidad(id);

                if (!eliminado)
                    return NotFound(new { mensaje = $"No se encontró la incapacidad con ID {id}" });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar incapacidad con ID {Id}", id);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<IncapacidadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<IncapacidadDto>>> ListarIncapacidades()
        {
            var incapacidades = await _managerIncapacidades.ListarIncapacidadesAsync();
            return Ok(incapacidades);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IncapacidadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IncapacidadDto>> ObtenerIncapacidadPorId(int id)
        {
            try
            {
                var incapacidad = await _managerIncapacidades.ObtenerIncapacidadPorIdAsync(id);
                if (incapacidad == null)
                    return NotFound(new { mensaje = $"No se encontró la incapacidad con ID {id}" });

                return Ok(incapacidad);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(IncapacidadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IncapacidadDto>> RegistrarIncapacidad(
    [FromForm] RegistrarIncapacidadDto dto, 
    IFormFile? archivo)
        {
            try
            {
                // Guardar archivo si viene
                if (archivo != null)
                {
                    var extensionesPermitidas = new[] { ".pdf", ".jpg", ".png" };
                    var extension = Path.GetExtension(archivo.FileName).ToLower();

                    if (!extensionesPermitidas.Contains(extension))
                        return BadRequest(new { mensaje = "Formato de archivo no permitido" });

                    var carpeta = Path.Combine("wwwroot", "uploads", "incapacidades");
                    Directory.CreateDirectory(carpeta); // Crea la carpeta si no existe

                    // Nombre único para evitar colisiones
                    var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                    var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                    using var stream = new FileStream(rutaCompleta, FileMode.Create);
                    await archivo.CopyToAsync(stream);

                    // Guardar solo la ruta relativa en el DTO
                    dto.ArchivoAdjunto = $"/uploads/incapacidades/{nombreArchivo}";
                }

                var incapacidadCreada = await _managerIncapacidades.RegistrarIncapacidad(dto);
                return CreatedAtAction(
                    nameof(ObtenerIncapacidadPorId),
                    new { id = incapacidadCreada.IdIncapacidad },
                    incapacidadCreada);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar incapacidad");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }
    }
}