using ApplicationLayer.Interfaces;
using BusinessLogicLayer.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartamentosController : ControllerBase
    {
        private readonly IDepartamentoService _service;

        public DepartamentosController(IDepartamentoService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var departamentos = await _service.GetAllAsync();

            if (departamentos == null || !departamentos.Any())
            {
                return Ok(new
                {
                    message = "No hay registros para mostrar",
                    data = new List<object>()
                });
            }

            return Ok(departamentos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var departamento = await _service.GetByIdAsync(id);

            if (departamento is null)
                return NotFound($"Departamento con ID {id} no encontrado.");

            return Ok(departamento);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DepartamentoDTO dto)
        {
            if (dto is null)
                return BadRequest("El cuerpo de la solicitud no puede ser nulo.");

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var creado = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = creado.IdDepartamento },
                creado
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] DepartamentoDTO dto)
        {
            if (dto is null)
                return BadRequest("El cuerpo de la solicitud no puede ser nulo.");

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            await _service.UpdateAsync(dto);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var departamento = await _service.GetByIdAsync(id);

            if (departamento is null)
                return NotFound($"Departamento con ID {id} no encontrado.");

            await _service.DeleteAsync(id);

            return NoContent();
        }
    }
}