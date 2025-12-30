using ApplicationLayer.Interfaces;
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
    }
}