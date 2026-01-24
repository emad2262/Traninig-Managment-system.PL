using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Traninig_Managment_system.Utality
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string Message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("emadmehana22@gmail.com", "jjlo rzch cfno ohmb")
            };

            return client.SendMailAsync(
                new MailMessage(from: "emadmehana22@gmail.com",
                                to: email,
                                subject,
                                Message
                                )
                { IsBodyHtml = true });
        }
    }
}
