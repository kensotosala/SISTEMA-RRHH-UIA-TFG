using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PuestosController : ControllerBase
    {
        private readonly IPuestosManager _puestoService;
        private readonly ILogger<PuestosController> _logger;

        public PuestosController(IPuestosManager puestoService, ILogger<PuestosController> logger)
        {
            _puestoService = puestoService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PuestoDTO>>> GetAll()
        {
            var puestos = await _puestoService.GetAllPuestosAsync();
            return Ok(puestos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PuestoDTO>> GetById(int id)
        {
            var puesto = await _puestoService.GetPuestoByIdAsync(id);
            if (puesto == null)
                return NotFound();

            return Ok(puesto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearPuestoDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _puestoService.CreatePuestoAsync(dto);

            if (id == 0)
                return BadRequest("No se pudo crear el puesto.");

            return CreatedAtAction(nameof(GetById), new { id }, null);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarPuestoDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.IdPuesto)
                return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var updated = await _puestoService.UpdatePuestoAsync(dto);

            if (!updated)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _puestoService.DeletePuestoAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}