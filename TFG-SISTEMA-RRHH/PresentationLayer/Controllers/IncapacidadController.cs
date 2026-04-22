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
        private readonly IWebHostEnvironment _environment;

        public IncapacidadController(
            IIncapacidadesManager managerIncapacidades,
            ILogger<IncapacidadController> logger,
            IWebHostEnvironment environment)
        {
            _managerIncapacidades = managerIncapacidades;
            _logger = logger;
            _environment = environment;
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(IncapacidadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IncapacidadDto>> ActualizarIncapacidad(
            int id,
            [FromBody] ActualizarIncapacidadDto dto)
        {
            if (dto == null)
                return BadRequest(new { mensaje = "El cuerpo de la solicitud no puede estar vacío" });

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
                _logger.LogError(ex, "Error al actualizar incapacidad {Id}", id);
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
                if (id <= 0)
                    return BadRequest(new { mensaje = "El ID debe ser mayor a cero" });

                var incapacidad = await _managerIncapacidades.ObtenerIncapacidadPorIdAsync(id);
                if (incapacidad == null)
                    return NotFound(new { mensaje = $"No se encontró la incapacidad con ID {id}" });

                return Ok(incapacidad);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener incapacidad {Id}", id);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(IncapacidadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IncapacidadDto>> RegistrarIncapacidad(
            [FromForm] RegistrarIncapacidadDto dto)
        {

            _logger.LogInformation("=== REGISTRO INCAPACIDAD ===");
            _logger.LogInformation("DTO recibido: EmpleadoId={EmpleadoId}, Tipo={Tipo}, Diagnostico={Diagnostico}",
                dto?.EmpleadoId, dto?.TipoIncapacidad, dto?.Diagnostico);
            _logger.LogInformation("Form files count: {Count}", Request.Form.Files.Count);
            foreach (var file in Request.Form.Files)
            {
                _logger.LogInformation("File: Name={Name}, FileName={FileName}, Length={Length}",
                    file.Name, file.FileName, file.Length);
            }


            if (dto == null)
                return BadRequest(new { mensaje = "El DTO no puede ser nulo" });

            if (dto.FechaFin < dto.FechaInicio)
                return BadRequest(new { mensaje = "La fecha fin no puede ser menor a la fecha de inicio" });

            var archivo = Request.Form.Files.GetFile("archivo");

            if (archivo != null && archivo.Length > 0)
            {
                var extensionesPermitidas = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(archivo.FileName).ToLower();

                if (!extensionesPermitidas.Contains(extension))
                    return BadRequest(new { mensaje = "Formato no permitido. Use: PDF, JPG, PNG" });

                if (archivo.Length > 5 * 1024 * 1024)
                    return BadRequest(new { mensaje = "El archivo no puede ser mayor a 5MB" });

                var carpeta = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "incapacidades");
                Directory.CreateDirectory(carpeta);

                var nombreArchivo = $"{Guid.NewGuid():N}{extension}";
                var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                using var stream = new FileStream(rutaCompleta, FileMode.Create);
                await archivo.CopyToAsync(stream);

                dto.ArchivoAdjunto = $"/uploads/incapacidades/{nombreArchivo}";
            }

            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar incapacidad");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpGet("empleado/{empleadoId}")]
        [ProducesResponseType(typeof(IEnumerable<IncapacidadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<IncapacidadDto>>> ListarPorEmpleado(int empleadoId)
        {
            var todas = await _managerIncapacidades.ListarIncapacidadesAsync();
            var filtradas = todas.Where(i => i.EmpleadoId == empleadoId);
            return Ok(filtradas);
        }
    }
}