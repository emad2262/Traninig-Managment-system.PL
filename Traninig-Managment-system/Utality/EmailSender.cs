using System.Net;
using System.Net.Mail;

namespace Traninig_Managment_system.Utality
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com",587)
            {
                EnableSsl=true, //الرسايل هتتبعت مشفره 
                UseDefaultCredentials=false, //هتتبعت من خلال اعدادات خاصه بينا 
                Credentials= new NetworkCredential("emadmehana22@gmail.com", "cdfy paxp anjg aouf")
            };
            return  client.SendMailAsync(new MailMessage(from: "emadmehana22@gmail.com", to: email, subject, htmlMessage)
            {
                IsBodyHtml=true
            });
        }
    }
}
