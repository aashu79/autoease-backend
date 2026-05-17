using autoease_backend.Data;
using autoease_backend.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace autoease_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PartsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/parts
        [HttpGet]
        public async Task<IActionResult> GetParts()
        {
            var parts = await _context.Parts
                .Select(p => new
                {
                    p.Id,
                    p.VendorId,
                    p.RequestedBy,
                    p.Name,
                    p.UnitPrice,
                    p.StockQuantity,
                    p.RequestStatus,
                    p.RequestDescription
                })
                .ToListAsync();

            return Ok(parts);
        }

        // GET: api/parts/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPartById(int id)
        {
            var part = await _context.Parts
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.VendorId,
                    p.RequestedBy,
                    p.Name,
                    p.UnitPrice,
                    p.StockQuantity,
                    p.RequestStatus,
                    p.RequestDescription
                })
                .FirstOrDefaultAsync();

            if (part == null)
                return NotFound("Part not found.");

            return Ok(part);
        }

        // POST: api/parts
        [HttpPost]
        public async Task<ActionResult<Part>> CreatePart(Part part)
        {
            if (string.IsNullOrWhiteSpace(part.Name))
                return BadRequest("Part name is required.");

            if (part.UnitPrice <= 0)
                return BadRequest("Unit price must be greater than zero.");

            if (part.StockQuantity < 0)
                return BadRequest("Stock quantity cannot be negative.");

            _context.Parts.Add(part);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPartById), new { id = part.Id }, part);
        }

        // PUT: api/parts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePart(int id, Part updatedPart)
        {
            var existingPart = await _context.Parts.FindAsync(id);

            if (existingPart == null)
                return NotFound("Part not found.");

            if (string.IsNullOrWhiteSpace(updatedPart.Name))
                return BadRequest("Part name is required.");

            if (updatedPart.UnitPrice <= 0)
                return BadRequest("Unit price must be greater than zero.");

            if (updatedPart.StockQuantity < 0)
                return BadRequest("Stock quantity cannot be negative.");

            existingPart.VendorId = updatedPart.VendorId;
            existingPart.RequestedBy = updatedPart.RequestedBy;
            existingPart.Name = updatedPart.Name;
            existingPart.UnitPrice = updatedPart.UnitPrice;
            existingPart.StockQuantity = updatedPart.StockQuantity;
            existingPart.RequestStatus = updatedPart.RequestStatus;
            existingPart.RequestDescription = updatedPart.RequestDescription;

            await _context.SaveChangesAsync();

            return Ok("Part updated successfully.");
        }

        // DELETE: api/parts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePart(int id)
        {
            var part = await _context.Parts.FindAsync(id);

            if (part == null)
                return NotFound("Part not found.");

            _context.Parts.Remove(part);
            await _context.SaveChangesAsync();

            return Ok("Part deleted successfully.");
        }
    }
}