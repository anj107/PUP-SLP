using Microsoft.Extensions.Configuration; 
using System.Net; 
using System.Net.Mail; // Required for SmtpClient and MailMessage
using System.Threading.Tasks; 

namespace pupslp_tickets.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var smtpHost = smtpSettings["Host"];
            var smtpPort = int.Parse(smtpSettings["Port"]); // Convert string port to int
            var smtpUsername = smtpSettings["Username"];
            var smtpPassword = smtpSettings["Password"];

            using (var client = new SmtpClient(smtpHost, smtpPort)) // Use 'using' for proper disposal
            {
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUsername, "PUP Sining-Lahi"), // Display name for your sender
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true // Set to true if your message contains HTML
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}
