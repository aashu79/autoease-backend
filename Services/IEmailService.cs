using System.Threading.Tasks;

namespace autoease_backend.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }
}