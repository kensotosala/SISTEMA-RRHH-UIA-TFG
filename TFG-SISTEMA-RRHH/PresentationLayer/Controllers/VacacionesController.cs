using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class VacacionesController : ControllerBase
    {
        private readonly IVacacionesManager _vacacionesManager;

        public VacacionesController(IVacacionesManager vacacionesManager)
        {
            _vacacionesManager = vacacionesManager ?? throw new ArgumentNullException(nameof(vacacionesManager));
        }

        // ========================================
        // ENDPOINTS CRUD BÁSICOS
        // ========================================

        [HttpPost]
        [ProducesResponseType(typeof(ResultDTO<ListarVacacionByIdDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResultDTO<ListarVacacionByIdDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Crear([FromBody] CrearVacacionDTO dto)
        {
            // Validar ModelState (DataAnnotations)
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResultDTO<object>
                {
                    Exitoso = false,
                    Mensaje = "Datos inválidos",
                    Errores = ObtenerErroresModelState()
                });
            }

            var resultado = await _vacacionesManager.CrearSolicitudAsync(dto);

            if (!resultado.Exitoso)
            {
                return BadRequest(resultado);
            }

            // Retornar 201 Created con la ubicación del recurso
            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = resultado.Datos!.IdVacacion },
                resultado
            );
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ResultDTO<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDTO<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarVacacionDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResultDTO<object>
                {
                    Exitoso = false,
                    Mensaje = "Datos inválidos",
                    Errores = ObtenerErroresModelState()
                });
            }

            var resultado = await _vacacionesManager.ActualizarSolicitudAsync(id, dto);

            if (!resultado.Exitoso)
            {
                // Si el mensaje indica que no existe, retornar 404
                if (resultado.Mensaje.Contains("no existe"))
                {
                    return NotFound(resultado);
                }

                return BadRequest(resultado);
            }

            return Ok(resultado);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ResultDTO<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDTO<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResultDTO<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Cancelar(int id)
        {
            var resultado = await _vacacionesManager.CancelarSolicitudAsync(id);

            if (!resultado.Exitoso)
            {
                if (resultado.Mensaje.Contains("no existe"))
                {
                    return NotFound(resultado);
                }

                return BadRequest(resultado);
            }

            return Ok(resultado);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResultDTO<ListarVacacionByIdDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var resultado = await _vacacionesManager.ObtenerPorIdAsync(id);

            if (!resultado.Exitoso)
            {
                return NotFound(resultado);
            }

            return Ok(resultado);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ResultDTO<IEnumerable<ListarVacacionesDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerTodos()
        {
            var resultado = await _vacacionesManager.ObtenerTodosAsync();

            return Ok(resultado);
        }

        [HttpGet("empleado/{empleadoId}")]
        [ProducesResponseType(typeof(ResultDTO<IEnumerable<ListarVacacionesDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerPorEmpleado(int empleadoId)
        {
            var resultado = await _vacacionesManager.ObtenerPorEmpleadoAsync(empleadoId);

            return Ok(resultado);
        }

        [HttpGet("estado/{estado}")]
        [ProducesResponseType(typeof(ResultDTO<IEnumerable<ListarVacacionesDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ObtenerPorEstado(string estado)
        {
            // Validar que el estado sea válido
            var estadosValidos = new[] { "PENDIENTE", "APROBADA", "RECHAZADA", "CANCELADA" };
            if (!Array.Exists(estadosValidos, e => e.Equals(estado, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new ResultDTO<object>
                {
                    Exitoso = false,
                    Mensaje = "Estado inválido",
                    Errores = new List<string> { "Los estados válidos son: PENDIENTE, APROBADA, RECHAZADA, CANCELADA" }
                });
            }

            var resultado = await _vacacionesManager.ObtenerPorEstadoAsync(estado.ToUpper());

            return Ok(resultado);
        }

        // ========================================
        // ENDPOINTS DE APROBACIÓN/RECHAZO
        // ========================================

        [HttpPatch("{id}/aprobar")]
        [ProducesResponseType(typeof(ResultDTO<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDTO<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Aprobar(int id, [FromQuery] int jefeId)
        {
            if (jefeId <= 0)
            {
                return BadRequest(new ResultDTO<bool>
                {
                    Exitoso = false,
                    Mensaje = "ID de jefe inválido",
                    Errores = new List<string> { "El jefeId debe ser mayor a 0" }
                });
            }

            var resultado = await _vacacionesManager.AprobarSolicitudAsync(id, jefeId);

            if (!resultado.Exitoso)
            {
                if (resultado.Mensaje.Contains("no encontrada"))
                {
                    return NotFound(resultado);
                }

                return BadRequest(resultado);
            }

            return Ok(resultado);
        }

        [HttpPatch("{id}/rechazar")]
        [ProducesResponseType(typeof(ResultDTO<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDTO<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Rechazar(int id, [FromBody] RechazarVacacionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResultDTO<object>
                {
                    Exitoso = false,
                    Mensaje = "Datos inválidos",
                    Errores = ObtenerErroresModelState()
                });
            }

            var resultado = await _vacacionesManager.RechazarSolicitudAsync(
                id,
                request.JefeId,
                request.Comentarios
            );

            if (!resultado.Exitoso)
            {
                if (resultado.Mensaje.Contains("no encontrada"))
                {
                    return NotFound(resultado);
                }

                return BadRequest(resultado);
            }

            return Ok(resultado);
        }

        // ========================================
        // ENDPOINTS DE SALDOS
        // ========================================

        [HttpGet("saldo/{empleadoId}")]
        [ProducesResponseType(typeof(ResultDTO<SaldoVacacionesDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerSaldo(int empleadoId, [FromQuery] int? anio = null)
        {
            var anioConsulta = anio ?? DateTime.Now.Year;

            var resultado = await _vacacionesManager.ObtenerSaldoAsync(empleadoId, anioConsulta);

            if (!resultado.Exitoso)
            {
                return BadRequest(resultado);
            }

            return Ok(resultado);
        }

        [HttpGet("saldo/{empleadoId}/historial")]
        [ProducesResponseType(typeof(ResultDTO<IEnumerable<SaldoVacacionesDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerHistorialSaldos(int empleadoId)
        {
            var resultado = await _vacacionesManager.ObtenerHistorialSaldosAsync(empleadoId);

            return Ok(resultado);
        }

        [HttpPost("saldo/{empleadoId}/recalcular")]
        [ProducesResponseType(typeof(ResultDTO<SaldoVacacionesDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RecalcularSaldo(int empleadoId, [FromQuery] int anio)
        {
            var resultado = await _vacacionesManager.RecalcularSaldoAsync(empleadoId, anio);

            if (!resultado.Exitoso)
            {
                return BadRequest(resultado);
            }

            return Ok(resultado);
        }

        // ========================================
        // ENDPOINTS DE VALIDACIÓN
        // ========================================

        [HttpPost("validar")]
        [ProducesResponseType(typeof(ResultDTO<ValidacionVacacionesDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidarSolicitud([FromBody] ValidarVacacionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResultDTO<object>
                {
                    Exitoso = false,
                    Mensaje = "Datos inválidos",
                    Errores = ObtenerErroresModelState()
                });
            }

            var resultado = await _vacacionesManager.ValidarSolicitudAsync(
                request.EmpleadoId,
                request.FechaInicio,
                request.FechaFin
            );

            return Ok(resultado);
        }

        // ========================================
        // MÉTODOS AUXILIARES PRIVADOS
        // ========================================

        private List<string> ObtenerErroresModelState()
        {
            var errores = new List<string>();

            foreach (var error in ModelState.Values)
            {
                foreach (var errorMessage in error.Errors)
                {
                    errores.Add(errorMessage.ErrorMessage);
                }
            }

            return errores;
        }
    }

    // ========================================
    // CLASES DE REQUEST ADICIONALES
    // ========================================

    public class RechazarVacacionRequest
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El ID del jefe es requerido")]
        public int JefeId { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Los comentarios son requeridos")]
        [System.ComponentModel.DataAnnotations.MaxLength(1000)]
        public string Comentarios { get; set; } = null!;
    }

    public class ValidarVacacionRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        public int EmpleadoId { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public DateTime FechaInicio { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public DateTime FechaFin { get; set; }
    }
}