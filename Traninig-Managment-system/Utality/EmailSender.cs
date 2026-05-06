using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Traninig_Managment_system.Utality
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string Message)
        {
            var host = _configuration["Smtp:Host"];
            var userName = _configuration["Smtp:UserName"];
            var password = _configuration["Smtp:Password"];
            var fromEmail = _configuration["Smtp:FromEmail"] ?? userName;
            var fromName = _configuration["Smtp:FromName"] ?? "Training Management System";
            var portValue = _configuration["Smtp:Port"];

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(userName) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new InvalidOperationException("SMTP settings are not configured.");
            }

            var port = int.TryParse(portValue, out var parsedPort) ? parsedPort : 587;
            var enableSsl = bool.TryParse(_configuration["Smtp:EnableSsl"], out var parsedSsl)
                ? parsedSsl
                : true;

            var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(userName, password)
            };

            var message = new MailMessage(
                from: new MailAddress(fromEmail, fromName),
                to: new MailAddress(email))
            {
                Subject = subject,
                Body = Message,
                IsBodyHtml = true
            };

            return client.SendMailAsync(message);
        }
    }
}
