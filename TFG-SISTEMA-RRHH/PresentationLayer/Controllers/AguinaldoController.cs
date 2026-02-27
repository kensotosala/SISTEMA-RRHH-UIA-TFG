
using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    /// <summary>
    /// Controlador de Aguinaldos según legislación de Costa Rica
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]

    public class AguinaldoController : ControllerBase
    {
        private readonly IAguinaldoManager _aguinaldoManager;

        public AguinaldoController(IAguinaldoManager aguinaldoManager)
        {
            _aguinaldoManager = aguinaldoManager ?? throw new ArgumentNullException(nameof(aguinaldoManager));
        }

        /// <summary>
        /// Obtiene todos los aguinaldos
        /// </summary>
        [HttpGet]
        // [Authorize(Roles = "Administrador,Recursos Humanos")]
        public async Task<IActionResult> ObtenerTodos()
        {
            try
            {
                var aguinaldos = await _aguinaldoManager.ObtenerTodosAsync();

                return Ok(new
                {
                    mensaje = "Aguinaldos obtenidos exitosamente",
                    datos = aguinaldos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener aguinaldos", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un aguinaldo por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var aguinaldo = await _aguinaldoManager.ObtenerPorIdAsync(id);

                if (aguinaldo == null)
                    return NotFound(new { mensaje = $"Aguinaldo {id} no encontrado" });

                return Ok(new
                {
                    mensaje = "Aguinaldo encontrado",
                    datos = aguinaldo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener aguinaldo", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene aguinaldos por año
        /// </summary>
        [HttpGet("anio/{anio}")]
        // [Authorize(Roles = "Administrador,Recursos Humanos")]
        public async Task<IActionResult> ObtenerPorAnio(int anio)
        {
            try
            {
                var aguinaldos = await _aguinaldoManager.ObtenerPorAnioAsync(anio);

                return Ok(new
                {
                    mensaje = $"Aguinaldos del año {anio} obtenidos",
                    datos = aguinaldos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener aguinaldos", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene aguinaldos de un empleado
        /// </summary>
        [HttpGet("empleado/{empleadoId}")]
        public async Task<IActionResult> ObtenerPorEmpleado(int empleadoId)
        {
            try
            {
                //// Verificar que el usuario solo pueda ver sus propios aguinaldos
                //var userEmployeeId = User.FindFirst("EmployeeId")?.Value;
                //var userRole = User.FindFirst("Roles")?.Value;

                //if (userRole != "Administrador" &&
                //    userRole != "Recursos Humanos" &&
                //    userEmployeeId != empleadoId.ToString())
                //{
                //    return Forbid();
                //}

                var aguinaldos = await _aguinaldoManager.ObtenerPorEmpleadoAsync(empleadoId);

                return Ok(new
                {
                    mensaje = "Aguinaldos del empleado obtenidos",
                    datos = aguinaldos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener aguinaldos", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene resumen de aguinaldos por año
        /// </summary>
        [HttpGet("resumen/{anio}")]
        // [Authorize(Roles = "Administrador,Recursos Humanos")]
        public async Task<IActionResult> ObtenerResumen(int anio)
        {
            try
            {
                var resumen = await _aguinaldoManager.ObtenerResumenPorAnioAsync(anio);

                return Ok(new
                {
                    mensaje = $"Resumen de aguinaldos {anio}",
                    datos = resumen
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener resumen", error = ex.Message });
            }
        }

        /// <summary>
        /// Calcula aguinaldo para un empleado
        /// </summary>
        [HttpPost("calcular")]
        // [Authorize(Roles = "Administrador,Recursos Humanos")]
        public async Task<IActionResult> CalcularAguinaldo([FromBody] CalcularAguinaldoDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { mensaje = "Datos inválidos", errores = ModelState });

                var resultado = await _aguinaldoManager.CalcularAguinaldoEmpleadoAsync(dto);

                return Ok(new
                {
                    mensaje = "Aguinaldo calculado exitosamente",
                    datos = resultado
                });
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

        /// <summary>
        /// Calcula aguinaldos para todos los empleados activos
        /// </summary>
        [HttpPost("calcular-masivo")]
        // [Authorize(Roles = "Administrador,Recursos Humanos")]
        public async Task<IActionResult> CalcularAguinaldoMasivo([FromBody] CalcularAguinaldoMasivoDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { mensaje = "Datos inválidos", errores = ModelState });

                var resultados = await _aguinaldoManager.CalcularAguinaldoMasivoAsync(dto);

                return Ok(new
                {
                    mensaje = $"Se calcularon {resultados.Count} aguinaldos",
                    datos = resultados
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al calcular aguinaldos", error = ex.Message });
            }
        }

        /// <summary>
        /// Registra aguinaldos calculados en la base de datos
        /// </summary>
        [HttpPost("registrar")]
        // [Authorize(Roles = "Administrador,Recursos Humanos")]
        public async Task<IActionResult> RegistrarAguinaldos([FromBody] RegistrarAguinaldosRequest request)
        {
            try
            {
                if (!ModelState.IsValid || request.Calculos == null || !request.Calculos.Any())
                    return BadRequest(new { mensaje = "Debe proporcionar al menos un cálculo" });

                var registrados = new List<AguinaldoDTO>();
                var errores = new List<string>();

                foreach (var calculo in request.Calculos)
                {
                    try
                    {
                        var aguinaldo = await _aguinaldoManager.RegistrarAguinaldoAsync(
                            calculo,
                            request.Anio);

                        registrados.Add(aguinaldo);
                    }
                    catch (Exception ex)
                    {
                        errores.Add($"{calculo.NombreEmpleado}: {ex.Message}");
                    }
                }

                return Ok(new
                {
                    mensaje = $"Se registraron {registrados.Count} aguinaldos",
                    exitosos = registrados.Count,
                    fallidos = errores.Count,
                    datos = registrados,
                    errores = errores
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al registrar aguinaldos", error = ex.Message });
            }
        }

        /// <summary>
        /// Paga un aguinaldo
        /// </summary>
        [HttpPut("{id}/pagar")]
        // [Authorize(Roles = "Administrador,Recursos Humanos")]
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

        /// <summary>
        /// Paga múltiples aguinaldos
        /// </summary>
        [HttpPost("pagar-masivo")]
        // [Authorize(Roles = "Administrador,Recursos Humanos")]
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
                    exitosos = exitosos,
                    fallidos = fallidos,
                    errores = errores
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al pagar aguinaldos", error = ex.Message });
            }
        }

        /// <summary>
        /// Anula un aguinaldo
        /// </summary>
        [HttpDelete("{id}")]
        // [Authorize(Roles = "Administrador")]
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
    }

    #region Request Models

    public class RegistrarAguinaldosRequest
    {
        public int Anio { get; set; }
        public List<ResultadoCalculoAguinaldoDTO> Calculos { get; set; } = new();
    }

    public class PagarAguinaldosMasivoRequest
    {
        public List<int> IdsAguinaldos { get; set; } = new();
        public DateTime FechaPago { get; set; }
    }

    #endregion Request Models
}