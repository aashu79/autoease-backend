namespace autoease_backend.DTOs
{
    // ── PURCHASE INVOICE DTOs ────────────────────────────────────────────────

    public class CreatePurchaseInvoiceDto
    {
        public int VendorId { get; set; }
        public int AdminId { get; set; }
        public List<PurchaseItemDto> Items { get; set; } = new();
    }

    public class PurchaseItemDto
    {
        public int PartId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }   // cost price paid to vendor
    }

    // ── SALES INVOICE DTOs ───────────────────────────────────────────────────

    public class CreateSalesInvoiceDto
    {
        public int CustomerId { get; set; }
        public int StaffId { get; set; }
        public string PaymentStatus { get; set; } = "Paid";  // Paid | Unpaid | Partial
        public bool SendEmailToCustomer { get; set; } = false;
        public List<SalesItemDto> Items { get; set; } = new();
    }

    public class SalesItemDto
    {
        public int PartId { get; set; }
        public int Quantity { get; set; }
    }

    // ── SHARED RESPONSE DTOs ─────────────────────────────────────────────────

    public class InvoiceResponseDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountApplied { get; set; }
        public bool LoyaltyDiscountApplied { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? VendorName { get; set; }
        public string? StaffName { get; set; }
        public List<InvoiceItemResponseDto> Items { get; set; } = new();
    }

    public class InvoiceItemResponseDto
    {
        public int PartId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
