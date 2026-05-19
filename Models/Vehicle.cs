using System;
using System.Collections.Generic;

namespace autoease_backend.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Model { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;

        public User? Customer { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<VehicleUsageLog>? UsageLogs { get; set; }
    }
}

