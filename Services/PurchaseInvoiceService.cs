using Microsoft.EntityFrameworkCore;
using autoease_backend.Data;
using autoease_backend.Data.Models;
using autoease_backend.DTOs;

namespace autoease_backend.Services
{
    public interface IPurchaseInvoiceService
    {
        Task<InvoiceResponseDto> CreateAsync(CreatePurchaseInvoiceDto dto);
        Task<InvoiceResponseDto?> GetByIdAsync(int id);
        Task<List<InvoiceResponseDto>> GetAllAsync();
    }

    public class PurchaseInvoiceService : IPurchaseInvoiceService
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly ILogger<PurchaseInvoiceService> _logger;

        public PurchaseInvoiceService(
            AppDbContext db,
            IEmailService emailService,
            IConfiguration config,
            ILogger<PurchaseInvoiceService> logger)
        {
            _db = db;
            _emailService = emailService;
            _config = config;
            _logger = logger;
        }

        public async Task<InvoiceResponseDto> CreateAsync(CreatePurchaseInvoiceDto dto)
        {
            var vendor = await _db.Vendors.FindAsync(dto.VendorId)
                ?? throw new KeyNotFoundException($"Vendor ID {dto.VendorId} not found.");

            var admin = await _db.Users.FindAsync(dto.AdminId)
                ?? throw new KeyNotFoundException($"User ID {dto.AdminId} not found.");

            if (admin.Role != "Admin")
                throw new UnauthorizedAccessException("Only admins can create purchase invoices.");

            if (!dto.Items.Any())
                throw new ArgumentException("At least one item is required.");

            var partIds = dto.Items.Select(i => i.PartId).ToList();
            var parts = await _db.Parts.Where(p => partIds.Contains(p.Id)).ToListAsync();

            var missing = partIds.Except(parts.Select(p => p.Id)).ToList();
            if (missing.Any())
                throw new KeyNotFoundException($"Parts not found: {string.Join(", ", missing)}");

            decimal total = 0;
            var invoiceItems = new List<InvoiceItem>();

            foreach (var item in dto.Items)
            {
                if (item.Quantity <= 0)
                    throw new ArgumentException($"Quantity must be > 0 for part ID {item.PartId}.");
                if (item.UnitPrice <= 0)
                    throw new ArgumentException($"Unit price must be > 0 for part ID {item.PartId}.");

                total += item.Quantity * item.UnitPrice;
                invoiceItems.Add(new InvoiceItem { PartId = item.PartId, Quantity = item.Quantity });
            }

            var invoice = new Invoice
            {
                Type = "Purchase",
                VendorId = dto.VendorId,
                StaffId = dto.AdminId,
                CustomerId = dto.AdminId,
                TotalAmount = total,
                DiscountApplied = 0,
                PaymentStatus = "Paid",
                InvoiceDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                InvoiceItems = invoiceItems
            };

            _db.Invoices.Add(invoice);

            var lowStockParts = new List<Part>();
            foreach (var item in dto.Items)
            {
                var part = parts.First(p => p.Id == item.PartId);
                part.StockQuantity += item.Quantity;
                if (part.StockQuantity < 10)
                    lowStockParts.Add(part);
            }

            await _db.SaveChangesAsync();

            var adminEmail = _config["SmtpSettings:AdminEmail"];
            if (!string.IsNullOrEmpty(adminEmail))
            {
                foreach (var p in lowStockParts)
                {
                    await _emailService.SendEmailAsync(adminEmail,
                        $"Low Stock Alert: {p.Name}",
                        $"<p>Part <strong>{p.Name}</strong> has only <strong>{p.StockQuantity}</strong> units left.</p>");
                    _logger.LogWarning("Low stock alert for {Part}, qty: {Qty}", p.Name, p.StockQuantity);
                }
            }

            return MapToDto(invoice, vendor.Name, admin.Name, null, parts);
        }

        public async Task<InvoiceResponseDto?> GetByIdAsync(int id)
        {
            var invoice = await _db.Invoices
                .Include(i => i.Vendor)
                .Include(i => i.Staff)
                .Include(i => i.InvoiceItems!).ThenInclude(ii => ii.Part)
                .FirstOrDefaultAsync(i => i.Id == id && i.Type == "Purchase");

            if (invoice == null) return null;
            var parts = invoice.InvoiceItems!.Select(ii => ii.Part!).ToList();
            return MapToDto(invoice, invoice.Vendor?.Name, invoice.Staff?.Name, null, parts);
        }

        public async Task<List<InvoiceResponseDto>> GetAllAsync()
        {
            var invoices = await _db.Invoices
                .Where(i => i.Type == "Purchase")
                .Include(i => i.Vendor)
                .Include(i => i.Staff)
                .Include(i => i.InvoiceItems!).ThenInclude(ii => ii.Part)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();

            return invoices.Select(inv =>
                MapToDto(inv, inv.Vendor?.Name, inv.Staff?.Name, null,
                    inv.InvoiceItems!.Select(ii => ii.Part!).ToList())
            ).ToList();
        }

        private static InvoiceResponseDto MapToDto(
            Invoice invoice, string? vendorName, string? staffName, string? customerName, List<Part> parts)
        {
            return new InvoiceResponseDto
            {
                Id = invoice.Id,
                Type = invoice.Type,
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate,
                TotalAmount = invoice.TotalAmount,
                DiscountApplied = invoice.DiscountApplied,
                LoyaltyDiscountApplied = false,
                PaymentStatus = invoice.PaymentStatus,
                VendorName = vendorName,
                StaffName = staffName,
                CustomerName = customerName,
                Items = invoice.InvoiceItems?.Select(ii =>
                {
                    var part = parts.FirstOrDefault(p => p.Id == ii.PartId);
                    return new InvoiceItemResponseDto
                    {
                        PartId = ii.PartId,
                        PartName = part?.Name ?? "Unknown",
                        Quantity = ii.Quantity,
                        UnitPrice = part?.UnitPrice ?? 0,
                        LineTotal = ii.Quantity * (part?.UnitPrice ?? 0)
                    };
                }).ToList() ?? new()
            };
        }
    }
}
