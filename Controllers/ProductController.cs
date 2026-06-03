using E_Commerce_System;
using E_Commerce_System_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_System_API.Controllers
{
    [ApiController]
    [Route("api/Product")]
    public class ProductController : ControllerBase
    {
        ApplicationDbContext context = new ApplicationDbContext();

        // function to Add a new product
        [HttpPost("AddProduct")]
        public IActionResult AddProduct(Product p)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string role = HttpContext.Session.GetString("Role");

            // check login
            if (userId == null)
            {
                return Unauthorized("Please login first.");
            }

            // check user exists
            var user = context.Users.Find(userId);

            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            // check admin role
            if (role != "Admin")
            {
                return Forbid("You do not have permission to perform this action.");
            }

            context.Products.Add(p);
            context.SaveChanges();
            return Ok("Product added successfully.");
        }

        // function to Update product details
        [HttpPut("UpdateProduct")]
        public IActionResult UpdateProduct(Product p)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string role = HttpContext.Session.GetString("Role");

            // check login
            if (userId == null)
            {
                return Unauthorized("Please login first.");
            }

            var user = context.Users.Find(userId);

            if (user == null || role != "Admin")
            {
                return Forbid("You do not have permission to perform this action.");
            }

            // check product exists
            var product = context.Products.Find(p.PId);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            // update only required fields

            product.PName = p.PName;
            product.Description = p.Description;
            product.Price = p.Price;
            product.Stock = p.Stock;

            context.SaveChanges();
            return Ok("Product update successfully.");
        }

        // function to Get a list of products
        [HttpGet("ListOfProducts")]
        public IActionResult ListOfProducts(int page = 1)
        {
            int pageSize = 10; // each page have 10 products

            var products = context.Products.Select(p => new { p.PName, p.Stock, p.Price })
                                           .OrderBy(p => p.PName)
                                           .Skip((page - 1) * pageSize)
                                           .Take(pageSize).ToList();

            if (!products.Any())
            {
                return NotFound("No products found.");
            }

            return Ok(products);
        }

        // function to Get product details by ID
        [HttpGet("ProductDetail")]
        public IActionResult ProductDetail(int proId)
        {
            var prod = context.Products.ToList();

            var product = context.Products.Select(p => new { p.PId, p.PName, p.Price, p.Description, p.Stock })
                                          .FirstOrDefault(p => p.PId == proId);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            return Ok(product);
        }

    }
}
