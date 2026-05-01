using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using autoease_backend.Data;

namespace autoease_backend.Services
{
    public class InvoiceEmailService : IInvoiceEmailService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<InvoiceEmailService> _logger;

        public InvoiceEmailService(
            AppDbContext context,
            IEmailService emailService,
            ILogger<InvoiceEmailService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<bool> SendInvoiceAsync(int invoiceId)
        {
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.Customer)
                    .Include(i => i.InvoiceItems)
                    .ThenInclude(ii => ii.Part)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId);

                if (invoice == null)
                {
                    _logger.LogWarning($"Invoice {invoiceId} not found");
                    return false;
                }

                if (invoice.Customer == null)
                {
                    _logger.LogWarning($"Customer not found for invoice {invoiceId}");
                    return false;
                }

                return await SendInvoiceToCustomerAsync(invoiceId, invoice.Customer.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending invoice {invoiceId}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendInvoiceToCustomerAsync(int invoiceId, string customerEmail)
        {
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.Customer)
                    .Include(i => i.InvoiceItems)
                    .ThenInclude(ii => ii.Part)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId);

                if (invoice == null)
                {
                    _logger.LogWarning($"Invoice {invoiceId} not found");
                    return false;
                }

                var emailBody = GenerateInvoiceEmailBody(invoice);
                var subject = $"Invoice #{invoice.Id} - AutoEase";

                await _emailService.SendEmailAsync(customerEmail, subject, emailBody);

                _logger.LogInformation($"Invoice {invoiceId} sent to {customerEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending invoice {invoiceId} to {customerEmail}: {ex.Message}");
                return false;
            }
        }

        private string GenerateInvoiceEmailBody(autoease_backend.Data.Models.Invoice invoice)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; }");
            sb.AppendLine("table { border-collapse: collapse; width: 100%; margin: 10px 0; }");
            sb.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            sb.AppendLine("th { background-color: #4CAF50; color: white; }");
            sb.AppendLine(".header { background-color: #f2f2f2; padding: 20px; }");
            sb.AppendLine(".footer { margin-top: 20px; font-size: 12px; color: #666; }");
            sb.AppendLine(".total { font-weight: bold; background-color: #f2f2f2; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Header
            sb.AppendLine("<div class='header'>");
            sb.AppendLine("<h2>AutoEase - Invoice</h2>");
            sb.AppendLine($"<p><strong>Invoice #:</strong> {invoice.Id}</p>");
            sb.AppendLine($"<p><strong>Invoice Date:</strong> {invoice.InvoiceDate:yyyy-MM-dd}</p>");
            sb.AppendLine($"<p><strong>Due Date:</strong> {invoice.DueDate:yyyy-MM-dd}</p>");
            sb.AppendLine("</div>");

            // Customer Info
            sb.AppendLine("<div>");
            sb.AppendLine("<h3>Customer Information</h3>");
            sb.AppendLine($"<p><strong>Name:</strong> {invoice.Customer?.Name}</p>");
            sb.AppendLine($"<p><strong>Email:</strong> {invoice.Customer?.Email}</p>");
            sb.AppendLine($"<p><strong>Phone:</strong> {invoice.Customer?.Phone}</p>");
            sb.AppendLine("</div>");

            // Invoice Items
            sb.AppendLine("<div>");
            sb.AppendLine("<h3>Invoice Details</h3>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>Part Name</th>");
            sb.AppendLine("<th>Unit Price</th>");
            sb.AppendLine("<th>Quantity</th>");
            sb.AppendLine("<th>Amount</th>");
            sb.AppendLine("</tr>");

            if (invoice.InvoiceItems != null)
            {
                foreach (var item in invoice.InvoiceItems)
                {
                    var amount = (item.Part?.UnitPrice ?? 0) * item.Quantity;
                    sb.AppendLine("<tr>");
                    sb.AppendLine($"<td>{item.Part?.Name}</td>");
                    sb.AppendLine($"<td>${item.Part?.UnitPrice:F2}</td>");
                    sb.AppendLine($"<td>{item.Quantity}</td>");
                    sb.AppendLine($"<td>${amount:F2}</td>");
                    sb.AppendLine("</tr>");
                }
            }

            sb.AppendLine("</table>");
            sb.AppendLine("</div>");

            // Summary
            sb.AppendLine("<div>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td><strong>Subtotal:</strong></td>");
            sb.AppendLine($"<td>${invoice.TotalAmount:F2}</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td><strong>Discount Applied:</strong></td>");
            sb.AppendLine($"<td>-${invoice.DiscountApplied:F2}</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr class='total'>");
            sb.AppendLine($"<td><strong>Total Amount Due:</strong></td>");
            sb.AppendLine($"<td>${(invoice.TotalAmount - invoice.DiscountApplied):F2}</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine($"<tr><td><strong>Payment Status:</strong></td><td>{invoice.PaymentStatus}</td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");

            // Footer
            sb.AppendLine("<div class='footer'>");
            sb.AppendLine("<p>Thank you for your business!</p>");
            sb.AppendLine("<p>If you have any questions about this invoice, please contact us.</p>");
            sb.AppendLine("<p>AutoEase - Professional Vehicle Services</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
    }
}