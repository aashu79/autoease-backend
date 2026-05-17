using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using autoease_backend.Data;
using autoease_backend.Data.Models;
using autoease_backend.Models.DTOs;

namespace autoease_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VehiclesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public VehiclesController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyVehicles()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found.");

            var vehicles = await _context.Vehicles
                .Where(v => v.CustomerId == user.Id)
                .Select(v => new { v.Id, v.Model, v.PlateNumber })
                .ToListAsync();

            return Ok(vehicles);
        }

        [HttpPost]
        public async Task<IActionResult> AddVehicle([FromBody] VehicleDto model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found.");

            var vehicle = new Vehicle
            {
                CustomerId = user.Id,
                Model = model.Model,
                PlateNumber = model.PlateNumber
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Vehicle added successfully.", vehicle.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] VehicleDto model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found.");

            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id && v.CustomerId == user.Id);
            if (vehicle == null) return NotFound("Vehicle not found or you do not have permission.");

            vehicle.Model = model.Model;
            vehicle.PlateNumber = model.PlateNumber;

            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Vehicle updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found.");

            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id && v.CustomerId == user.Id);
            if (vehicle == null) return NotFound("Vehicle not found or you do not have permission.");

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Vehicle removed successfully." });
        }
    }
}