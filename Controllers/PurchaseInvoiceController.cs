using Microsoft.AspNetCore.Mvc;
using autoease_backend.DTOs;
using autoease_backend.Services;

namespace autoease_backend.Controllers
{
    [ApiController]
    [Route("api/invoices/purchase")]
    public class PurchaseInvoiceController : ControllerBase
    {
        private readonly IPurchaseInvoiceService _service;
        private readonly ILogger<PurchaseInvoiceController> _logger;

        public PurchaseInvoiceController(IPurchaseInvoiceService service, ILogger<PurchaseInvoiceController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // GET /api/invoices/purchase
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var invoices = await _service.GetAllAsync();
            return Ok(invoices);
        }

        // GET /api/invoices/purchase/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var invoice = await _service.GetByIdAsync(id);
            if (invoice == null)
                return NotFound(new { message = $"Purchase invoice #{id} not found." });
            return Ok(invoice);
        }

        // POST /api/invoices/purchase
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseInvoiceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchase invoice.");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
