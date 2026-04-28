using System.Collections.Generic;

namespace autoease_backend.Data.Models
{
    public class Part
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public int RequestedBy { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
        public string RequestStatus { get; set; } = string.Empty;
        public string RequestDescription { get; set; } = string.Empty;

        public Vendor? Vendor { get; set; }
        public User? Requester { get; set; }
        public ICollection<InvoiceItem>? InvoiceItems { get; set; }
    }
}
