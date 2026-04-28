using Microsoft.AspNetCore.Mvc;
using autoease_backend.DTOs;
using autoease_backend.Services;

namespace autoease_backend.Controllers
{
    [ApiController]
    [Route("api/invoices/sales")]
    public class SalesInvoiceController : ControllerBase
    {
        private readonly ISalesInvoiceService _service;
        private readonly ILogger<SalesInvoiceController> _logger;

        public SalesInvoiceController(ISalesInvoiceService service, ILogger<SalesInvoiceController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // GET /api/invoices/sales
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var invoices = await _service.GetAllAsync();
            return Ok(invoices);
        }

        // GET /api/invoices/sales/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var invoice = await _service.GetByIdAsync(id);
            if (invoice == null)
                return NotFound(new { message = $"Sales invoice #{id} not found." });
            return Ok(invoice);
        }

        // GET /api/invoices/sales/customer/{customerId}
        [HttpGet("customer/{customerId:int}")]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            var invoices = await _service.GetByCustomerIdAsync(customerId);
            return Ok(invoices);
        }

        // POST /api/invoices/sales
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSalesInvoiceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sales invoice.");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
