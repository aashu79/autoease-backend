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

        public InvoicesController(AppDbContext context)
        {
            _context = context;
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
            public int StaffId { get; set; }
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
                StaffId = dto.StaffId,
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

            return Ok(invoice);
        }
    }
}
