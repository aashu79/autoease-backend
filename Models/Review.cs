namespace autoease_backend.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;

        public User? Customer { get; set; }
    }
}

