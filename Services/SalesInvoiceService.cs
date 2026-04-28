using Microsoft.EntityFrameworkCore;
using autoease_backend.Data;
using autoease_backend.Data.Models;
using autoease_backend.DTOs;

namespace autoease_backend.Services
{
    public interface ISalesInvoiceService
    {
        Task<InvoiceResponseDto> CreateAsync(CreateSalesInvoiceDto dto);
        Task<InvoiceResponseDto?> GetByIdAsync(int id);
        Task<List<InvoiceResponseDto>> GetAllAsync();
        Task<List<InvoiceResponseDto>> GetByCustomerIdAsync(int customerId);
    }

    public class SalesInvoiceService : ISalesInvoiceService
    {
        private const decimal LoyaltyThreshold = 5000m;
        private const decimal LoyaltyDiscountRate = 0.10m;

        private readonly AppDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly ILogger<SalesInvoiceService> _logger;

        public SalesInvoiceService(AppDbContext db, IEmailService emailService,
            IConfiguration config, ILogger<SalesInvoiceService> logger)
        {
            _db = db; _emailService = emailService; _config = config; _logger = logger;
        }

        public async Task<InvoiceResponseDto> CreateAsync(CreateSalesInvoiceDto dto)
        {
            var customer = await _db.Users.FindAsync(dto.CustomerId)
                ?? throw new KeyNotFoundException($"Customer ID {dto.CustomerId} not found.");
            if (customer.Role != "Customer")
                throw new ArgumentException("Specified user is not a customer.");

            var staff = await _db.Users.FindAsync(dto.StaffId)
                ?? throw new KeyNotFoundException($"Staff ID {dto.StaffId} not found.");
            if (staff.Role != "Staff")
                throw new ArgumentException("Specified user is not a staff member.");

            if (!dto.Items.Any())
                throw new ArgumentException("At least one item is required.");

            var partIds = dto.Items.Select(i => i.PartId).ToList();
            var parts = await _db.Parts.Where(p => partIds.Contains(p.Id)).ToListAsync();

            var missing = partIds.Except(parts.Select(p => p.Id)).ToList();
            if (missing.Any())
                throw new KeyNotFoundException($"Parts not found: {string.Join(", ", missing)}");

            foreach (var item in dto.Items)
            {
                var part = parts.First(p => p.Id == item.PartId);
                if (part.StockQuantity < item.Quantity)
                    throw new InvalidOperationException(
                        $"Insufficient stock for '{part.Name}'. Available: {part.StockQuantity}, Requested: {item.Quantity}.");
            }

            decimal subtotal = 0;
            var invoiceItems = new List<InvoiceItem>();

            foreach (var item in dto.Items)
            {
                if (item.Quantity <= 0)
                    throw new ArgumentException($"Quantity must be > 0 for part ID {item.PartId}.");
                var part = parts.First(p => p.Id == item.PartId);
                subtotal += item.Quantity * part.UnitPrice;
                invoiceItems.Add(new InvoiceItem { PartId = item.PartId, Quantity = item.Quantity });
            }

            bool loyaltyApplied = subtotal > LoyaltyThreshold;
            decimal discount = loyaltyApplied ? Math.Round(subtotal * LoyaltyDiscountRate, 2) : 0;
            decimal total = subtotal - discount;

            // VendorId is NOT NULL in DB, use 1 as placeholder for sales invoices
            var firstVendor = await _db.Vendors.FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("No vendors exist. Please add a vendor first.");

            var invoice = new Invoice
            {
                Type = "Sales",
                CustomerId = dto.CustomerId,
                StaffId = dto.StaffId,
                VendorId = firstVendor.Id,
                TotalAmount = total,
                DiscountApplied = discount,
                PaymentStatus = dto.PaymentStatus,
                InvoiceDate = DateTime.UtcNow,
                DueDate = dto.PaymentStatus == "Unpaid" ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow,
                InvoiceItems = invoiceItems
            };

            _db.Invoices.Add(invoice);

            var lowStockParts = new List<Part>();
            foreach (var item in dto.Items)
            {
                var part = parts.First(p => p.Id == item.PartId);
                part.StockQuantity -= item.Quantity;
                if (part.StockQuantity < 10) lowStockParts.Add(part);
            }

            await _db.SaveChangesAsync();

            if (dto.SendEmailToCustomer)
            {
                var body = $@"<h2>AutoEase</h2><p>Dear {customer.Name},</p>
                    <p>Invoice <strong>#{invoice.Id}</strong> for <strong>NPR {total:N2}</strong> created.</p>
                    {(loyaltyApplied ? $"<p style='color:green'>Loyalty discount of NPR {discount:N2} applied!</p>" : "")}";
                await _emailService.SendEmailAsync(customer.Email, $"AutoEase – Invoice #{invoice.Id}", body);
            }

            var adminEmail = _config["SmtpSettings:AdminEmail"];
            if (!string.IsNullOrEmpty(adminEmail))
            {
                foreach (var p in lowStockParts)
                {
                    await _emailService.SendEmailAsync(adminEmail,
                        $"Low Stock Alert: {p.Name}",
                        $"<p>Part <strong>{p.Name}</strong> has only <strong>{p.StockQuantity}</strong> units left.</p>");
                    _logger.LogWarning("Low stock after sale — Part: {Part}, Qty: {Qty}", p.Name, p.StockQuantity);
                }
            }

            return MapToDto(invoice, null, staff.Name, customer.Name, parts, loyaltyApplied);
        }

