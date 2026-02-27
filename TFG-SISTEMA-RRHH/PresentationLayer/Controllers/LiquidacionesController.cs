using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class LiquidacionesController : ControllerBase
    {
        private readonly ILiquidacionesManager _manager;
        private readonly ILogger<LiquidacionesController> _logger;

        public LiquidacionesController(ILiquidacionesManager manager, ILogger<LiquidacionesController> logger)
        {
            _manager = manager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> CalcularSalarioPromedio(int idEmpleado)
        {
            try
            {
                var salarioPromedio = await _manager.CalcularSalarioPromedio(idEmpleado);
                return Ok(salarioPromedio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular el salario promedio para el empleado con ID {IdEmpleado}", idEmpleado);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al calcular el salario promedio.");
            }
        }
    }
}