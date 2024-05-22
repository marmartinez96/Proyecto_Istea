
using System.Net;
using System.Net.Mail;

namespace ProdigyPlanningAPI.Helpers
{
    public class EmailSender : IEmailSender
    {
        private string _email = "noti-prodigy-planning@outlook.com";
        private string _password = "oY}w92]0C=so";

        public Task SendEmailAsync(string email, string subject, string message)
        {    
            SmtpClient client = new SmtpClient("smtp-mail.outlook.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_email,_password)
            };
            MailMessage _mail = new MailMessage();
            _mail.IsBodyHtml = true;
            _mail.From = new MailAddress(_email);
            _mail.To.Add(email);
            _mail.Subject = subject;
            _mail.Body = message;

            return client.SendMailAsync(_mail);
        }
    }
}