        public async Task<InvoiceResponseDto?> GetByIdAsync(int id)
        {
            var invoice = await _db.Invoices
                .Include(i => i.Customer).Include(i => i.Staff)
                .Include(i => i.InvoiceItems!).ThenInclude(ii => ii.Part)
                .FirstOrDefaultAsync(i => i.Id == id && i.Type == "Sales");
            if (invoice == null) return null;
            return MapToDto(invoice, null, invoice.Staff?.Name, invoice.Customer?.Name,
                invoice.InvoiceItems!.Select(ii => ii.Part!).ToList(), invoice.DiscountApplied > 0);
        }

        public async Task<List<InvoiceResponseDto>> GetAllAsync()
        {
            var invoices = await _db.Invoices.Where(i => i.Type == "Sales")
                .Include(i => i.Customer).Include(i => i.Staff)
                .Include(i => i.InvoiceItems!).ThenInclude(ii => ii.Part)
                .OrderByDescending(i => i.InvoiceDate).ToListAsync();
            return invoices.Select(inv => MapToDto(inv, null, inv.Staff?.Name, inv.Customer?.Name,
                inv.InvoiceItems!.Select(ii => ii.Part!).ToList(), inv.DiscountApplied > 0)).ToList();
        }

        public async Task<List<InvoiceResponseDto>> GetByCustomerIdAsync(int customerId)
        {
            var invoices = await _db.Invoices
                .Where(i => i.Type == "Sales" && i.CustomerId == customerId)
                .Include(i => i.Customer).Include(i => i.Staff)
                .Include(i => i.InvoiceItems!).ThenInclude(ii => ii.Part)
                .OrderByDescending(i => i.InvoiceDate).ToListAsync();
            return invoices.Select(inv => MapToDto(inv, null, inv.Staff?.Name, inv.Customer?.Name,
                inv.InvoiceItems!.Select(ii => ii.Part!).ToList(), inv.DiscountApplied > 0)).ToList();
        }

        private static InvoiceResponseDto MapToDto(Invoice invoice, string? vendorName,
            string? staffName, string? customerName, List<Part> parts, bool loyaltyApplied = false)
        {
            return new InvoiceResponseDto
            {
                Id = invoice.Id, Type = invoice.Type, InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate, TotalAmount = invoice.TotalAmount,
                DiscountApplied = invoice.DiscountApplied, LoyaltyDiscountApplied = loyaltyApplied,
                PaymentStatus = invoice.PaymentStatus, VendorName = vendorName,
                StaffName = staffName, CustomerName = customerName,
                Items = invoice.InvoiceItems?.Select(ii =>
                {
                    var part = parts.FirstOrDefault(p => p.Id == ii.PartId);
                    return new InvoiceItemResponseDto
                    {
                        PartId = ii.PartId, PartName = part?.Name ?? "Unknown",
                        Quantity = ii.Quantity, UnitPrice = part?.UnitPrice ?? 0,
                        LineTotal = ii.Quantity * (part?.UnitPrice ?? 0)
                    };
                }).ToList() ?? new()
            };
        }
    }
}
