using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
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
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argumento inválido al generar nómina quincenal");
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar nómina quincenal");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

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

        [HttpGet("quincena/{quincena}/mes/{mes}/anio/{anio}")]
        public async Task<ActionResult<List<NominaDTO>>> ObtenerNominasQuincena(
            int quincena, int mes, int anio)
        {
            try
            {
                if (quincena != 1 && quincena != 2)
                    return BadRequest(new { mensaje = "La quincena debe ser 1 o 2" });

                if (mes < 1 || mes > 12)
                    return BadRequest(new { mensaje = "El mes debe estar entre 1 y 12" });

                if (anio < 2000 || anio > DateTime.Now.Year + 1)
                    return BadRequest(new { mensaje = "El año no es válido" });

                var nominas = await _nominaManager.ObtenerNominasQuincenaAsync(quincena, mes, anio);
                return Ok(nominas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener nóminas de quincena");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}/aprobar")]
        public async Task<ActionResult> AprobarNomina(int id)
        {
            try
            {
                var resultado = await _nominaManager.AprobarNominaAsync(id);

                return resultado switch
                {
                    BusinessLogicLayer.Shared.AprobarNominaResultado.Aprobada => Ok(new { mensaje = "Nómina aprobada correctamente" }),
                    BusinessLogicLayer.Shared.AprobarNominaResultado.NoEncontrada => NotFound(new { mensaje = "Nómina no encontrada" }),
                    BusinessLogicLayer.Shared.AprobarNominaResultado.EstadoInvalido => BadRequest(new { mensaje = "Solo se pueden aprobar nóminas en estado PENDIENTE" }),
                    _ => StatusCode(500, new { mensaje = "Error desconocido" })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aprobar nómina {Id}", id);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}/pagar")]
        public async Task<ActionResult> PagarNomina(int id)
        {
            try
            {
                var resultado = await _nominaManager.PagarNominaAsync(id);

                return resultado switch
                {
                    BusinessLogicLayer.Shared.PagarNominaResultado.Pagada => Ok(new { mensaje = "Nómina pagada correctamente" }),
                    BusinessLogicLayer.Shared.PagarNominaResultado.NoEncontrada => NotFound(new { mensaje = "Nómina no encontrada" }),
                    BusinessLogicLayer.Shared.PagarNominaResultado.NoAprobada => BadRequest(new { mensaje = "La nómina debe estar aprobada para poder pagarla" }),
                    _ => StatusCode(500, new { mensaje = "Error desconocido" })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al pagar nómina {Id}", id);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}/anular")]
        public async Task<ActionResult> AnularNomina(int id)
        {
            try
            {
                var resultado = await _nominaManager.AnularNominaAsync(id);

                return resultado switch
                {
                    BusinessLogicLayer.Shared.AnularNominaResultado.Anulada => Ok(new { mensaje = "Nómina anulada correctamente" }),
                    BusinessLogicLayer.Shared.AnularNominaResultado.NoEncontrada => NotFound(new { mensaje = "Nómina no encontrada" }),
                    BusinessLogicLayer.Shared.AnularNominaResultado.NoPuedeAnularse => BadRequest(new { mensaje = "No se puede anular una nómina pagada" }),
                    _ => StatusCode(500, new { mensaje = "Error desconocido" })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al anular nómina {Id}", id);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

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

        [HttpGet("parcial/hoy")]
        public async Task<ActionResult<NominaParcialDTO>> ObtenerNominaParcialHoy()
        {
            try
            {
                var resultado = await _nominaManager.CalcularNominaParcialHoyAsync();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular nómina parcial");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }
    }
}