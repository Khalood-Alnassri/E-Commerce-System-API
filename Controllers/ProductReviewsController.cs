using E_Commerce_System;
using E_Commerce_System_API.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_System_API.Controllers
{
    [ApiController]
    [Route("api/ProductReviews")]
    public class ProductReviewsController : ControllerBase
    {
        public ApplicationDbContext _context;

        public ProductReviewsController (ApplicationDbContext context)
        {
            _context = context;
        }

        // function to add review 
        [HttpGet("AddReview")]
        public IActionResult AddReview(Review r, int productId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Unauthorized("Please login first.");
            }

            // search for Product
            Product product = _context.Products.Include(p => p.Reviews)
                                              .FirstOrDefault(p => p.PId == productId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            _context.Reviews.Add(r);
            _context.SaveChanges();

            // recalculate overall rating
            product.OverallRating = (decimal)product.Reviews
                                                    .Append(r)
                                                    .Average(r => r.Rating);
            _context.SaveChanges();

            return Ok("Review added successfully.");
        }

        // function to Delete review 
        [HttpDelete("DeleteReview")]
        public IActionResult DeleteReview(int reviewId)
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

            // search product with reviews
            var product = _context.Products
                                 .Include(p => p.Reviews)
                                 .FirstOrDefault(p => p.PId == review.PId);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            // delete review 
            _context.Reviews.Remove(review);
            _context.SaveChanges();

            // recalculate overall rating
            if (product.Reviews.Count > 1)
            {
                product.OverallRating = (decimal)product.Reviews
                                                        .Where(r => r.RId != reviewId)
                                                        .Average(r => r.Rating);
            }

            else
            {
                product.OverallRating = 0;
            }

            _context.SaveChanges();

            return Ok("Review deleted successfully.");
        }

        // function to Get all reviews for product
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
                                         .Select(r => new { r.Rating, r.Comment, r.ReviewDate })
                                         .ToList();
            if (!reviews.Any())
            {
                return NotFound("No reviews found for this product.");
            }
            return Ok(reviews);
        }

    }
}
