using System.ComponentModel.DataAnnotations;

namespace autoease_backend.Models.DTOs
{
    public class VehicleDto
    {
        [Required]
        public string Model { get; set; } = string.Empty;
        [Required]
        public string PlateNumber { get; set; } = string.Empty;
    }
}