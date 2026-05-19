namespace autoease_backend.Models
{
    public class PartRequest
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public User? Customer { get; set; }
    }
}

