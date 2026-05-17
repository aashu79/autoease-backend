using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using autoease_backend.Data;
using autoease_backend.Data.Models;
using System.Security.Cryptography;
using System.Text;

namespace autoease_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("create-staff")]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Staff name is required.");

            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Staff email is required.");

            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Password is required.");

            if (string.IsNullOrWhiteSpace(request.Phone))
                return BadRequest("Phone number is required.");

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (emailExists)
                return BadRequest("A user with this email already exists.");

            var staff = new User
            {
                Name = request.Name,
                Email = request.Email,
                Password = HashPassword(request.Password),
                Phone = request.Phone,
                Role = "staff"
            };

            _context.Users.Add(staff);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Staff created successfully.",
                staff = new
                {
                    staff.Id,
                    staff.Name,
                    staff.Email,
                    staff.Phone,
                    staff.Role
                }
            });
        }

        [HttpGet("staff-list")]
        public async Task<IActionResult> GetAllStaff()
        {
            var staffList = await _context.Users
                .Where(u => u.Role == "staff")
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Phone,
                    u.Role
                })
                .ToListAsync();

            return Ok(staffList);
        }

        [HttpPut("update-role/{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Role))
                return BadRequest("Role is required.");

            var allowedRoles = new[] { "admin", "staff", "customer" };

            if (!allowedRoles.Contains(request.Role.ToLower()))
                return BadRequest("Invalid role. Allowed roles are admin, staff, or customer.");

            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound("User not found.");

            user.Role = request.Role.ToLower();
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User role updated successfully.",
                user = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Phone,
                    user.Role
                }
            });
        }

        [HttpDelete("delete-staff/{id}")]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            var staff = await _context.Users.FindAsync(id);

            if (staff == null)
                return NotFound("Staff not found.");

            if (staff.Role != "staff")
                return BadRequest("Only staff users can be deleted from this endpoint.");

            _context.Users.Remove(staff);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Staff deleted successfully." });
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }

    public class CreateStaffRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class UpdateRoleRequest
    {
        public string Role { get; set; } = string.Empty;
    }
}