using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
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

        [HttpGet("calcular-salario-promedio")]
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

        [HttpGet("calcular-preaviso")]
        public async Task<IActionResult> CalcularPreaviso(int idEmpleado, DateOnly fechaSalida)
        {
            try
            {
                var resultadoPreaviso = await _manager.CalcularPreaviso(idEmpleado, fechaSalida);
                return Ok(resultadoPreaviso);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular el preaviso para el empleado con ID {IdEmpleado}", idEmpleado);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al calcular el preaviso.");
            }
        }

        [HttpGet("calcular-auxilio-cesantia")]

        public async Task<IActionResult> CalcularAuxilioCesantia(int idEmpleado, DateOnly fechaSalida)
        {
            try
            {
                var resultadoAuxilioCesantia = await _manager.CalcularAuxilioCesantia(idEmpleado, fechaSalida);
                return Ok(resultadoAuxilioCesantia);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular el auxilio de cesantía para el empleado con ID {IdEmpleado}", idEmpleado);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al calcular el auxilio de cesantía.");
            }
        }

        [HttpGet("calcular-vacaciones-proporcionales")]
        public async Task<IActionResult> CalcularVacacionesProporcionales(int idEmpleado, DateOnly fechaSalida)
        {
            try
            {
                var resultadoVacacionesProporcionales = await _manager.CalcularVacacionesProporcionales(idEmpleado, fechaSalida);
                return Ok(resultadoVacacionesProporcionales);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular las vacaciones proporcionales para el empleado con ID {IdEmpleado}", idEmpleado);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al calcular las vacaciones proporcionales.");
            }
        }

        [HttpGet("calcular-aguinaldo-proporcional")]
        public async Task<IActionResult> CalcularAguinaldoProporcional(int idEmpleado, DateOnly fechaSalida)
        {
            try
            {
                var resultadoAguinaldoProporcional = await _manager.CalcularAguinaldoProporcional(idEmpleado, fechaSalida);
                return Ok(resultadoAguinaldoProporcional);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular el aguinaldo proporcional para el empleado con ID {IdEmpleado}", idEmpleado);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al calcular el aguinaldo proporcional.");
            }
        }

        [HttpGet("calcular-liquidacion")]
        public async Task<IActionResult> CalcularLiquidacion(int idEmpleado, DateOnly fechaSalida)
        {
            try
            {
                var liquidacion = await _manager.CalcularLiquidacion(idEmpleado, fechaSalida);
                return Ok(liquidacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular la liquidación para el empleado con ID {IdEmpleado}", idEmpleado);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al calcular la liquidación.");
            }
        }

        /*
         * Endpoints CRUD para liquidaciones
         */

        [HttpGet]
        public async Task<IActionResult> ListarLiquidaciones()
        {
            try
            {
                var liquidaciones = await _manager.ListarLiquidaciones();
                return Ok(liquidaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar las liquidaciones");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al listar las liquidaciones.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearLiquidacion([FromBody] CrearLiquidacionRequest request) 
        {
            try
            {
                var liquidacion = await _manager.CrearLiquidacion(
                    request.EmpleadoId,
                    DateOnly.FromDateTime(request.FechaSalida), 
                    request.MotivoLiquidacion,
                    request.PreavisoEntregado
                );
                return Ok(liquidacion);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la liquidación para el empleado con ID {IdEmpleado}", request.EmpleadoId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al crear la liquidación.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerLiquidacionPorId(int id)
        {
            try
            {
                var liquidacion = await _manager.ObtenerLiquidacionPorId(id);
                if (liquidacion == null)
                    return NotFound($"No se encontró una liquidación con ID {id}");
                return Ok(liquidacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la liquidación con ID {IdLiquidacion}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al obtener la liquidación.");
            }
        }

        [HttpPatch("{id}/anular")]
        public async Task<IActionResult> AnularLiquidacion(int id)
        {
            try
            {
                var resultado = await _manager.AnularLiquidacion(id);
                return resultado.Exitoso ? Ok(resultado) : BadRequest(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al anular la liquidación con ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al anular la liquidación.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ModificarLiquidacion(int id, [FromBody] ModificarLiquidacionRequest request)
        {
            try
            {
                var liquidacionExistente = await _manager.ObtenerLiquidacionPorId(id);
                if (liquidacionExistente == null)
                    return NotFound($"No se encontró una liquidación con ID {id}");

                liquidacionExistente.VacacionesPendientes = request.MontoVacaciones;
                liquidacionExistente.AguinaldoProporcional = request.MontoAguinaldo;
                liquidacionExistente.Indemnizacion = request.MontoCesantia;
                liquidacionExistente.MontoPreaviso = request.MontoPreaviso;
                liquidacionExistente.TotalLiquidacion = request.MontoPreaviso
                                                           + request.MontoVacaciones
                                                           + request.MontoAguinaldo
                                                           + request.MontoCesantia;
                liquidacionExistente.Estado = "CALCULADA";

                var resultado = await _manager.ModificarLiquidacion(liquidacionExistente);
                return resultado.Exitoso ? Ok(resultado) : BadRequest(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al modificar la liquidación con ID {Id}", id);
                return StatusCode(500, "Error al modificar la liquidación.");
            }
        }
    }
}