using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using autoease_backend.Services;

namespace autoease_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceEmailService _invoiceEmailService;
        private readonly ILogger<InvoicesController> _logger;

        public InvoicesController(
            IInvoiceEmailService invoiceEmailService,
            ILogger<InvoicesController> logger)
        {
            _invoiceEmailService = invoiceEmailService;
            _logger = logger;
        }

        /// <summary>
        /// Send invoice to customer via email
        /// Staff can use this endpoint to send invoices to customers
        /// </summary>
        [HttpPost("{invoiceId}/send-email")]
        public async Task<IActionResult> SendInvoiceEmail(int invoiceId)
        {
            try
            {
                if (invoiceId <= 0)
                    return BadRequest(new { success = false, message = "Invalid invoice ID" });

                var result = await _invoiceEmailService.SendInvoiceAsync(invoiceId);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"Invoice {invoiceId} sent successfully"
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Failed to send invoice"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending invoice {invoiceId}: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error sending invoice"
                });
            }
        }

        /// <summary>
        /// Send invoice to a specific email address
        /// Staff can use this endpoint to send invoices to alternative email addresses
        /// </summary>
        [HttpPost("{invoiceId}/send-email-to")]
        public async Task<IActionResult> SendInvoiceEmailTo(int invoiceId, [FromBody] SendInvoiceEmailRequest request)
        {
            try
            {
                if (invoiceId <= 0)
                    return BadRequest(new { success = false, message = "Invalid invoice ID" });

                if (string.IsNullOrWhiteSpace(request?.Email))
                    return BadRequest(new { success = false, message = "Email address is required" });

                var result = await _invoiceEmailService.SendInvoiceToCustomerAsync(invoiceId, request.Email);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"Invoice {invoiceId} sent to {request.Email} successfully"
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Failed to send invoice"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending invoice {invoiceId}: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error sending invoice"
                });
            }
        }

        public class SendInvoiceEmailRequest
        {
            public string Email { get; set; } = string.Empty;
        }
    }
}