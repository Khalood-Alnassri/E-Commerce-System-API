using E_Commerce_System;
using E_Commerce_System_API.DTOs;
using E_Commerce_System_API.Models;
using E_Commerce_System_API.Serviece;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace E_Commerce_System_API.Controllers
{
    [ApiController]
    [Route("api/ProductReviews")]
    public class ProductReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly LoggingService _loggingService;

        public ProductReviewsController(ApplicationDbContext context, LoggingService logging)
        {
            _context = context;
            _loggingService = logging;
        }

        // function to add review 
        [Authorize]
        [HttpPost("AddReview")]
        public IActionResult AddReview(AddReviewDTO reviewDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                _loggingService.LogWarning("Unauthorized review attempt.");
                return Unauthorized("Please login first.");
            }

            if (!int.TryParse(userId, out int id))
            {
                return Unauthorized("Invalid user ID.");
            }

            // validate review data
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // search for Product
            Product product = _context.Products.Include(p => p.Reviews)
                                              .FirstOrDefault(p => p.PId == reviewDTO.PId);
            if (product == null)
            {
                _loggingService.LogWarning("Product " + reviewDTO.PId + " not found for review.");
                return NotFound("Product not found.");
            }

            // check if user purchased product
            bool hasPurchased = _context.OrderProducts
                                        .Any(op => op.PId == reviewDTO.PId &&
                                                   op.Order.UId == id);

            if (!hasPurchased)
            {
                _loggingService.LogWarning(
                    "User " + id +
                    " attempted to review a product not purchased."
                );

                return BadRequest(
                    "You can only review products you have purchased."
                );
            }

            // check if user already reviewed this product
            var existingReview = _context.Reviews
                                         .FirstOrDefault(r => r.PId == reviewDTO.PId && r.UId == id);

            if (existingReview != null)
            {
                _loggingService.LogWarning("User " + id + " attempted to review product " + reviewDTO.PId + " multiple times.");
                return BadRequest("You already reviewed this product.");
            }

            Review review = new Review
            {
                Rating = reviewDTO.Rating,
                Comment = reviewDTO.Comment,
                ReviewDate = DateTime.Now,
                PId = reviewDTO.PId,
                UId = id
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();

            // recalculate overall rating
            product.OverallRating = (decimal)_context.Reviews
                                                     .Where(r => r.PId == reviewDTO.PId)
                                                    .Average(r => r.Rating);
            _context.SaveChanges();

            _loggingService.LogInfo("Review added for product " + reviewDTO.PId + " by user " + id);
            return Ok("Review added successfully.");
        }

        // function to Delete review 
        [Authorize]
        [HttpDelete("DeleteReview")]
        public IActionResult DeleteReview(int reviewId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                _loggingService.LogWarning("Unauthorized review deletion attempt.");
                return Unauthorized("Please login first.");
            }

            if (!int.TryParse(userId, out int id))
            {
                return Unauthorized("Invalid user ID.");
            }

            // search all reviews for the user 
            var myReviews = _context.Reviews.Where(r => r.UId == id).ToList();

            if (!myReviews.Any())
            {
                _loggingService.LogWarning("No reviews found for user " + id + " to delete.");
                return NotFound("No reviews found.");
            }

            // take review id from the user
            var review = _context.Reviews.FirstOrDefault(r => r.RId == reviewId && r.UId == id);

            if (review == null)
            {
                _loggingService.LogWarning("Review " + reviewId + " not found for user " + id);
                return NotFound("Review not found.");
            }

            // search product with reviews
            var product = _context.Products
                                 .Include(p => p.Reviews)
                                 .FirstOrDefault(p => p.PId == review.PId);

            if (product == null)
            {
                _loggingService.LogWarning("Product " + review.PId + " not found for review deletion.");
                return NotFound("Product not found.");
            }

            // delete review 
            _context.Reviews.Remove(review);
            _context.SaveChanges();

            var remainingReviews = _context.Reviews
                                           .Where(r => r.PId == product.PId);

            if (remainingReviews.Any())
            {
                product.OverallRating = (decimal)remainingReviews.Average(r => r.Rating);
            }
            else
            {
                product.OverallRating = 0;
            }

            _context.SaveChanges();

            _loggingService.LogInfo("Review " + reviewId + " deleted for product " + review.PId + " by user " + id);
            return Ok("Review deleted successfully.");
        }

        // function to Get all reviews for product
        [AllowAnonymous]
        [HttpGet("ProductReviews")]
        public IActionResult ProductReviews(int productId)
        {
            // check product exists
            var product = _context.Products
                                 .FirstOrDefault(p => p.PId == productId);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            var reviews = _context.Reviews.Where(r => r.PId == productId)
                                         .Select(r => new ProductReviewsDTO
                                         {
                                             Rating = r.Rating,
                                             Comment = r.Comment,
                                             ReviewDate = r.ReviewDate
                                         })
                                         .ToList();
            if (!reviews.Any())
            {
                _loggingService.LogInfo("No reviews found for product " + productId);
                return NotFound("No reviews found for this product.");
            }
            _loggingService.LogInfo("Retrieved reviews for product " + productId);
            return Ok(reviews);
        }

    }
}
