using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using autoease_backend.Data;

namespace autoease_backend.Services
{
    public class CustomerReportService : ICustomerReportService
    {
        private readonly AppDbContext _context;

        public CustomerReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RegularCustomerReportDto>> GetRegularCustomersReportAsync()
        {
            var customers = await _context.Users
                .Where(u => u.Role == "Customer")
                .Include(u => u.CustomerAppointments)
                .Include(u => u.CustomerInvoices)
                .Include(u => u.Vehicles)
                .ToListAsync();

            var regularCustomers = customers
                .Where(u => (u.CustomerAppointments?.Count ?? 0) >= 3 || (u.CustomerInvoices?.Count ?? 0) >= 2)
                .Select(u => new RegularCustomerReportDto
                {
                    CustomerId = u.Id,
                    CustomerName = u.Name,
                    Email = u.Email,
                    Phone = u.Phone,
                    TotalAppointments = u.CustomerAppointments?.Count ?? 0,
                    TotalInvoices = u.CustomerInvoices?.Count ?? 0,
                    TotalSpent = u.CustomerInvoices?.Sum(i => i.TotalAmount - i.DiscountApplied) ?? 0,
                    LastServiceDate = u.CustomerAppointments?
                        .OrderByDescending(a => a.ScheduledAt)
                        .FirstOrDefault()?.ScheduledAt ?? DateTime.MinValue,
                    VehiclesCount = u.Vehicles?.Count ?? 0
                })
                .OrderByDescending(r => r.TotalSpent)
                .ToList();

            return regularCustomers;
        }

        public async Task<List<HighSpenderReportDto>> GetHighSpendersReportAsync(decimal minAmount = 5000)
        {
            var customers = await _context.Users
                .Where(u => u.Role == "Customer")
                .Include(u => u.CustomerInvoices)
                .ToListAsync();

            var highSpenders = customers
                .AsEnumerable()
                .Where(u => (u.CustomerInvoices?.Sum(i => i.TotalAmount - i.DiscountApplied) ?? 0) >= minAmount)
                .Select(u => new HighSpenderReportDto
                {
                    CustomerId = u.Id,
                    CustomerName = u.Name,
                    Email = u.Email,
                    Phone = u.Phone,
                    TotalSpent = u.CustomerInvoices?.Sum(i => i.TotalAmount - i.DiscountApplied) ?? 0,
                    InvoiceCount = u.CustomerInvoices?.Count ?? 0,
                    AverageInvoiceValue = (u.CustomerInvoices?.Count ?? 0) > 0
                        ? (u.CustomerInvoices!.Sum(i => i.TotalAmount - i.DiscountApplied) / u.CustomerInvoices!.Count)
                        : 0,
                    FirstPurchaseDate = u.CustomerInvoices?
                        .OrderBy(i => i.InvoiceDate)
                        .FirstOrDefault()?.InvoiceDate ?? DateTime.MinValue,
                    LastPurchaseDate = u.CustomerInvoices?
                        .OrderByDescending(i => i.InvoiceDate)
                        .FirstOrDefault()?.InvoiceDate ?? DateTime.MinValue
                })
                .OrderByDescending(r => r.TotalSpent)
                .ToList();

            return highSpenders;
        }

        public async Task<List<PendingCreditsReportDto>> GetPendingCreditsReportAsync()
        {
            var customers = await _context.Users
                .Where(u => u.Role == "Customer")
                .Include(u => u.CustomerInvoices)
                .ToListAsync();

            var pendingCredits = customers
                .AsEnumerable()
                .Where(u => u.CustomerInvoices != null && u.CustomerInvoices.Any(i => i.PaymentStatus != "Paid"))
                .Select(u => new PendingCreditsReportDto
                {
                    CustomerId = u.Id,
                    CustomerName = u.Name,
                    Email = u.Email,
                    Phone = u.Phone,
                    PendingAmount = u.CustomerInvoices!
                        .Where(i => i.PaymentStatus != "Paid")
                        .Sum(i => i.TotalAmount - i.DiscountApplied),
                    PendingInvoiceCount = u.CustomerInvoices!
                        .Count(i => i.PaymentStatus != "Paid"),
                    PendingInvoices = u.CustomerInvoices!
                        .Where(i => i.PaymentStatus != "Paid")
                        .Select(i => new PendingInvoiceDto
                        {
                            InvoiceId = i.Id,
                            Amount = i.TotalAmount - i.DiscountApplied,
                            DueDate = i.DueDate,
                            DaysOverdue = Math.Max(0, (int)(DateTime.Now - i.DueDate).TotalDays)
                        })
                        .ToList(),
                    OldestInvoiceDate = u.CustomerInvoices!
                        .Where(i => i.PaymentStatus != "Paid")
                        .OrderBy(i => i.InvoiceDate)
                        .FirstOrDefault()?.InvoiceDate ?? DateTime.MinValue
                })
                .OrderByDescending(r => r.PendingAmount)
                .ToList();

            return pendingCredits;
        }
    }
}