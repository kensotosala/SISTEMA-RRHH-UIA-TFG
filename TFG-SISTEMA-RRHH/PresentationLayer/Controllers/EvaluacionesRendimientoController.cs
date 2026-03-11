using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class EvaluacionesRendimientoController : ControllerBase
    {
        private readonly IEvaluacionRendimientoManager _manager;
        private readonly ILogger<EvaluacionesRendimientoController> _logger;

        public EvaluacionesRendimientoController(
            IEvaluacionRendimientoManager manager,
            ILogger<EvaluacionesRendimientoController> logger)
        {
            _manager = manager;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _manager.GetAllAsync();
            return result.Exitoso ? Ok(result) : StatusCode(500, result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var result = await _manager.GetByIdAsync(id);

            if (!result.Exitoso)
                return result.Datos is null ? NotFound(result) : StatusCode(500, result);

            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateEvaluacionDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _manager.CreateAsync(dto);

            if (!result.Exitoso)
                return result.Errores.Any()
                    ? BadRequest(result)
                    : StatusCode(500, result);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Datos!.IdEvaluacion },
                result);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateEvaluacionDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _manager.UpdateAsync(id, dto);

            if (!result.Exitoso)
            {
                if (result.Errores.Any()) return BadRequest(result);
                if (result.Mensaje.Contains("No se encontró")) return NotFound(result);
                return StatusCode(500, result);
            }

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var result = await _manager.DeleteAsync(id);

            if (!result.Exitoso)
            {
                if (result.Mensaje.Contains("No se encontró"))
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPatch("aprobar/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Aprobar([FromRoute] int id)
        {
            var result = await _manager.AproveAsync(id);

            if (!result.Exitoso)
            {
                if (result.Mensaje.Contains("No se encontró"))
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}