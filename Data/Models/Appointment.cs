using System;

namespace autoease_backend.Data.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public int StaffId { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string Status { get; set; } = string.Empty;

        public User? Customer { get; set; }
        public User? Staff { get; set; }
        public Vehicle? Vehicle { get; set; }
    }
}
