using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using autoease_backend.Data;
using autoease_backend.Data.Models;

namespace autoease_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly autoease_backend.Services.IEmailService _emailService;

        public InvoicesController(AppDbContext context, autoease_backend.Services.IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: api/Invoices/history
        [HttpGet("history")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyHistory()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var invoices = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .Where(i => i.CustomerId == userId)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();

            return Ok(invoices);
        }

        public class CreateInvoiceDto
        {
            public int VendorId { get; set; }
            public string Type { get; set; } = string.Empty;
            public DateTime DueDate { get; set; }
            public List<InvoiceItemDto> Items { get; set; } = new();
        }

        public class InvoiceItemDto
        {
            public int PartId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }

        // POST: api/Invoices
        [HttpPost]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceDto dto, [FromQuery] int customerId)
        {
            var vendorExists = await _context.Vendors.AnyAsync(v => v.Id == dto.VendorId);
            if (!vendorExists)
            {
                return BadRequest("Vendor not found.");
            }

            decimal totalAmount = dto.Items.Sum(i => i.Quantity * i.UnitPrice);
            decimal discountApplied = 0;

            // Loyalty Program: 10% discount if spend more than 5000
            if (totalAmount > 5000)
            {
                discountApplied = totalAmount * 0.10m;
            }

            var invoice = new Invoice
            {
                CustomerId = customerId,
                VendorId = dto.VendorId,
                Type = dto.Type,
                TotalAmount = totalAmount - discountApplied,
                DiscountApplied = discountApplied,
                PaymentStatus = "Pending",
                InvoiceDate = DateTime.UtcNow,
                DueDate = dto.DueDate,
                InvoiceItems = dto.Items.Select(i => new InvoiceItem
                {
                    PartId = i.PartId,
                    Quantity = i.Quantity
                }).ToList()
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            var createdInvoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == invoice.Id);

            if (createdInvoice == null)
            {
                return Ok(invoice);
            }

            return Ok(new
            {
                createdInvoice.Id,
                createdInvoice.CustomerId,
                Customer = createdInvoice.Customer == null
                    ? null
                    : new
                    {
                        createdInvoice.Customer.Id,
                        createdInvoice.Customer.Name,
                        createdInvoice.Customer.Email,
                        PhoneNumber = createdInvoice.Customer.PhoneNumber,
                        createdInvoice.Customer.Role
                    },
                createdInvoice.VendorId,
                createdInvoice.Type,
                createdInvoice.TotalAmount,
                createdInvoice.DiscountApplied,
                createdInvoice.PaymentStatus,
                createdInvoice.InvoiceDate,
                createdInvoice.DueDate,
                InvoiceItems = createdInvoice.InvoiceItems?.Select(item => new
                {
                    item.Id,
                    item.InvoiceId,
                    item.PartId,
                    item.Quantity
                })
            });
        }

        [HttpPost("{id}/send-email")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendInvoiceEmail(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound("Invoice not found.");
            }

            if (invoice.Customer == null || string.IsNullOrWhiteSpace(invoice.Customer.Email))
            {
                return BadRequest("Customer email not available.");
            }

            var emailBody = $@"<h2>Sales Invoice</h2>
<p>Invoice ID: {invoice.Id}</p>
<p>Total Amount: {invoice.TotalAmount}</p>
<p>Discount Applied: {invoice.DiscountApplied}</p>
<p>Payment Status: {invoice.PaymentStatus}</p>
<p>Invoice Date: {invoice.InvoiceDate:yyyy-MM-dd}</p>
<p>Due Date: {invoice.DueDate:yyyy-MM-dd}</p>";

            await _emailService.SendEmailAsync(invoice.Customer.Email, "Your Sales Invoice", emailBody);

            return Ok(new { message = "Invoice email sent successfully." });
        }
    }
}
