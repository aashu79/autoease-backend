using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using autoease_backend.Data;
using autoease_backend.Data.Models;

namespace autoease_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VendorController(AppDbContext context)
        {
            _context = context;
        }

        // CREATE Vendor
        [HttpPost("create")]
        public async Task<IActionResult> CreateVendor([FromBody] Vendor vendor)
        {
            if (vendor == null)
                return BadRequest("Invalid vendor data.");

            if (string.IsNullOrWhiteSpace(vendor.Name))
                return BadRequest("Vendor name is required.");

            if (string.IsNullOrWhiteSpace(vendor.Phone))
                return BadRequest("Vendor phone is required.");

            var exists = await _context.Vendors
                .AnyAsync(v => v.Name.ToLower() == vendor.Name.ToLower()
                            && v.Phone == vendor.Phone);

            if (exists)
                return BadRequest("Vendor already exists.");

            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Vendor created successfully.",
                vendor = new
                {
                    vendor.Id,
                    vendor.Name,
                    vendor.Phone
                }
            });
        }

        // READ All Vendors
        [HttpGet("list")]
        public async Task<IActionResult> GetAllVendors()
        {
            var vendors = await _context.Vendors
                .Select(v => new
                {
                    v.Id,
                    v.Name,
                    v.Phone
                })
                .ToListAsync();

            return Ok(vendors);
        }

        // READ Vendor By Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVendorById(int id)
        {
            var vendor = await _context.Vendors
                .Where(v => v.Id == id)
                .Select(v => new
                {
                    v.Id,
                    v.Name,
                    v.Phone
                })
                .FirstOrDefaultAsync();

            if (vendor == null)
                return NotFound("Vendor not found.");

            return Ok(vendor);
        }

        // UPDATE Vendor
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateVendor(int id, [FromBody] Vendor updatedVendor)
        {
            if (updatedVendor == null)
                return BadRequest("Invalid vendor data.");

            if (string.IsNullOrWhiteSpace(updatedVendor.Name))
                return BadRequest("Vendor name is required.");

            if (string.IsNullOrWhiteSpace(updatedVendor.Phone))
                return BadRequest("Vendor phone is required.");

            var vendor = await _context.Vendors.FindAsync(id);

            if (vendor == null)
                return NotFound("Vendor not found.");

            vendor.Name = updatedVendor.Name;
            vendor.Phone = updatedVendor.Phone;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Vendor updated successfully.",
                vendor = new
                {
                    vendor.Id,
                    vendor.Name,
                    vendor.Phone
                }
            });
        }

        // DELETE Vendor
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteVendor(int id)
        {
            var vendor = await _context.Vendors.FindAsync(id);

            if (vendor == null)
                return NotFound("Vendor not found.");

            _context.Vendors.Remove(vendor);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Vendor deleted successfully." });
        }
    }
}