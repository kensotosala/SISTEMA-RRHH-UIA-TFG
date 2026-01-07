using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Shared;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogicLayer.Managers
{
    public class AsistenciaManager : IAsistenciaManager
    {
        private readonly IAsistenciasRepository _repoAsistencias;
        private readonly ILogger<AsistenciaManager> _logger;

        public AsistenciaManager(IAsistenciasRepository repoAsistencias, ILogger<AsistenciaManager> logger)
        {
            _repoAsistencias = repoAsistencias;
            _logger = logger;
        }

        public async Task<MarcarAsistenciaResponse> MarcarAsistenciaAsync(int empleadoId)
        {
            var hoy = DateTime.Today;
            var ahora = DateTime.Now;

            // Buscar asistencia del día
            var asistencia = await _repoAsistencias.GetByEmpleadoYFechaAsync(empleadoId, hoy);

            // Caso: no existe registro => marcar entrada
            if (asistencia == null)
            {
                asistencia = new Asistencias
                {
                    EmpleadoId = empleadoId,
                    FechaRegistro = hoy,
                    HoraEntrada = ahora,
                    Estado = EstadoAsistencia.PRESENTE.ToString(),
                    FechaCreacion = ahora
                };

                await _repoAsistencias.CreateAsync(asistencia);
                _logger.LogInformation($"Entrada registrada para empleado {empleadoId} a las {ahora:HH:mm}");

                return new MarcarAsistenciaResponse
                {
                    Accion = "ENTRADA",
                    Hora = ahora,
                    Mensaje = "Entrada registrada exitosamente",
                    HoraEntrada = asistencia.HoraEntrada,
                    Estado = asistencia.Estado,
                    Exito = true
                };
            }

            // Caso: tiene entrada pero no salida => marcar salida
            if (asistencia.HoraEntrada != null && asistencia.HoraSalida == null)
            {
                asistencia.HoraSalida = ahora;
                asistencia.FechaModificacion = ahora;

                await _repoAsistencias.UpdateAsync(asistencia);
                _logger.LogInformation($"Salida registrada para empleado {empleadoId} a las {ahora:HH:mm}");

                return new MarcarAsistenciaResponse
                {
                    Accion = "SALIDA",
                    Hora = ahora,
                    Mensaje = "Salida registrada exitosamente",
                    HoraEntrada = asistencia.HoraEntrada,
                    HoraSalida = asistencia.HoraSalida,
                    Estado = asistencia.Estado,
                    Exito = true
                };
            }

            // Caso: ya tiene entrada y salida => devolver info sin lanzar excepción
            if (asistencia.HoraEntrada != null && asistencia.HoraSalida != null)
            {
                var mensaje = $"La asistencia de hoy ya fue completada. " +
                              $"Entrada: {asistencia.HoraEntrada:HH:mm}, " +
                              $"Salida: {asistencia.HoraSalida:HH:mm}";

                _logger.LogInformation(mensaje);

                return new MarcarAsistenciaResponse
                {
                    Accion = "YA_COMPLETADO",
                    Hora = ahora,
                    Mensaje = mensaje,
                    HoraEntrada = asistencia.HoraEntrada,
                    HoraSalida = asistencia.HoraSalida,
                    Estado = asistencia.Estado,
                    Exito = false
                };
            }

            // Caso especial: registro existe pero sin hora de entrada
            asistencia.HoraEntrada = ahora;
            asistencia.FechaModificacion = ahora;
            await _repoAsistencias.UpdateAsync(asistencia);

            return new MarcarAsistenciaResponse
            {
                Accion = "ENTRADA_CORREGIDA",
                Hora = ahora,
                Mensaje = "Entrada registrada (corrección)",
                HoraEntrada = asistencia.HoraEntrada,
                Estado = asistencia.Estado,
                Exito = true
            };
        }

        public async Task<EstadoAsistenciaDTO> ObtenerEstadoAsistenciaAsync(int empleadoId)
        {
            var hoy = DateTime.Today;
            var asistencia = await _repoAsistencias.GetByEmpleadoYFechaAsync(empleadoId, hoy);

            if (asistencia == null)
            {
                return new EstadoAsistenciaDTO
                {
                    TieneRegistro = false,
                    PuedeMarcarEntrada = true,
                    PuedeMarcarSalida = false,
                    Estado = "SIN_REGISTRO",
                    Mensaje = "Puede marcar entrada"
                };
            }

            var puedeMarcarEntrada = asistencia.HoraEntrada == null;
            var puedeMarcarSalida = asistencia.HoraEntrada != null && asistencia.HoraSalida == null;
            var estado = puedeMarcarSalida ? "ENTRADA_REGISTRADA" :
                         puedeMarcarEntrada ? "SIN_ENTRADA" :
                         "COMPLETO";

            return new EstadoAsistenciaDTO
            {
                TieneRegistro = true,
                HoraEntrada = asistencia.HoraEntrada,
                HoraSalida = asistencia.HoraSalida,
                PuedeMarcarEntrada = puedeMarcarEntrada,
                PuedeMarcarSalida = puedeMarcarSalida,
                Estado = estado,
                Mensaje = estado switch
                {
                    "ENTRADA_REGISTRADA" => $"Entrada registrada a las {asistencia.HoraEntrada:HH:mm}. Puede marcar salida.",
                    "COMPLETO" => $"Asistencia completa. Entrada: {asistencia.HoraEntrada:HH:mm}, Salida: {asistencia.HoraSalida:HH:mm}",
                    _ => "Puede marcar entrada"
                }
            };
        }
    }
}
