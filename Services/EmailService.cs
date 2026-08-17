using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Pos.Services.Interfaces;

namespace Pos.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? "";
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? "";
                var fromEmail = _configuration["EmailSettings:FromEmail"] ?? smtpUsername;
                var fromName = _configuration["EmailSettings:FromName"] ?? "POS System";

                using var client = new SmtpClient(smtpServer, smtpPort);
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };
                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email enviado a: {to}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al enviar email: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string to, string userName, string resetToken)
        {
            var resetLink = $"{_configuration["AppUrl"]}/Account/RestablecerPassword?token={resetToken}&email={to}";

            var body = $@"
            <h2>Recuperación de Contraseña - POS System</h2>
            <p>Hola <strong>{userName}</strong>,</p>
            <p>Hemos recibido una solicitud para restablecer tu contraseña. Haz clic en el siguiente enlace para continuar:</p>
            <p><a href='{resetLink}' style='background-color: #4CAF50; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px;'>Restablecer Contraseña</a></p>
            <p>O copia y pega este enlace en tu navegador:</p>
            <p><code>{resetLink}</code></p>
            <p>Este enlace expirará en 1 hora.</p>
            <p>Si no solicitaste este cambio, ignora este mensaje.</p>
            <br>
            <p>Saludos,<br><strong>POS System</strong></p>
            <hr>
            <p style='color: #888; font-size: 12px;'>Este es un mensaje automático, por favor no responder.</p>
            ";

            return await SendEmailAsync(to, "Recuperación de Contraseña - POS System", body);
        }

        public async Task<bool> SendPasswordChangedNotificationAsync(string to, string userName)
        {
            var body = $@"
            <h2>Contraseña Cambiada - POS System</h2>
            <p>Hola <strong>{userName}</strong>,</p>
            <p>Tu contraseña ha sido cambiada exitosamente.</p>
            <p>Si no realizaste este cambio, contacta al administrador inmediatamente.</p>
            <br>
            <p>Saludos,<br><strong>POS System</strong></p>
            ";

            return await SendEmailAsync(to, "Contraseña Cambiada - POS System", body);
        }
    }
}