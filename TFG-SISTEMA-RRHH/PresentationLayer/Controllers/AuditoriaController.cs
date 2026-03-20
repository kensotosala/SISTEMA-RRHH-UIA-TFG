using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuditoriaController : ControllerBase
    {
        private readonly IAuditoriaService _auditoriaManager;
        private readonly ILogger<AuditoriaController> _logger;

        public AuditoriaController(IAuditoriaService auditoriaManager, ILogger<AuditoriaController> logger)
        {
            _auditoriaManager = auditoriaManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> ListarLogs()
        {
            try
            {
                var logs = await _auditoriaManager.ListarAsync();
                return Ok(new { mensaje = "Logs obtenidos exitosamente", datos = logs });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener logs", error = ex.Message });
            }
        }
    }
}