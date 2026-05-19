using System.Collections.Generic;

namespace autoease_backend.Models
{
    public class Vendor
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public ICollection<Part>? Parts { get; set; }
        public ICollection<Invoice>? Invoices { get; set; }
    }
}

