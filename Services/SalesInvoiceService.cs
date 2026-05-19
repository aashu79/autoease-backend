using System;
using System.Threading.Tasks;
using autoease_backend.Data;
using autoease_backend.Data.Models;

namespace autoease_backend.Services
{
    public class SalesInvoiceService : ISalesInvoiceService
    {
        private readonly AppDbContext _context;

        public SalesInvoiceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Invoice> CreateSalesInvoiceAsync(Invoice invoice)
        {
            invoice.Type = "Sales";
            invoice.InvoiceDate = DateTime.UtcNow;

            // Feature 16: Loyalty Program: 10% discount if they spend more than 5000
            if (invoice.TotalAmount > 5000)
            {
                decimal discount = invoice.TotalAmount * 0.10m;
                invoice.DiscountApplied = discount;
                invoice.TotalAmount -= discount;
            }
            
            _context.Invoices.Add(invoice);
            
            if (invoice.InvoiceItems != null)
            {
                foreach (var item in invoice.InvoiceItems)
                {
                    // Ensure the item is linked to the invoice for proper relationship tracking
                    item.Invoice = invoice;
                    
                    var part = await _context.Parts.FindAsync(item.PartId);
                    if (part != null)
                    {
                        part.StockQuantity -= item.Quantity;
                    }
                }
            }
            
            await _context.SaveChangesAsync();

            return invoice;
        }
    }
}
