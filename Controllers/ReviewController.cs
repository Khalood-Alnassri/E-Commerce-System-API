using E_Commerce_System;
using E_Commerce_System_API.DTOs;
using E_Commerce_System_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_System_API.Controllers
{
    [ApiController]
    [Route("api/Review")]
    public class ReviewController : ControllerBase
    {
        public ApplicationDbContext _context;

        public ReviewController (ApplicationDbContext context)
        {
            _context = context;
        }

        // function to update review
        [HttpPut("UpdateReview")]
        public IActionResult UpdateReview(int reviewId, UpdateReviewDTO reviewDTO)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Unauthorized("Please login first.");
            }

            // take review id from the user
            var review = _context.Reviews.FirstOrDefault(r => r.RId == reviewId && r.UId == userId);

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
            return Ok("Review updated successfully.");
        }

    }
}
