using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Shared;
using DataAccessLayer.Entities;
using DataAccessLayer.Interfaces;

namespace BusinessLogicLayer.Managers
{
    public class AsistenciaManager : IAsistenciaManager
    {
        private readonly IAsistenciasRepository _repoAsistencias;

        public AsistenciaManager(IAsistenciasRepository repoAsistencias)
        {
            _repoAsistencias = repoAsistencias;
        }

        public async Task MarcarAsistenciaAsync(int empleadoId)
        {
            var hoy = DateTime.Today;

            var asistencia = await _repoAsistencias
                .GetByEmpleadoYFechaAsync(empleadoId, hoy);

            if (asistencia == null)
            {
                asistencia = new Asistencias
                {
                    EmpleadoId = empleadoId,
                    FechaRegistro = hoy,
                    HoraEntrada = DateTime.Now,
                    Estado = EstadoAsistencia.PRESENTE.ToString(),
                    FechaCreacion = DateTime.Now
                };

                await _repoAsistencias.CreateAsync(asistencia);
                return;
            }

            // SALIDA
            if (asistencia.HoraEntrada != null && asistencia.HoraSalida == null)
            {
                asistencia.HoraSalida = DateTime.Now;
                asistencia.FechaModificacion = DateTime.Now;

                await _repoAsistencias.UpdateAsync(asistencia);
                return;
            }

            // 🔴 COMPLETO
            throw new BusinessException("La asistencia de hoy ya fue completada.");
        }
    }
}