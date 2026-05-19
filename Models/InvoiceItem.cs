namespace autoease_backend.Models
{
    public class InvoiceItem
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int PartId { get; set; }
        public int Quantity { get; set; }

        public Invoice? Invoice { get; set; }
        public Part? Part { get; set; }
    }
}

