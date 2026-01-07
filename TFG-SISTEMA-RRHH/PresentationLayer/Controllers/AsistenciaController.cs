using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AsistenciaController : ControllerBase
    {
        private readonly IAsistenciaManager _manager;

        public AsistenciaController(IAsistenciaManager manager)
        {
            _manager = manager;
        }

        [HttpPost("marcar")]
        public async Task<IActionResult> Marcar([FromBody] MarcarAsistenciaRequest request)
        {
            await _manager.MarcarAsistenciaAsync(request.EmpleadoId);
            return Ok();
        }
    }
}