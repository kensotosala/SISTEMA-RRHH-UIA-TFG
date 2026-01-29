using BusinessLogicLayer.DTOs;
using BusinessLogicLayer.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace BusinessLogicLayer.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfigurationDTO _emailConfig;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _logger = logger;
            _emailConfig = new EmailConfigurationDTO
            {
                SmtpServer = configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com",
                SmtpPort = int.Parse(configuration["EmailSettings:SmtpPort"] ?? "587"),
                SenderEmail = configuration["EmailSettings:SenderEmail"] ?? "",
                SenderName = configuration["EmailSettings:SenderName"] ?? "Sistema RRHH",
                Username = configuration["EmailSettings:Username"] ?? "",
                Password = configuration["EmailSettings:Password"] ?? "",
                EnableSsl = bool.Parse(configuration["EmailSettings:EnableSsl"] ?? "true")
            };
        }

        public async Task EnviarEmailConfirmacionSolicitudAsync(string emailDestino, string nombreEmpleado, string tipoSolicitud, string detalles)
        {
            var subject = $"Confirmación de Solicitud de {tipoSolicitud}";
            var body = GenerarHtmlConfirmacion(nombreEmpleado, tipoSolicitud, detalles);
            await EnviarEmailAsync(emailDestino, subject, body);
        }

        public async Task EnviarEmailAprobacionAsync(string emailDestino, string nombreEmpleado, string tipoSolicitud, string detalles)
        {
            var subject = $"✅ Solicitud de {tipoSolicitud} Aprobada";
            var body = GenerarHtmlAprobacion(nombreEmpleado, tipoSolicitud, detalles);
            await EnviarEmailAsync(emailDestino, subject, body);
        }

        public async Task EnviarEmailRechazoAsync(string emailDestino, string nombreEmpleado, string tipoSolicitud, string motivo, string detalles)
        {
            var subject = $"❌ Solicitud de {tipoSolicitud} Rechazada";
            var body = GenerarHtmlRechazo(nombreEmpleado, tipoSolicitud, motivo, detalles);
            await EnviarEmailAsync(emailDestino, subject, body);
        }

        public async Task EnviarEmailCancelacionAsync(string emailDestino, string nombreEmpleado, string tipoSolicitud, string detalles)
        {
            var subject = $"Solicitud de {tipoSolicitud} Cancelada";
            var body = GenerarHtmlCancelacion(nombreEmpleado, tipoSolicitud, detalles);
            await EnviarEmailAsync(emailDestino, subject, body);
        }

        public async Task EnviarEmailNotificacionJefeAsync(string emailDestino, string nombreJefe, string nombreEmpleado, string tipoSolicitud, string detalles)
        {
            var subject = $"Nueva Solicitud de {tipoSolicitud} - {nombreEmpleado}";
            var body = GenerarHtmlNotificacionJefe(nombreJefe, nombreEmpleado, tipoSolicitud, detalles);
            await EnviarEmailAsync(emailDestino, subject, body);
        }

        private async Task EnviarEmailAsync(string emailDestino, string subject, string body)
        {
            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_emailConfig.SenderEmail, _emailConfig.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(emailDestino);

                using var smtpClient = new SmtpClient(_emailConfig.SmtpServer, _emailConfig.SmtpPort)
                {
                    EnableSsl = _emailConfig.EnableSsl,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_emailConfig.Username, _emailConfig.Password)
                };

                await smtpClient.SendMailAsync(message);
                _logger.LogInformation($"Email enviado exitosamente a {emailDestino}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al enviar email a {emailDestino}: {ex.Message}");
                throw;
            }
        }

        private string GenerarHtmlConfirmacion(string nombreEmpleado, string tipoSolicitud, string detalles)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .header {{ background-color: #3b82f6; color: white; padding: 20px; border-radius: 5px; text-align: center; }}
        .content {{ padding: 20px; line-height: 1.6; }}
        .details {{ background-color: #f8f9fa; padding: 15px; border-left: 4px solid #3b82f6; margin: 20px 0; }}
        .footer {{ text-align: center; color: #666; font-size: 12px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Confirmación de Solicitud</h2>
        </div>
        <div class='content'>
            <p>Estimado/a <strong>{nombreEmpleado}</strong>,</p>
            <p>Tu solicitud de <strong>{tipoSolicitud}</strong> ha sido recibida correctamente y está pendiente de aprobación.</p>
            <div class='details'>
                <h3>Detalles de la Solicitud:</h3>
                {detalles}
            </div>
            <p>Recibirás una notificación cuando tu solicitud sea procesada.</p>
            <p>Gracias por usar nuestro sistema.</p>
        </div>
        <div class='footer'>
            <p>Sistema de Recursos Humanos</p>
            <p>Este es un mensaje automático, por favor no responder.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerarHtmlAprobacion(string nombreEmpleado, string tipoSolicitud, string detalles)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .header {{ background-color: #10b981; color: white; padding: 20px; border-radius: 5px; text-align: center; }}
        .content {{ padding: 20px; line-height: 1.6; }}
        .details {{ background-color: #f0fdf4; padding: 15px; border-left: 4px solid #10b981; margin: 20px 0; }}
        .footer {{ text-align: center; color: #666; font-size: 12px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>✅ Solicitud Aprobada</h2>
        </div>
        <div class='content'>
            <p>Estimado/a <strong>{nombreEmpleado}</strong>,</p>
            <p>¡Buenas noticias! Tu solicitud de <strong>{tipoSolicitud}</strong> ha sido <strong>APROBADA</strong>.</p>
            <div class='details'>
                <h3>Detalles de la Solicitud:</h3>
                {detalles}
            </div>
            <p>Puedes consultar los detalles completos en el sistema.</p>
        </div>
        <div class='footer'>
            <p>Sistema de Recursos Humanos</p>
            <p>Este es un mensaje automático, por favor no responder.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerarHtmlRechazo(string nombreEmpleado, string tipoSolicitud, string motivo, string detalles)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .header {{ background-color: #ef4444; color: white; padding: 20px; border-radius: 5px; text-align: center; }}
        .content {{ padding: 20px; line-height: 1.6; }}
        .details {{ background-color: #fef2f2; padding: 15px; border-left: 4px solid #ef4444; margin: 20px 0; }}
        .motivo {{ background-color: #fff7ed; padding: 15px; border-left: 4px solid #f59e0b; margin: 20px 0; }}
        .footer {{ text-align: center; color: #666; font-size: 12px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>❌ Solicitud Rechazada</h2>
        </div>
        <div class='content'>
            <p>Estimado/a <strong>{nombreEmpleado}</strong>,</p>
            <p>Lamentamos informarte que tu solicitud de <strong>{tipoSolicitud}</strong> ha sido <strong>RECHAZADA</strong>.</p>
            <div class='motivo'>
                <h3>Motivo del Rechazo:</h3>
                <p>{motivo}</p>
            </div>
            <div class='details'>
                <h3>Detalles de la Solicitud:</h3>
                {detalles}
            </div>
            <p>Si tienes dudas, por favor contacta con tu supervisor o con el departamento de Recursos Humanos.</p>
        </div>
        <div class='footer'>
            <p>Sistema de Recursos Humanos</p>
            <p>Este es un mensaje automático, por favor no responder.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerarHtmlCancelacion(string nombreEmpleado, string tipoSolicitud, string detalles)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .header {{ background-color: #6b7280; color: white; padding: 20px; border-radius: 5px; text-align: center; }}
        .content {{ padding: 20px; line-height: 1.6; }}
        .details {{ background-color: #f9fafb; padding: 15px; border-left: 4px solid #6b7280; margin: 20px 0; }}
        .footer {{ text-align: center; color: #666; font-size: 12px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Solicitud Cancelada</h2>
        </div>
        <div class='content'>
            <p>Estimado/a <strong>{nombreEmpleado}</strong>,</p>
            <p>Tu solicitud de <strong>{tipoSolicitud}</strong> ha sido <strong>CANCELADA</strong>.</p>
            <div class='details'>
                <h3>Detalles de la Solicitud:</h3>
                {detalles}
            </div>
            <p>Si la cancelación no fue realizada por ti, por favor contacta con Recursos Humanos de inmediato.</p>
        </div>
        <div class='footer'>
            <p>Sistema de Recursos Humanos</p>
            <p>Este es un mensaje automático, por favor no responder.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerarHtmlNotificacionJefe(string nombreJefe, string nombreEmpleado, string tipoSolicitud, string detalles)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .header {{ background-color: #8b5cf6; color: white; padding: 20px; border-radius: 5px; text-align: center; }}
        .content {{ padding: 20px; line-height: 1.6; }}
        .details {{ background-color: #faf5ff; padding: 15px; border-left: 4px solid #8b5cf6; margin: 20px 0; }}
        .action-button {{ background-color: #8b5cf6; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; margin-top: 10px; }}
        .footer {{ text-align: center; color: #666; font-size: 12px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Nueva Solicitud Pendiente</h2>W
        </div>
        <div class='content'>
            <p>Estimado/a <strong>{nombreJefe}</strong>,</p>
            <p>El empleado <strong>{nombreEmpleado}</strong> ha enviado una nueva solicitud de <strong>{tipoSolicitud}</strong> que requiere tu aprobación.</p>
            <div class='details'>
                <h3>Detalles de la Solicitud:</h3>
                {detalles}
            </div>
            <p>Por favor, revisa y procesa esta solicitud en el sistema.</p>
            <a href='#' class='action-button'>Ir al Sistema</a>
        </div>
        <div class='footer'>
            <p>Sistema de Recursos Humanos</p>
            <p>Este es un mensaje automático, por favor no responder.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}