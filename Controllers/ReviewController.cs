using E_Commerce_System;
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
        public IActionResult UpdateReview(int reviewId, string newComment, int newRating)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Unauthorized("Please login first.");
            }

            // search all reviews for the user 
            var myReviews = _context.Reviews.Where(r => r.UId == userId).ToList();

            if (!myReviews.Any())
            {
                return NotFound("No reviews found.");
            }

            // take review id from the user
            var review = _context.Reviews.FirstOrDefault(r => r.RId == reviewId && r.UId == userId);

            if (review == null)
            {
                return NotFound("Review not found.");
            }

            if (string.IsNullOrWhiteSpace(newComment))
            {
                return BadRequest("Comment is required.");
            }

            if (newRating < 1 || newRating > 5)
            {
                return BadRequest("Rating must be between 1 and 5.");
            }

            review.Comment = newComment;
            review.Rating = newRating;

            _context.SaveChanges();
            return Ok("Review updated successfully.");
        }

    }
}
