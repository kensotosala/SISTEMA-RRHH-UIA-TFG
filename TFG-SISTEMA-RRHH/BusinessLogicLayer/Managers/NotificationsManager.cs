using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Interfaces;

namespace BusinessLogicLayer.Managers
{
    public class NotificacionesManager
    {
        private readonly IEmailService _emailService;
        private readonly IEmpleadosRepository _empleadosRepository;

        public NotificacionesManager(
            IEmailService emailService,
            IEmpleadosRepository empleadosRepository)
        {
            _emailService = emailService;
            _empleadosRepository = empleadosRepository;
        }

        public async Task NotificarSolicitudCreadaAsync(int empleadoId, string tipoSolicitud, string detalles)
        {
            var empleado = await _empleadosRepository.GetByIdAsync(empleadoId);
            if (empleado == null || string.IsNullOrEmpty(empleado.Email))
                return;

            await _emailService.EnviarEmailConfirmacionSolicitudAsync(
                empleado.Email,
                $"{empleado.Nombre} {empleado.PrimerApellido}",
                tipoSolicitud,
                detalles
            );

            // Notificar al jefe si existe
            if (empleado.JefeInmediatoId.HasValue)
            {
                var jefe = await _empleadosRepository.GetByIdAsync(empleado.JefeInmediatoId.Value);
                if (jefe != null && !string.IsNullOrEmpty(jefe.Email))
                {
                    await _emailService.EnviarEmailNotificacionJefeAsync(
                        jefe.Email,
                        $"{jefe.Nombre} {jefe.PrimerApellido}",
                        $"{empleado.Nombre} {empleado.PrimerApellido}",
                        tipoSolicitud,
                        detalles
                    );
                }
            }
        }

        public async Task NotificarSolicitudAprobadaAsync(int empleadoId, string tipoSolicitud, string detalles)
        {
            var empleado = await _empleadosRepository.GetByIdAsync(empleadoId);
            if (empleado == null || string.IsNullOrEmpty(empleado.Email))
                return;

            await _emailService.EnviarEmailAprobacionAsync(
                empleado.Email,
                $"{empleado.Nombre} {empleado.PrimerApellido}",
                tipoSolicitud,
                detalles
            );
        }

        public async Task NotificarSolicitudRechazadaAsync(int empleadoId, string tipoSolicitud, string motivo, string detalles)
        {
            var empleado = await _empleadosRepository.GetByIdAsync(empleadoId);
            if (empleado == null || string.IsNullOrEmpty(empleado.Email))
                return;

            await _emailService.EnviarEmailRechazoAsync(
                empleado.Email,
                $"{empleado.Nombre} {empleado.PrimerApellido}",
                tipoSolicitud,
                motivo,
                detalles
            );
        }

        public async Task NotificarSolicitudCanceladaAsync(int empleadoId, string tipoSolicitud, string detalles)
        {
            var empleado = await _empleadosRepository.GetByIdAsync(empleadoId);
            if (empleado == null || string.IsNullOrEmpty(empleado.Email))
                return;

            await _emailService.EnviarEmailCancelacionAsync(
                empleado.Email,
                $"{empleado.Nombre} {empleado.PrimerApellido}",
                tipoSolicitud,
                detalles
            );
        }
    }
}