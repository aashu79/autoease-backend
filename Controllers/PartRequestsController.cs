using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using autoease_backend.Data;
using autoease_backend.Data.Models;

namespace autoease_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PartRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PartRequestsController(AppDbContext context)
        {
            _context = context;
        }

        public class CreatePartRequestDto
        {
            public string PartName { get; set; } = string.Empty;
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RequestPart([FromBody] CreatePartRequestDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var pr = new PartRequest
            {
                CustomerId = userId,
                PartName = dto.PartName,
                Status = "Requested"
            };

            _context.PartRequests.Add(pr);
            await _context.SaveChangesAsync();

            return Ok(pr);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllRequests()
        {
            var requests = await _context.PartRequests
                .Include(r => r.Customer)
                .OrderByDescending(r => r.Id)
                .Select(r => new
                {
                    r.Id,
                    r.PartName,
                    r.Status,
                    r.CustomerId,
                    CustomerName = r.Customer != null ? r.Customer.Name : null,
                    CustomerEmail = r.Customer != null ? r.Customer.Email : null
                })
                .ToListAsync();

            return Ok(requests);
        }
    }
}
