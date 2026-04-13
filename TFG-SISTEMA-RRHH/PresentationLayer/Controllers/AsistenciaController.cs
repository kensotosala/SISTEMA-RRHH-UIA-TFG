using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Shared;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class AsistenciasController : ControllerBase
    {
        private readonly IAsistenciaManager _asistenciaManager;
        private readonly ILogger<AsistenciasController> _logger;

        public AsistenciasController(
            IAsistenciaManager asistenciaManager,
            ILogger<AsistenciasController> logger)
        {
            _asistenciaManager = asistenciaManager;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las asistencias (Administrador)
        /// </summary>
        [HttpGet]
        //[Authorize(Roles = "Administrador")]
        public async Task<ActionResult<IEnumerable<AsistenciaDTO>>> GetAll()
        {
            try
            {
                var asistencias = await _asistenciaManager.GetAllAsync();
                return Ok(asistencias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las asistencias");
                return StatusCode(500, new { message = "Error al obtener asistencias" });
            }
        }

        /// <summary>
        /// Obtiene una asistencia por ID (Administrador)
        /// </summary>
        [HttpGet("{id}")]
        //[Authorize(Roles = "Administrador")]
        public async Task<ActionResult<AsistenciaDTO>> GetById(int id)
        {
            try
            {
                var asistencia = await _asistenciaManager.GetByIdAsync(id);
                if (asistencia == null)
                {
                    return NotFound(new { message = "Registro de asistencia no encontrado" });
                }

                return Ok(asistencia);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener asistencia por ID: {Id}", id);
                return StatusCode(500, new { message = "Error al obtener asistencia" });
            }
        }

        /// <summary>
        /// Busca asistencias con filtros (Administrador)
        /// </summary>
        [HttpPost("buscar")]
        //[Authorize(Roles = "Administrador")]
        public async Task<ActionResult<IEnumerable<AsistenciaDTO>>> BuscarPorFiltros(
            [FromBody] FiltrosAsistenciaDTO filtros)
        {
            try
            {
                var asistencias = await _asistenciaManager.GetByFiltrosAsync(filtros);
                return Ok(asistencias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar asistencias con filtros");
                return StatusCode(500, new { message = "Error al buscar asistencias" });
            }
        }

        /// <summary>
        /// Crea un nuevo registro de asistencia (Administrador)
        /// </summary>
        [HttpPost]
        //[Authorize(Roles = "Administrador")]
        public async Task<ActionResult<AsistenciaDTO>> Create(
            [FromBody] CrearAsistenciaDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var asistencia = await _asistenciaManager.CreateAsync(dto);
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = asistencia.IdAsistencia },
                    asistencia);
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Error de negocio al crear asistencia");
                return BadRequest(new { message = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear asistencia");
                return StatusCode(500, new { message = "Error al crear asistencia" });
            }
        }

        /// <summary>
        /// Actualiza un registro de asistencia (Administrador)
        /// </summary>
        [HttpPut("{id}")]
        //[Authorize(Roles = "Administrador")]
        public async Task<ActionResult> Update(
            int id,
            [FromBody] ActualizarAsistenciaDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var resultado = await _asistenciaManager.UpdateAsync(id, dto);
                if (!resultado)
                {
                    return NotFound(new { message = "Registro de asistencia no encontrado" });
                }

                return Ok(new { message = "Asistencia actualizada correctamente" });
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Error de negocio al actualizar asistencia");
                return BadRequest(new { message = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar asistencia con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar asistencia" });
            }
        }

        /// <summary>
        /// Elimina un registro de asistencia (Administrador)
        /// </summary>
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Administrador")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var resultado = await _asistenciaManager.DeleteAsync(id);
                if (!resultado)
                {
                    return NotFound(new { message = "Registro de asistencia no encontrado" });
                }

                return Ok(new { message = "Asistencia eliminada correctamente" });
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Error de negocio al eliminar asistencia");
                return BadRequest(new { message = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar asistencia con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar asistencia" });
            }
        }

        /// <summary>
        /// Obtiene el reporte de asistencia de un empleado (Administrador)
        /// </summary>
        [HttpGet("reporte/{empleadoId}")]
        //[Authorize(Roles = "Administrador")]
        public async Task<ActionResult<ReporteAsistenciaDTO>> GetReporte(
            int empleadoId,
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                var reporte = await _asistenciaManager.GetReporteEmpleadoAsync(
                    empleadoId,
                    fechaInicio,
                    fechaFin);

                return Ok(reporte);
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Error de negocio al generar reporte");
                return BadRequest(new { message = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte para empleado: {EmpleadoId}", empleadoId);
                return StatusCode(500, new { message = "Error al generar reporte" });
            }
        }

        /// <summary>
        /// Permite al empleado marcar su entrada/salida
        /// </summary>
        [HttpPost("marcar")]
        public async Task<ActionResult<MarcarAsistenciaResponse>> MarcarAsistencia(
            [FromBody] MarcarAsistenciaRequest request)
        {
            try
            {
                var resultado = await _asistenciaManager.MarcarAsistenciaAsync(request.EmpleadoId);
                return Ok(resultado);
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Error de negocio al marcar asistencia");
                return BadRequest(new { message = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar asistencia");
                return StatusCode(500, new { message = "Error al marcar asistencia" });
            }
        }

        [HttpPost("v2/marcar")]
        public async Task<ActionResult<MarcarAsistenciaResponse>> MarcarAsistenciaV2(
            [FromBody] MarcarAsistenciaRequest request)
        {
            try
            {
                var resultado = await _asistenciaManager.MarcarAsistenciaAsyncV2(request.EmpleadoId);
                return Ok(resultado);
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Error de negocio al marcar asistencia");
                return BadRequest(new { message = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar asistencia");
                return StatusCode(500, new { message = "Error al marcar asistencia" });
            }
        }

        /// <summary>
        /// Obtiene el estado de asistencia del empleado para hoy
        /// </summary>
        [HttpGet("estado/{empleadoId}")]
        public async Task<ActionResult<EstadoAsistenciaDTO>> GetEstado(int empleadoId)
        {
            try
            {
                var estado = await _asistenciaManager.ObtenerEstadoAsistenciaAsync(empleadoId);
                return Ok(estado);
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Error de negocio al obtener estado");
                return BadRequest(new { message = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estado para empleado: {EmpleadoId}", empleadoId);
                return StatusCode(500, new { message = "Error al obtener estado" });
            }
        }
    }
}