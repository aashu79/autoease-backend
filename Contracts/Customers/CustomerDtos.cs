using System.ComponentModel.DataAnnotations;

namespace autoease_backend.Contracts.Customers
{
    public class RegisterCustomerRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string VehicleModel { get; set; } = string.Empty;

        [Required]
        public string PlateNumber { get; set; } = string.Empty;
    }

    public class CustomerVehicleDto
    {
        public int Id { get; set; }
        public string Model { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
    }

    public class AppointmentDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string? VehicleModel { get; set; }
        public string? VehiclePlateNumber { get; set; }
        public int StaffId { get; set; }
        public string? StaffName { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class InvoiceDto
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public string? VendorName { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal DiscountApplied { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class VehicleUsageLogDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string? VehicleModel { get; set; }
        public string? VehiclePlateNumber { get; set; }
        public DateTime LogDate { get; set; }
        public int Mileage { get; set; }
        public string ConditionNotes { get; set; } = string.Empty;
    }

    public class PartRequestDto
    {
        public int Id { get; set; }
        public string PartName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class ReviewDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class CustomerSearchResultDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public List<CustomerVehicleDto> Vehicles { get; set; } = new();
    }

    public class CustomerDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public List<CustomerVehicleDto> Vehicles { get; set; } = new();
        public List<AppointmentDto> Appointments { get; set; } = new();
        public List<InvoiceDto> Invoices { get; set; } = new();
        public List<VehicleUsageLogDto> VehicleUsageLogs { get; set; } = new();
        public List<PartRequestDto> PartRequests { get; set; } = new();
        public List<ReviewDto> Reviews { get; set; } = new();
    }
}
