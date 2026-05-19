using System;

namespace autoease_backend.Models
{
    public class VehicleUsageLog
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int CustomerId { get; set; }
        public DateTime LogDate { get; set; }
        public int Mileage { get; set; }
        public string ConditionNotes { get; set; } = string.Empty;

        public Vehicle? Vehicle { get; set; }
        public User? Customer { get; set; }
    }
}

