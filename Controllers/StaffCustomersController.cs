using autoease_backend.Contracts.Customers;
using autoease_backend.Data;
using autoease_backend.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace autoease_backend.Controllers
{
    [ApiController]
    [Route("api/staff/customers")]
    public class StaffCustomersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public StaffCustomersController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<ActionResult<CustomerDetailsDto>> RegisterCustomer([FromBody] RegisterCustomerRequest request)
        {
            var normalizedEmail = request.Email.Trim();
            var normalizedPhone = request.Phone.Trim();
            var normalizedPlate = request.PlateNumber.Trim();

            var emailExists = await _context.Users.AnyAsync(u => u.Email == normalizedEmail);
            if (emailExists)
            {
                return Conflict($"A customer with email '{normalizedEmail}' already exists.");
            }

            var phoneExists = await _context.Users.AnyAsync(u => u.PhoneNumber == normalizedPhone);
            if (phoneExists)
            {
                return Conflict($"A customer with phone '{normalizedPhone}' already exists.");
            }

            var plateExists = await _context.Vehicles.AnyAsync(v => v.PlateNumber == normalizedPlate);
            if (plateExists)
            {
                return Conflict($"A vehicle with plate number '{normalizedPlate}' already exists.");
            }

            var customer = new User
            {
                UserName = normalizedEmail,
                Email = normalizedEmail,
                Name = request.Name.Trim(),
                PhoneNumber = normalizedPhone,
                Role = "Customer"
            };

            var createResult = await _userManager.CreateAsync(customer, request.Password);
            if (!createResult.Succeeded)
            {
                return BadRequest(createResult.Errors);
            }

            // Create vehicle record linked to the newly created user
            var vehicle = new Vehicle
            {
                Model = request.VehicleModel.Trim(),
                PlateNumber = normalizedPlate,
                CustomerId = customer.Id
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomerDetails), new { customerId = customer.Id }, await BuildCustomerDetailsAsync(customer.Id));
        }

        [HttpGet("{customerId:int}")]
        public async Task<ActionResult<CustomerDetailsDto>> GetCustomerDetails(int customerId)
        {
            var details = await BuildCustomerDetailsAsync(customerId);
            if (details == null)
            {
                return NotFound();
            }

            return Ok(details);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CustomerSearchResultDto>>> SearchCustomers([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query is required.");
            }

            var term = query.Trim().ToLower();
            var customersQuery = _context.Users
                .AsNoTracking()
                .Include(u => u.Vehicles)
                .Where(u => u.Role == "Customer")
                .AsQueryable();

            if (int.TryParse(term, out var customerId))
            {
                customersQuery = customersQuery.Where(u => u.Id == customerId ||
                    u.Name.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term) ||
                    (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(term)) ||
                    u.Vehicles!.Any(v => v.PlateNumber.ToLower().Contains(term)));
            }
            else
            {
                customersQuery = customersQuery.Where(u =>
                    u.Name.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term) ||
                    (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(term)) ||
                    u.Vehicles!.Any(v => v.PlateNumber.ToLower().Contains(term)));
            }

            var customers = await customersQuery.ToListAsync();
            var results = customers.Select(customer => new CustomerSearchResultDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.PhoneNumber ?? string.Empty,
                Vehicles = customer.Vehicles?
                    .Select(vehicle => new CustomerVehicleDto
                    {
                        Id = vehicle.Id,
                        Model = vehicle.Model,
                        PlateNumber = vehicle.PlateNumber
                    })
                    .ToList() ?? new List<CustomerVehicleDto>()
            })
            .ToList();

            return Ok(results);
        }

        private async Task<CustomerDetailsDto?> BuildCustomerDetailsAsync(int customerId)
        {
            var customer = await _context.Users
                .AsNoTracking()
                .Include(u => u.Vehicles)
                .FirstOrDefaultAsync(u => u.Id == customerId && u.Role == "Customer");

            if (customer == null)
            {
                return null;
            }

            var appointments = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Vehicle)
                .Include(a => a.Staff)
                .Where(a => a.CustomerId == customerId)
                .OrderByDescending(a => a.ScheduledAt)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    VehicleId = a.VehicleId,
                    VehicleModel = a.Vehicle != null ? a.Vehicle.Model : null,
                    VehiclePlateNumber = a.Vehicle != null ? a.Vehicle.PlateNumber : null,
                    StaffId = a.StaffId,
                    StaffName = a.Staff != null ? a.Staff.Name : null,
                    ScheduledAt = a.ScheduledAt,
                    Status = a.Status
                })
                .ToListAsync();

            var invoices = await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Vendor)
                .Where(i => i.CustomerId == customerId)
                .OrderByDescending(i => i.InvoiceDate)
                .Select(i => new InvoiceDto
                {
                    Id = i.Id,
                    VendorId = i.VendorId,
                    VendorName = i.Vendor != null ? i.Vendor.Name : null,
                    Type = i.Type,
                    TotalAmount = i.TotalAmount,
                    DiscountApplied = i.DiscountApplied,
                    PaymentStatus = i.PaymentStatus,
                    InvoiceDate = i.InvoiceDate,
                    DueDate = i.DueDate
                })
                .ToListAsync();

            var vehicleUsageLogs = await _context.VehicleUsageLogs
                .AsNoTracking()
                .Include(l => l.Vehicle)
                .Where(l => l.CustomerId == customerId)
                .OrderByDescending(l => l.LogDate)
                .Select(l => new VehicleUsageLogDto
                {
                    Id = l.Id,
                    VehicleId = l.VehicleId,
                    VehicleModel = l.Vehicle != null ? l.Vehicle.Model : null,
                    VehiclePlateNumber = l.Vehicle != null ? l.Vehicle.PlateNumber : null,
                    LogDate = l.LogDate,
                    Mileage = l.Mileage,
                    ConditionNotes = l.ConditionNotes
                })
                .ToListAsync();

            var partRequests = await _context.PartRequests
                .AsNoTracking()
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.Id)
                .Select(r => new PartRequestDto
                {
                    Id = r.Id,
                    PartName = r.PartName,
                    Status = r.Status
                })
                .ToListAsync();

            var reviews = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.Id)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment
                })
                .ToListAsync();

            return new CustomerDetailsDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.PhoneNumber ?? string.Empty,
                Role = customer.Role,
                Vehicles = customer.Vehicles?
                    .Select(vehicle => new CustomerVehicleDto
                    {
                        Id = vehicle.Id,
                        Model = vehicle.Model,
                        PlateNumber = vehicle.PlateNumber
                    })
                    .ToList() ?? new List<CustomerVehicleDto>(),
                Appointments = appointments,
                Invoices = invoices,
                VehicleUsageLogs = vehicleUsageLogs,
                PartRequests = partRequests,
                Reviews = reviews
            };
        }
    }
}
