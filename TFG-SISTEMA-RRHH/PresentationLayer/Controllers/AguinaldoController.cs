using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AguinaldoController : ControllerBase
    {
        private readonly IAguinaldoManager _aguinaldoManager;

        public AguinaldoController(IAguinaldoManager aguinaldoManager)
        {
            _aguinaldoManager = aguinaldoManager ?? throw new ArgumentNullException(nameof(aguinaldoManager));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> AnularAguinaldo(int id)
        {
            try
            {
                var resultado = await _aguinaldoManager.AnularAguinaldoAsync(id);

                if (!resultado)
                    return NotFound(new { mensaje = "Aguinaldo no encontrado" });

                return Ok(new { mensaje = "Aguinaldo anulado exitosamente" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al anular aguinaldo", error = ex.Message });
            }
        }

        [HttpPost("calcular")]
        public async Task<IActionResult> CalcularAguinaldo([FromBody] CalcularAguinaldoDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { mensaje = "Datos inválidos", errores = ModelState });

                var resultado = await _aguinaldoManager.CalcularAguinaldoEmpleadoAsync(dto);

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { id = resultado.IdAguinaldo },
                    new { mensaje = "Aguinaldo calculado y registrado exitosamente", datos = resultado });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al calcular aguinaldo", error = ex.Message });
            }
        }

        [HttpPost("calcular-hasta-hoy")]
        public async Task<IActionResult> CalcularAguinaldoHastaHoy()
        {
            var hoy = DateTime.Now;

            var anio = hoy.Month == 12 ? hoy.Year + 1 : hoy.Year;

            var dto = new CalcularAguinaldoMasivoDTO
            {
                Anio = anio,
                FechaCorte = hoy
            };

            var (registrados, errores) = await _aguinaldoManager.CalcularAguinaldoMasivoAsync(dto);

            return Ok(new
            {
                registrados,
                errores
            });
        }

        [HttpPost("calcular-masivo")]
        public async Task<IActionResult> CalcularAguinaldoMasivo([FromBody] CalcularAguinaldoMasivoDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { mensaje = "Datos inválidos", errores = ModelState });

                var (registrados, errores) = await _aguinaldoManager.CalcularAguinaldoMasivoAsync(dto);

                return Ok(new
                {
                    mensaje = $"Se calcularon y registraron {registrados.Count} aguinaldos",
                    exitosos = registrados.Count,
                    fallidos = errores.Count,
                    datos = registrados,
                    errores = errores
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al calcular aguinaldos", error = ex.Message });
            }
        }

        [HttpPost("calcular-masivo-v2")]
        public async Task<IActionResult> CalcularAguinaldoMasivoV2()
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { mensaje = "Datos inválidos", errores = ModelState });

                var (registrados, errores) = await _aguinaldoManager.CalcularAguinaldoMasivoV2Async();

                return Ok(new
                {
                    mensaje = $"Se calcularon y registraron {registrados} aguinaldos",
                    exitosos = registrados,
                    fallidos = errores.Count,
                    datos = registrados,
                    errores = errores
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al calcular aguinaldos", error = ex.Message });
            }
        }

        [HttpPost("calcular-v2")]
        public async Task<IActionResult> CalcularAguinaldoPresenteAnio(int idEmpleado)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { mensaje = "Datos inválidos", errores = ModelState });

                var resultado = await _aguinaldoManager.CalcularAguinaldoPresenteAnio(idEmpleado);

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { id = resultado.IdAguinaldo },
                    new { mensaje = "Aguinaldo calculado y registrado exitosamente", datos = resultado });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al calcular aguinaldo", error = ex.Message });
            }
        }

        [HttpGet("anio/{anio}")]
        public async Task<IActionResult> ObtenerPorAnio(int anio)
        {
            try
            {
                var aguinaldos = await _aguinaldoManager.ObtenerPorAnioAsync(anio);
                return Ok(new { mensaje = $"Aguinaldos del año {anio} obtenidos", datos = aguinaldos });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener aguinaldos", error = ex.Message });
            }
        }

        [HttpGet("empleado/{empleadoId}")]
        public async Task<IActionResult> ObtenerPorEmpleado(int empleadoId)
        {
            try
            {
                var aguinaldos = await _aguinaldoManager.ObtenerPorEmpleadoAsync(empleadoId);
                return Ok(new { mensaje = "Aguinaldos del empleado obtenidos", datos = aguinaldos });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener aguinaldos", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var aguinaldo = await _aguinaldoManager.ObtenerPorIdAsync(id);

                if (aguinaldo == null)
                    return NotFound(new { mensaje = $"Aguinaldo {id} no encontrado" });

                return Ok(new { mensaje = "Aguinaldo encontrado", datos = aguinaldo });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener aguinaldo", error = ex.Message });
            }
        }

        [HttpGet("resumen/{anio}")]
        public async Task<IActionResult> ObtenerResumen(int anio)
        {
            try
            {
                var resumen = await _aguinaldoManager.ObtenerResumenPorAnioAsync(anio);
                return Ok(new { mensaje = $"Resumen de aguinaldos {anio}", datos = resumen });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener resumen", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            try
            {
                var aguinaldos = await _aguinaldoManager.ObtenerTodosAsync();
                return Ok(new { mensaje = "Aguinaldos obtenidos exitosamente", datos = aguinaldos });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener aguinaldos", error = ex.Message });
            }
        }
        [HttpPut("{id}/pagar")]
        public async Task<IActionResult> PagarAguinaldo(int id, [FromBody] PagarAguinaldoDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { mensaje = "Datos inválidos", errores = ModelState });

                var resultado = await _aguinaldoManager.PagarAguinaldoAsync(id, dto.FechaPago);

                if (!resultado)
                    return BadRequest(new { mensaje = "No se pudo pagar el aguinaldo" });

                return Ok(new { mensaje = "Aguinaldo pagado exitosamente" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al pagar aguinaldo", error = ex.Message });
            }
        }

        [HttpPost("pagar-masivo")]
        public async Task<IActionResult> PagarAguinaldosMasivo([FromBody] PagarAguinaldosMasivoRequest request)
        {
            try
            {
                if (!ModelState.IsValid || request.IdsAguinaldos == null || !request.IdsAguinaldos.Any())
                    return BadRequest(new { mensaje = "Debe proporcionar al menos un ID de aguinaldo" });

                var (exitosos, fallidos, errores) = await _aguinaldoManager.PagarAguinaldosMasivoAsync(
                    request.IdsAguinaldos,
                    request.FechaPago);

                return Ok(new
                {
                    mensaje = $"Proceso completado: {exitosos} exitosos, {fallidos} fallidos",
                    exitosos,
                    fallidos,
                    errores
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al pagar aguinaldos", error = ex.Message });
            }
        }
        public class PagarAguinaldosMasivoRequest
        {
            public DateTime FechaPago { get; set; }
            public List<int> IdsAguinaldos { get; set; } = new();
        }
    }
}