using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace autoease_backend.Services
{
    public interface IFinancialReportService
    {
        Task<FinancialReportDto> GetDailyReportAsync(DateTime date);
        Task<FinancialReportDto> GetMonthlyReportAsync(int year, int month);
        Task<FinancialReportDto> GetYearlyReportAsync(int year);
    }

    public class FinancialReportDto
    {
        public DateTime ReportDate { get; set; }
        public string ReportPeriod { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public decimal TotalDiscounts { get; set; }
        public decimal NetRevenue { get; set; }
        public int TotalInvoices { get; set; }
        public int PaidInvoices { get; set; }
        public int PendingInvoices { get; set; }
        public decimal OutstandingAmount { get; set; }
        public List<InvoiceBreakdownDto> InvoiceDetails { get; set; } = new();
    }

    public class InvoiceBreakdownDto
    {
        public int InvoiceId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal DiscountApplied { get; set; }
        public decimal NetAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
    }
}