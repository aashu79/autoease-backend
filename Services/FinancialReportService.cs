using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using autoease_backend.Data;

namespace autoease_backend.Services
{
    public class FinancialReportService : IFinancialReportService
    {
        private readonly AppDbContext _context;

        public FinancialReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<FinancialReportDto> GetDailyReportAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            return await GenerateReportAsync(startDate, endDate, $"Daily Report - {date:yyyy-MM-dd}");
        }

        public async Task<FinancialReportDto> GetMonthlyReportAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            return await GenerateReportAsync(startDate, endDate, $"Monthly Report - {year}-{month:D2}");
        }

        public async Task<FinancialReportDto> GetYearlyReportAsync(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = startDate.AddYears(1);

            return await GenerateReportAsync(startDate, endDate, $"Yearly Report - {year}");
        }

        private async Task<FinancialReportDto> GenerateReportAsync(DateTime startDate, DateTime endDate, string period)
        {
            var invoices = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceItems)
                .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate < endDate)
                .ToListAsync();

            var report = new FinancialReportDto
            {
                ReportDate = DateTime.Now,
                ReportPeriod = period,
                TotalInvoices = invoices.Count,
                TotalRevenue = invoices.Sum(i => i.TotalAmount),
                TotalDiscounts = invoices.Sum(i => i.DiscountApplied),
                NetRevenue = invoices.Sum(i => i.TotalAmount - i.DiscountApplied),
                PaidInvoices = invoices.Count(i => i.PaymentStatus == "Paid"),
                PendingInvoices = invoices.Count(i => i.PaymentStatus != "Paid"),
                OutstandingAmount = invoices
                    .Where(i => i.PaymentStatus != "Paid")
                    .Sum(i => i.TotalAmount - i.DiscountApplied)
            };

            report.InvoiceDetails = invoices.Select(i => new InvoiceBreakdownDto
            {
                InvoiceId = i.Id,
                CustomerName = i.Customer?.Name ?? "Unknown",
                Amount = i.TotalAmount,
                DiscountApplied = i.DiscountApplied,
                NetAmount = i.TotalAmount - i.DiscountApplied,
                PaymentStatus = i.PaymentStatus,
                InvoiceDate = i.InvoiceDate
            }).ToList();

            return report;
        }
    }
}