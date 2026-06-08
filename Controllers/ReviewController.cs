using E_Commerce_System;
using E_Commerce_System_API.DTOs;
using E_Commerce_System_API.Models;
using E_Commerce_System_API.Serviece;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_Commerce_System_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Review")]
    public class ReviewController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly LoggingService _loggingService;

        public ReviewController(ApplicationDbContext context, LoggingService logging)
        {
            _context = context;
            _loggingService = logging;
        }

        // function to update review
        [HttpPut("UpdateReview")]
        public IActionResult UpdateReview(int reviewId, UpdateReviewDTO reviewDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                _loggingService.LogWarning("Review " + reviewId + " not found for user " + userId);
                return Unauthorized("Please login first.");
            }

            if (!int.TryParse(userId, out int id))
            {
                return Unauthorized("Invalid user ID.");
            }

            // take review id from the user
            var review = _context.Reviews.FirstOrDefault(r => r.RId == reviewId && r.UId == id);

            if (review == null)
            {
                return NotFound("Review not found.");
            }

            if (reviewDTO.Rating.HasValue)
            {
                review.Rating = reviewDTO.Rating.Value;
            }

            if (!string.IsNullOrEmpty(reviewDTO.Comment))
            {
                review.Comment = reviewDTO.Comment;
            }

            _context.SaveChanges();
            _loggingService.LogInfo("User " + id + " updated review " + reviewId);
            return Ok("Review updated successfully.");
        }

    }
}
