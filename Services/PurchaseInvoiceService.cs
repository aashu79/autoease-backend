using System;
using System.Threading.Tasks;
using autoease_backend.Data;
using autoease_backend.Data.Models;

namespace autoease_backend.Services
{
    public class PurchaseInvoiceService : IPurchaseInvoiceService
    {
        private readonly AppDbContext _context;

        public PurchaseInvoiceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Invoice> CreatePurchaseInvoiceAsync(Invoice invoice)
        {
            invoice.Type = "Purchase";
            invoice.InvoiceDate = DateTime.UtcNow;
            
            _context.Invoices.Add(invoice);
            
            if (invoice.InvoiceItems != null)
            {
                foreach (var item in invoice.InvoiceItems)
                {
                    var part = await _context.Parts.FindAsync(item.PartId);
                    if (part != null)
                    {
                        part.StockQuantity += item.Quantity;
                    }
                }
            }
            
            await _context.SaveChangesAsync();
            return invoice;
        }
    }
}
