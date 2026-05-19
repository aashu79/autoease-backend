using System;
using System.Threading.Tasks;
using autoease_backend.Data.Models;
using autoease_backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace autoease_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IPurchaseInvoiceService _purchaseService;
        private readonly ISalesInvoiceService _salesService;
        private readonly autoease_backend.Data.AppDbContext _context;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(
            IPurchaseInvoiceService purchaseService, 
            ISalesInvoiceService salesService, 
            autoease_backend.Data.AppDbContext context,
            ILogger<InvoiceController> logger)
        {
            _purchaseService = purchaseService;
            _salesService = salesService;
            _context = context;
            _logger = logger;
        }

        [HttpPost("purchase")]
        public async Task<IActionResult> CreatePurchaseInvoice([FromBody] Invoice invoice)
        {
            try
            {
                var createdInvoice = await _purchaseService.CreatePurchaseInvoiceAsync(invoice);
                return Ok(createdInvoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create purchase invoice");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("sales")]
        public async Task<IActionResult> CreateSalesInvoice([FromBody] Invoice invoice)
        {
            try
            {
                var createdInvoice = await _salesService.CreateSalesInvoiceAsync(invoice);
                return Ok(createdInvoice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoices()
        {
            try
            {
                var invoices = await _context.Invoices
                    .Include(i => i.Customer)
                    .Include(i => i.InvoiceItems)
                    .OrderByDescending(i => i.InvoiceDate)
                    .Select(i => new
                    {
                        i.Id,
                        i.CustomerId,
                        Customer = i.Customer == null
                            ? null
                            : new
                            {
                                i.Customer.Id,
                                i.Customer.Name,
                                i.Customer.Email,
                                PhoneNumber = i.Customer.PhoneNumber,
                                i.Customer.Role
                            },
                        i.VendorId,
                        i.Type,
                        i.TotalAmount,
                        i.DiscountApplied,
                        i.PaymentStatus,
                        i.InvoiceDate,
                        i.DueDate,
                        InvoiceItems = i.InvoiceItems.Select(item => new
                        {
                            item.Id,
                            item.InvoiceId,
                            item.PartId,
                            item.Quantity
                        })
                    })
                    .ToListAsync();
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch invoices");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
