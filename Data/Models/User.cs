using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace autoease_backend.Data.Models
{
    public class User : IdentityUser<int>
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public ICollection<Vehicle>? Vehicles { get; set; }
        public ICollection<Appointment>? CustomerAppointments { get; set; }
        public ICollection<Appointment>? StaffAppointments { get; set; }
        public ICollection<VehicleUsageLog>? VehicleUsageLogs { get; set; }
        public ICollection<PartRequest>? PartRequests { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<Invoice>? CustomerInvoices { get; set; }
    }
}
