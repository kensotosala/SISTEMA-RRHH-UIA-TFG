using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MetricasRendimientoController : ControllerBase
    {
        private readonly IMetricasRendimientoManager _manager;

        public MetricasRendimientoController(IMetricasRendimientoManager manager)
        {
            _manager = manager;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _manager.GetMetricasRendimientoAsync();
            return result.Exitoso ? Ok(result) : StatusCode(500, result);
        }
    }
}