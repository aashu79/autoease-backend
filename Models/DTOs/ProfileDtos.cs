using System.ComponentModel.DataAnnotations;

namespace autoease_backend.Models.DTOs
{
    public class UpdateProfileDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}