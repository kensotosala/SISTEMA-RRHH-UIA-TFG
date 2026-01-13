using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Shared;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HorasExtrasController : ControllerBase
    {
        private readonly IHorasExtrasManager _horasExtrasManager;
        private readonly ILogger<HorasExtrasController> _logger;

        public HorasExtrasController(
            IHorasExtrasManager horasExtrasManager,
            ILogger<HorasExtrasController> logger)
        {
            _horasExtrasManager = horasExtrasManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HoraExtraDTO>>> GetAll()
        {
            try
            {
                var horasExtras = await _horasExtrasManager.GetAllAsync();
                return Ok(horasExtras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las horas extras");
                return StatusCode(500, new { message = "Error al obtener horas extras" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HoraExtraDTO>> GetById(int id)
        {
            try
            {
                var horaExtra = await _horasExtrasManager.GetByIdAsync(id);
                if (horaExtra == null)
                {
                    return NotFound(new { message = "Registro de hora extra no encontrado" });
                }

                return Ok(horaExtra);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener hora extra por ID: {Id}", id);
                return StatusCode(500, new { message = "Error al obtener hora extra" });
            }
        }

        [HttpPost("buscar")]
        public async Task<ActionResult<IEnumerable<HoraExtraDTO>>> BuscarPorFiltros(
            [FromBody] FiltrosHorasExtrasDTO filtros)
        {
            try
            {
                var horasExtras = await _horasExtrasManager.GetByFiltrosAsync(filtros);
                return Ok(horasExtras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar horas extras con filtros");
                return StatusCode(500, new { message = "Error al buscar horas extras" });
            }
        }

        [HttpGet("empleado/{empleadoId}")]
        public async Task<ActionResult<IEnumerable<HoraExtraDTO>>> GetByEmpleado(int empleadoId)
        {
            try
            {
                var horasExtras = await _horasExtrasManager.GetByEmpleadoAsync(empleadoId);
                return Ok(horasExtras);
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Error de negocio al obtener horas extras del empleado");
                return BadRequest(new { message = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener horas extras del empleado: {EmpleadoId}", empleadoId);
                return StatusCode(500, new { message = "Error al obtener horas extras" });
            }
        }

        [HttpGet("pendientes/jefe/{jefeId}")]
        public async Task<ActionResult<IEnumerable<HoraExtraDTO>>> GetPendientesByJefe(int jefeId)
        {
            try
            {
                var horasExtras = await _horasExtrasManager.GetPendientesByJefeAsync(jefeId);
                return Ok(horasExtras);
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Error de negocio al obtener pendientes del jefe");
                return BadRequest(new { message = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pendientes del jefe: {JefeId}", jefeId);
                return StatusCode(500, new { message = "Error al obtener pendientes" });
            }
        }

        /// <summary>
        /// Crea una nueva solicitud de hora extra
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<HoraExtraDTO>> Create([FromBody] CrearHoraExtraDTO dto)
        {
            try
            {
                // Log para debugging
                _logger.LogInformation("📥 Recibiendo solicitud: {@DTO}", dto);

                // Validación explícita del ModelState
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    _logger.LogWarning("❌ ModelState inválido: {@Errors}", errors);

                    return BadRequest(new
                    {
                        message = "Datos de entrada inválidos",
                        errors = errors
                    });
                }

                var horaExtra = await _horasExtrasManager.CreateAsync(dto);

                _logger.LogInformation("✅ Hora extra creada: {Id}", horaExtra.IdHoraExtra);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = horaExtra.IdHoraExtra },
                    horaExtra);
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "⚠️ Error de negocio al crear hora extra");
                return BadRequest(new { message = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error inesperado al crear hora extra");
                return StatusCode(500, new
                {
                    message = "Error interno del servidor al crear hora extra",
                    detail = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] ActualizarHoraExtraDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var resultado = await _horasExtrasManager.UpdateAsync(id, dto);
                if (!resultado)
                {
                    return NotFound(new { message = "Registro de hora extra no encontrado" });
                }

                return Ok(new { message = "Hora extra actualizada correctamente" });
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Error de negocio al actualizar hora extra");
                return BadRequest(new { message = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar hora extra con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar hora extra" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var resultado = await _horasExtrasManager.DeleteAsync(id);
                if (!resultado)
                {
                    return NotFound(new { message = "Registro de hora extra no encontrado" });
                }

                return Ok(new { message = "Hora extra eliminada correctamente" });
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Error de negocio al eliminar hora extra");
                return BadRequest(new { message = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar hora extra con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar hora extra" });
            }
        }

        [HttpPatch("{id}/aprobar-rechazar")]
        public async Task<ActionResult> AprobarRechazar(
            int id,
            [FromBody] AprobarRechazarHoraExtraDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var resultado = await _horasExtrasManager.AprobarRechazarAsync(id, dto);
                if (!resultado)
                {
                    return NotFound(new { message = "Registro de hora extra no encontrado" });
                }

                var accion = dto.EstadoSolicitud == "APROBADA" ? "aprobada" : "rechazada";
                return Ok(new { message = $"Solicitud {accion} correctamente" });
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Error de negocio al aprobar/rechazar hora extra");
                return BadRequest(new { message = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aprobar/rechazar hora extra con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al procesar solicitud" });
            }
        }

        [HttpGet("reporte/{empleadoId}")]
        public async Task<ActionResult<ReporteHorasExtrasDTO>> GetReporte(
            int empleadoId,
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                var reporte = await _horasExtrasManager.GetReporteEmpleadoAsync(
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
    }
}