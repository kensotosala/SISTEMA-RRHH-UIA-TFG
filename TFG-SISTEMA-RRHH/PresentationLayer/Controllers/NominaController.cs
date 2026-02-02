using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]

    public class NominaController : ControllerBase
    {
        private readonly INominaManager _nominaManager;
        private readonly ILogger<NominaController> _logger;

        public NominaController(INominaManager nominaManager, ILogger<NominaController> logger)
        {
            _nominaManager = nominaManager;
            _logger = logger;
        }

        /// <summary>
        /// Generar nómina quincenal para todos los empleados activos
        /// </summary>
        [HttpPost("generar")]
        public async Task<ActionResult<List<DetalleNominaDTO>>> GenerarNominaQuincenal(
            [FromBody] GenerarNominaQuincenalDTO dto)
        {
            try
            {
                var detalles = await _nominaManager.GenerarNominaQuincenalAsync(dto);

                _logger.LogInformation(
                    "Nómina generada: Quincena {Quincena}, Mes {Mes}, Año {Anio}, Total empleados: {Total}",
                    dto.Quincena, dto.Mes, dto.Anio, detalles.Count);

                return Ok(detalles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar nómina quincenal");
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Obtener nómina por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<NominaDTO>> ObtenerNominaPorId(int id)
        {
            try
            {
                var nomina = await _nominaManager.ObtenerNominaPorIdAsync(id);

                if (nomina == null)
                    return NotFound(new { mensaje = "Nómina no encontrada" });

                return Ok(nomina);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener nómina {Id}", id);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Listar todas las nóminas
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<NominaDTO>>> ListarNominas()
        {
            try
            {
                var nominas = await _nominaManager.ListarNominasAsync();
                return Ok(nominas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar nóminas");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener nóminas de un empleado específico
        /// </summary>
        [HttpGet("empleado/{empleadoId}")]
        public async Task<ActionResult<List<NominaDTO>>> ObtenerNominasPorEmpleado(int empleadoId)
        {
            try
            {
                var nominas = await _nominaManager.ObtenerNominasPorEmpleadoAsync(empleadoId);
                return Ok(nominas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener nóminas del empleado {EmpleadoId}", empleadoId);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener nóminas de una quincena específica
        /// </summary>
        [HttpGet("quincena/{quincena}/mes/{mes}/anio/{anio}")]
        public async Task<ActionResult<List<NominaDTO>>> ObtenerNominasQuincena(
            int quincena, int mes, int anio)
        {
            try
            {
                if (quincena != 1 && quincena != 2)
                    return BadRequest(new { mensaje = "La quincena debe ser 1 o 2" });

                var nominas = await _nominaManager.ObtenerNominasQuincenaAsync(quincena, mes, anio);
                return Ok(nominas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener nóminas de quincena");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Aprobar una nómina
        /// </summary>
        [HttpPut("{id}/aprobar")]
        public async Task<ActionResult> AprobarNomina(int id)
        {
            try
            {
                var resultado = await _nominaManager.AprobarNominaAsync(id);

                if (!resultado)
                    return NotFound(new { mensaje = "Nómina no encontrada" });

                return Ok(new { mensaje = "Nómina aprobada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aprobar nómina {Id}", id);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Pagar una nómina
        /// </summary>
        [HttpPut("{id}/pagar")]
        public async Task<ActionResult> PagarNomina(int id)
        {
            try
            {
                var resultado = await _nominaManager.PagarNominaAsync(id);

                if (!resultado)
                    return BadRequest(new { mensaje = "La nómina debe estar aprobada para poder pagarla" });

                return Ok(new { mensaje = "Nómina pagada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al pagar nómina {Id}", id);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Anular una nómina
        /// </summary>
        [HttpPut("{id}/anular")]
        public async Task<ActionResult> AnularNomina(int id)
        {
            try
            {
                var resultado = await _nominaManager.AnularNominaAsync(id);

                if (!resultado)
                    return BadRequest(new { mensaje = "No se puede anular una nómina pagada" });

                return Ok(new { mensaje = "Nómina anulada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al anular nómina {Id}", id);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener resumen de una quincena
        /// </summary>
        [HttpGet("resumen/quincena/{quincena}/mes/{mes}/anio/{anio}")]
        public async Task<ActionResult<ResumenNominaQuincenalDTO>> ObtenerResumenQuincena(
            int quincena, int mes, int anio)
        {
            try
            {
                var resumen = await _nominaManager.ObtenerResumenQuincenaAsync(quincena, mes, anio);
                return Ok(resumen);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen de quincena");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Generar planilla CCSS mensual
        /// </summary>
        [HttpGet("reportes/ccss/mes/{mes}/anio/{anio}")]
        public async Task<ActionResult<PlanillaCCSSDTO>> GenerarPlanillaCCSS(int mes, int anio)
        {
            try
            {
                var planilla = await _nominaManager.GenerarPlanillaCCSSAsync(mes, anio);
                return Ok(planilla);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar planilla CCSS");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Generar declaración D-151 (impuesto sobre la renta)
        /// </summary>
        [HttpGet("reportes/d151/mes/{mes}/anio/{anio}")]
        public async Task<ActionResult<DeclaracionD151DTO>> GenerarDeclaracionD151(int mes, int anio)
        {
            try
            {
                var declaracion = await _nominaManager.GenerarDeclaracionD151Async(mes, anio);
                return Ok(declaracion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar declaración D-151");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }
    }
}