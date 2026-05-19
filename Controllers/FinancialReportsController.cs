using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using autoease_backend.Services;

namespace autoease_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialReportsController : ControllerBase
    {
        private readonly IFinancialReportService _reportService;
        private readonly ILogger<FinancialReportsController> _logger;

        public FinancialReportsController(
            IFinancialReportService reportService,
            ILogger<FinancialReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        /// <summary>
        /// Get daily financial report for a specific date
        /// Admin only
        /// </summary>
        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyReport([FromQuery] DateTime date)
        {
            try
            {
                var report = await _reportService.GetDailyReportAsync(date);
                return Ok(new
                {
                    success = true,
                    data = report
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating daily report: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error generating daily report"
                });
            }
        }

        /// <summary>
        /// Get monthly financial report
        /// Admin only
        /// </summary>
        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlyReport([FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                if (month < 1 || month > 12)
                    return BadRequest(new { success = false, message = "Month must be between 1 and 12" });

                var report = await _reportService.GetMonthlyReportAsync(year, month);
                return Ok(new
                {
                    success = true,
                    data = report
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating monthly report: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error generating monthly report"
                });
            }
        }

        /// <summary>
        /// Get yearly financial report
        /// Admin only
        /// </summary>
        [HttpGet("yearly")]
        public async Task<IActionResult> GetYearlyReport([FromQuery] int year)
        {
            try
            {
                if (year < 2000 || year > DateTime.Now.Year)
                    return BadRequest(new { success = false, message = "Invalid year" });

                var report = await _reportService.GetYearlyReportAsync(year);
                return Ok(new
                {
                    success = true,
                    data = report
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating yearly report: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error generating yearly report"
                });
            }
        }
    }
}