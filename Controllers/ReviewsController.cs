using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using autoease_backend.Data;
using autoease_backend.Data.Models;

namespace autoease_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        public class CreateReviewDto
        {
            public int Rating { get; set; }
            public string Comment { get; set; } = string.Empty;
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var review = new Review
            {
                CustomerId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(review);
        }

        // GET: api/reviews/my-reviews
        [HttpGet("my-reviews")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyReviews()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var reviews = await _context.Reviews
                .Where(r => r.CustomerId == userId)
                .ToListAsync();

            return Ok(reviews);
        }

        // GET: api/reviews/all (admin)
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReviews()
        {
            var reviews = await _context.Reviews.ToListAsync();
            return Ok(reviews);
        }
    }
}
