using System.Collections.Generic;

namespace autoease_backend.Data.Models
{
    public class Part
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }

        public Vendor? Vendor { get; set; }
        public ICollection<InvoiceItem>? InvoiceItems { get; set; }
    }
}
