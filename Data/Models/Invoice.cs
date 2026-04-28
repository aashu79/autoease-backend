using System;
using System.Collections.Generic;

namespace autoease_backend.Data.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int VendorId { get; set; }
        public int StaffId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal DiscountApplied { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }

        public User? Customer { get; set; }
        public Vendor? Vendor { get; set; }
        public User? Staff { get; set; }
        public ICollection<InvoiceItem>? InvoiceItems { get; set; }
    }
}
