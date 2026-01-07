using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowFrontend")]
    public class AsistenciaController : ControllerBase
    {
        private readonly IAsistenciaManager _manager;

        public AsistenciaController(IAsistenciaManager manager)
        {
            _manager = manager;
        }

        // POST api/asistencia/marcar
        [HttpPost("marcar")]
        [EnableCors("AllowFrontend")]
        public async Task<IActionResult> Marcar([FromBody] MarcarAsistenciaRequest request)
        {
            var response = await _manager.MarcarAsistenciaAsync(request.EmpleadoId);

            // Siempre devolver la respuesta, aunque no haya sido exitosa
            return Ok(response);
        }

        // GET api/asistencia/estado/{empleadoId}
        [HttpGet("estado/{empleadoId}")]
        [EnableCors("AllowFrontend")]
        public async Task<IActionResult> ObtenerEstado(int empleadoId)
        {
            var estado = await _manager.ObtenerEstadoAsistenciaAsync(empleadoId);
            return Ok(estado);
        }
    }
}