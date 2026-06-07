using E_Commerce_System;
using E_Commerce_System_API.DTOs;
using E_Commerce_System_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_System_API.Controllers
{
    [ApiController]
    [Route("api/Product")]
    public class ProductController : ControllerBase
    {
        public ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // function to Add a new product
        [HttpPost("AddProduct")]
        public IActionResult AddProduct(AddProductDTO productDto)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string role = HttpContext.Session.GetString("Role");

            // check login
            if (userId == null)
            {
                return Unauthorized("Please login first.");
            }

            // check user exists
            var user = _context.Users.Find(userId);

            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            // check admin role
            if (role != "Admin")
            {
                return Forbid("You do not have permission to perform this action.");
            }

            var proDto = new Product
            {
                PName = productDto.PName,
                Description = productDto.Description,
                Price = productDto.Price,
                Stock = productDto.Stock
            };

            _context.Products.Add(proDto);
            _context.SaveChanges();
            return Ok("Product added successfully.");
        }

        // function to Update product details
        [HttpPut("UpdateProduct")]
        public IActionResult UpdateProduct(UpdateProductDTO proDTO , int p)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string role = HttpContext.Session.GetString("Role");

            // check login
            if (userId == null)
            {
                return Unauthorized("Please login first.");
            }

            var user = _context.Users.Find(userId);

            if (user == null || role != "Admin")
            {
                return Forbid("You do not have permission to perform this action.");
            }

            // check product exists
            var product = _context.Products.Find(p);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            // update only required fields

            product.PName = proDTO.PName;
            product.Description = proDTO.Description;
            product.Price = proDTO.Price;
            product.Stock = proDTO.Stock;

            _context.SaveChanges();
            return Ok("Product update successfully, with ID: " + product.PId);
        }

        // function to Get a list of products
        [HttpGet("ListOfProducts")]
        public IActionResult ListOfProducts(ListOfProductsDTO productsDTO, int page = 1)
        {
            int pageSize = 10; // each page have 10 products

            var products = _context.Products.Select(p => new ListOfProductsDTO
                                            { PName = productsDTO.PName,
                                              Stock = productsDTO.Stock,
                                              Price = productsDTO.Price
                                            })
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
        public IActionResult ProductDetail(ProductDetailsDTO proDTO)
        {
            var prod = _context.Products.ToList();

            var product = _context.Products.FirstOrDefault(p => p.PId == proDTO.PId);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            return Ok(product);
        }

    }
}
