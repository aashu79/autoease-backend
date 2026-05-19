using System;
using System.Collections.Generic;

namespace autoease_backend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public ICollection<Vehicle>? Vehicles { get; set; }
        public ICollection<Appointment>? CustomerAppointments { get; set; }
        public ICollection<Appointment>? StaffAppointments { get; set; }
        public ICollection<VehicleUsageLog>? VehicleUsageLogs { get; set; }
        public ICollection<PartRequest>? PartRequests { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<Invoice>? CustomerInvoices { get; set; }
        public ICollection<Invoice>? StaffInvoices { get; set; }
        public ICollection<Part>? RequestedParts { get; set; }
    }
}

