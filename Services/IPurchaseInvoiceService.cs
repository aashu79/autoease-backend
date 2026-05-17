using System.Threading.Tasks;
using autoease_backend.Data.Models;

namespace autoease_backend.Services
{
    public interface IPurchaseInvoiceService
    {
        Task<Invoice> CreatePurchaseInvoiceAsync(Invoice invoice);
    }
}
