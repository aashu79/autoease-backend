using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using autoease_backend.Services;

namespace autoease_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerReportsController : ControllerBase
    {
        private readonly ICustomerReportService _reportService;
        private readonly ILogger<CustomerReportsController> _logger;

        public CustomerReportsController(
            ICustomerReportService reportService,
            ILogger<CustomerReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        /// <summary>
        /// Get list of regular customers (3+ appointments or 2+ invoices)
        /// Staff can use this report
        /// </summary>
        [HttpGet("regular-customers")]
        public async Task<IActionResult> GetRegularCustomersReport()
        {
            try
            {
                var report = await _reportService.GetRegularCustomersReportAsync();
                return Ok(new
                {
                    success = true,
                    count = report.Count,
                    data = report
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating regular customers report: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error generating regular customers report"
                });
            }
        }

        /// <summary>
        /// Get list of high spenders (customers who spent more than threshold)
        /// Staff can use this report
        /// </summary>
        [HttpGet("high-spenders")]
        public async Task<IActionResult> GetHighSpendersReport([FromQuery] decimal minAmount = 5000)
        {
            try
            {
                if (minAmount <= 0)
                    return BadRequest(new { success = false, message = "Minimum amount must be greater than 0" });

                var report = await _reportService.GetHighSpendersReportAsync(minAmount);
                return Ok(new
                {
                    success = true,
                    count = report.Count,
                    minAmount = minAmount,
                    data = report
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating high spenders report: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error generating high spenders report"
                });
            }
        }

        /// <summary>
        /// Get list of customers with pending credits
        /// Staff can use this report to follow up on payments
        /// </summary>
        [HttpGet("pending-credits")]
        public async Task<IActionResult> GetPendingCreditsReport()
        {
            try
            {
                var report = await _reportService.GetPendingCreditsReportAsync();
                return Ok(new
                {
                    success = true,
                    count = report.Count,
                    totalPendingAmount = report.Sum(r => r.PendingAmount),
                    data = report
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating pending credits report: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error generating pending credits report"
                });
            }
        }
    }
}