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
    public class AppointmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AppointmentsController(AppDbContext context)
        {
            _context = context;
        }

        public class CreateAppointmentDto
        {
            public int VehicleId { get; set; }
            public int StaffId { get; set; }
            public DateTime ScheduledAt { get; set; }
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> BookAppointment([FromBody] CreateAppointmentDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var appointment = new Appointment
            {
                CustomerId = userId,
                VehicleId = dto.VehicleId,
                StaffId = dto.StaffId,
                ScheduledAt = dto.ScheduledAt,
                Status = "Pending"
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Ok(appointment);
        }

        [HttpGet("my-appointments")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var appointments = await _context.Appointments
                .Where(a => a.CustomerId == userId)
                .OrderBy(a => a.ScheduledAt)
                .ToListAsync();

            return Ok(appointments);
        }

        [HttpGet("customer/")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCustomerAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Vehicle)
                .Include(a => a.Staff)
                //.Where(a => a.CustomerId == customerId)
                .OrderByDescending(a => a.ScheduledAt)
                .Select(a => new
                {
                    a.Id,
                    a.CustomerId,
                    a.VehicleId,
                    VehicleModel = a.Vehicle != null ? a.Vehicle.Model : null,
                    VehiclePlateNumber = a.Vehicle != null ? a.Vehicle.PlateNumber : null,
                    a.StaffId,
                    StaffName = a.Staff != null ? a.Staff.Name : null,
                    a.ScheduledAt,
                    a.Status
                })
                .ToListAsync();

            return Ok(appointments);
        }
    }
}
