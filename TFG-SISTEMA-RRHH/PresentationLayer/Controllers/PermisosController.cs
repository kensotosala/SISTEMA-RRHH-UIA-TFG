using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PermisosController : ControllerBase
    {
        private readonly IPermisosManager _permisosManager;

        public PermisosController(IPermisosManager permisosManager)
        {
            _permisosManager = permisosManager;
        }

        [HttpPut("{id}", Name = "ActualizarPermiso")]
        public async Task<IActionResult> ActualizarPermiso(int id, [FromBody] ActualizarPermisoDTO dto)
        {
            var existePermiso = await _permisosManager.ListarPermisoByIdAsync(id);
            if (existePermiso == null)
                return NotFound();

            await _permisosManager.ActualizarPermisoAsync(id, dto);

            return NoContent();
        }

        [HttpPost(Name = "CrearPermiso")]
        public async Task<ActionResult<CrearPermisoDto>> CrearPermiso([FromBody] CrearPermisoDto dto)
        {
            var permiso = await _permisosManager.CrearPermisoAsync(dto);
            return Ok(permiso);
        }

        [HttpDelete("{id}", Name = "EliminarPermiso")]
        public async Task<ActionResult<ListarPermisoByIdDTO>> EliminarPermiso(int id)
        {
            var existePermiso = await _permisosManager.ListarPermisoByIdAsync(id);

            if (existePermiso == null)
                return NotFound();

            var permiso = await _permisosManager.EliminarPermisoAsync(id);

            return Ok(permiso);
        }

        [HttpGet(Name = "ListarTodosLosPermisos")]
        public async Task<ActionResult<IEnumerable<ListarPermisosDTO>>> ListarTodosLosPermisos()
        {
            var permisos = await _permisosManager.ListarPermisosAsync();
            return Ok(permisos);
        }

        [HttpGet("{id}", Name = "ObtenerPermisoPorId")]
        public async Task<ActionResult<ListarPermisoByIdDTO>> ObtenerPermisoPorId(int id)
        {
            var permiso = await _permisosManager.ListarPermisoByIdAsync(id);

            if (permiso == null)
                return NotFound();

            return Ok(permiso);
        }
    }
}