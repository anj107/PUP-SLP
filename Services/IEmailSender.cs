
using System.Threading.Tasks;

namespace pupslp_tickets.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}



