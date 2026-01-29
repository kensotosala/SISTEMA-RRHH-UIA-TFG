namespace BusinessLogicLayer.Interfaces
{
    public interface IEmailService
    {
        Task EnviarEmailConfirmacionSolicitudAsync(string emailDestino, string nombreEmpleado, string tipoSolicitud, string detalles);

        Task EnviarEmailAprobacionAsync(string emailDestino, string nombreEmpleado, string tipoSolicitud, string detalles);

        Task EnviarEmailRechazoAsync(string emailDestino, string nombreEmpleado, string tipoSolicitud, string motivo, string detalles);

        Task EnviarEmailCancelacionAsync(string emailDestino, string nombreEmpleado, string tipoSolicitud, string detalles);

        Task EnviarEmailNotificacionJefeAsync(string emailDestino, string nombreJefe, string nombreEmpleado, string tipoSolicitud, string detalles);
    }
}