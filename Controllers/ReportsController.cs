using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using autoease_backend.Data;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace autoease_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // Admin Financial Reports
        [HttpGet("financial/daily")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDailyFinancialReport()
        {
            var reports = await _context.Invoices
                .GroupBy(i => i.InvoiceDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalSales = g.Where(i => i.Type == "Sales" || i.Type == "Service").Sum(i => i.TotalAmount - i.DiscountApplied),
                    TotalPurchases = g.Where(i => i.Type == "Purchase").Sum(i => i.TotalAmount),
                    NetProfit = g.Where(i => i.Type == "Sales" || i.Type == "Service").Sum(i => i.TotalAmount - i.DiscountApplied) - g.Where(i => i.Type == "Purchase").Sum(i => i.TotalAmount)
                })
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            return Ok(reports);
        }

        [HttpGet("financial/monthly")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMonthlyFinancialReport()
        {
            var reports = await _context.Invoices
                .GroupBy(i => new { i.InvoiceDate.Year, i.InvoiceDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalSales = g.Where(i => i.Type == "Sales" || i.Type == "Service").Sum(i => i.TotalAmount - i.DiscountApplied),
                    TotalPurchases = g.Where(i => i.Type == "Purchase").Sum(i => i.TotalAmount),
                    NetProfit = g.Where(i => i.Type == "Sales" || i.Type == "Service").Sum(i => i.TotalAmount - i.DiscountApplied) - g.Where(i => i.Type == "Purchase").Sum(i => i.TotalAmount)
                })
                .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
                .ToListAsync();

            return Ok(reports);
        }

        [HttpGet("financial/yearly")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetYearlyFinancialReport()
        {
            var reports = await _context.Invoices
                .GroupBy(i => i.InvoiceDate.Year)
                .Select(g => new
                {
                    Year = g.Key,
                    TotalSales = g.Where(i => i.Type == "Sales" || i.Type == "Service").Sum(i => i.TotalAmount - i.DiscountApplied),
                    TotalPurchases = g.Where(i => i.Type == "Purchase").Sum(i => i.TotalAmount),
                    NetProfit = g.Where(i => i.Type == "Sales" || i.Type == "Service").Sum(i => i.TotalAmount - i.DiscountApplied) - g.Where(i => i.Type == "Purchase").Sum(i => i.TotalAmount)
                })
                .OrderByDescending(x => x.Year)
                .ToListAsync();

            return Ok(reports);
        }

        // Staff Customer Reports
        [HttpGet("customers/regulars")]
        // [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetRegularCustomers()
        {
            // Customers with more than 3 invoices
            var regulars = await _context.Users
                .Where(u => u.Role == "Customer" || u.Role == "customer")
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.PhoneNumber,
                    InvoiceCount = _context.Invoices.Count(i => i.CustomerId == u.Id)
                })
                .Where(c => c.InvoiceCount >= 3)
                .OrderByDescending(c => c.InvoiceCount)
                .ToListAsync();

            return Ok(regulars);
        }

        [HttpGet("customers/high-spenders")]
        // [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetHighSpenders()
        {
            // Customers ordered by total spent
            var highSpenders = await _context.Users
                .Where(u => u.Role == "Customer" || u.Role == "customer")
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.PhoneNumber,
                    TotalSpent = _context.Invoices.Where(i => i.CustomerId == u.Id && (i.Type == "Sales" || i.Type == "Service")).Sum(i => i.TotalAmount - i.DiscountApplied)
                })
                .Where(c => c.TotalSpent > 0)
                .OrderByDescending(c => c.TotalSpent)
                .Take(50)
                .ToListAsync();

            return Ok(highSpenders);
        }

        [HttpGet("customers/pending-credits")]
        // [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetPendingCredits()
        {
            var pendingCredits = await _context.Users
                .Where(u => u.Role == "Customer" || u.Role == "customer")
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.PhoneNumber,
                    PendingAmount = _context.Invoices.Where(i => i.CustomerId == u.Id && i.PaymentStatus == "Pending").Sum(i => i.TotalAmount - i.DiscountApplied)
                })
                .Where(c => c.PendingAmount > 0)
                .OrderByDescending(c => c.PendingAmount)
                .ToListAsync();

            return Ok(pendingCredits);
        }
    }
}
