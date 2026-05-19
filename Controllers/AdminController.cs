using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using autoease_backend.Data;
using autoease_backend.Data.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace autoease_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
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
                UserName = request.Email,
                Email = request.Email,
                Name = request.Name,
                PhoneNumber = request.Phone,
                Role = "staff"
            };

            var result = await _userManager.CreateAsync(staff, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(staff);
            await _userManager.ConfirmEmailAsync(staff, token);

            // Return created staff info
            return Ok(new
            {
                message = "Staff created successfully.",
                staff = new
                {
                    staff.Id,
                    staff.Name,
                    staff.Email,
                    Phone = staff.PhoneNumber,
                    staff.Role
                }
            });
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersByRole([FromQuery] string role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return BadRequest("Role is required.");
            }

            var normalizedRole = role.Trim().ToLower();
            var users = await _context.Users
                .Where(u => u.Role.ToLower() == normalizedRole)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    Phone = u.PhoneNumber,
                    u.Role
                })
                .ToListAsync();

            return Ok(users);
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
                    Phone = u.PhoneNumber,
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
                    Phone = user.PhoneNumber,
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