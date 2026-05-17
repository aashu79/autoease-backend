using System.Threading.Tasks;
using autoease_backend.Data.Models;

namespace autoease_backend.Services
{
    public interface ISalesInvoiceService
    {
        Task<Invoice> CreateSalesInvoiceAsync(Invoice invoice);
    }
}
