using System.Threading.Tasks;

namespace autoease_backend.Services
{
    public interface IInvoiceEmailService
    {
        Task<bool> SendInvoiceAsync(int invoiceId);
        Task<bool> SendInvoiceToCustomerAsync(int invoiceId, string customerEmail);
    }
}