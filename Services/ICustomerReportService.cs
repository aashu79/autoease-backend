using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace autoease_backend.Services
{
    public interface ICustomerReportService
    {
        Task<List<RegularCustomerReportDto>> GetRegularCustomersReportAsync();
        Task<List<HighSpenderReportDto>> GetHighSpendersReportAsync(decimal minAmount = 5000);
        Task<List<PendingCreditsReportDto>> GetPendingCreditsReportAsync();
    }

    public class RegularCustomerReportDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public int TotalInvoices { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime LastServiceDate { get; set; }
        public int VehiclesCount { get; set; }
    }

    public class HighSpenderReportDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public decimal TotalSpent { get; set; }
        public int InvoiceCount { get; set; }
        public decimal AverageInvoiceValue { get; set; }
        public DateTime FirstPurchaseDate { get; set; }
        public DateTime LastPurchaseDate { get; set; }
    }

    public class PendingCreditsReportDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public decimal PendingAmount { get; set; }
        public int PendingInvoiceCount { get; set; }
        public List<PendingInvoiceDto> PendingInvoices { get; set; } = new();
        public DateTime OldestInvoiceDate { get; set; }
    }

    public class PendingInvoiceDto
    {
        public int InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
    }
}