using E_Commerce_System;
using E_Commerce_System_API.DTOs;
using E_Commerce_System_API.Models;
using E_Commerce_System_API.Serviece;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_Commerce_System_API.Controllers
{
    [ApiController]
    [Route("api/Product")]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly LoggingService _loggingService;

        private readonly Serviece.JwtService _jwtServiece;
        public ProductController(ApplicationDbContext context, Serviece.JwtService jwtServiece, LoggingService logging)
        {
            _context = context;
            _jwtServiece = jwtServiece;
            _loggingService = logging;
        }

        // function to Add a new product
        [Authorize(Roles = "Admin")]
        [HttpPost("AddProduct")]
        public IActionResult AddProduct(AddProductDTO productDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // check login
            if (userId == null)
            {
                _loggingService.LogWarning("Unauthorized access attempt to add product.");
                return Unauthorized("Please login first.");
            }

            // check admin role
            if (role != "Admin")
            {
                _loggingService.LogError("You do not have permission to perform this action.");
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

            _loggingService.LogInfo("Product added successfully with ID: " + proDto.PId);
            return Ok("Product added successfully.");
        }

        // function to Update product details
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateProduct")]
        public IActionResult UpdateProduct(UpdateProductDTO proDTO , int proId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // check login
            if (userId == null)
            {
                _loggingService.LogWarning("Unauthorized access attempt to update product.");
                return Unauthorized("Please login first.");
            }

            int id = int.Parse(userId);

            var product = _context.Products.FirstOrDefault(p => p.PId == proId);

            if (product == null)
            {
                _loggingService.LogWarning("Product with ID " + id + " not found.");
                return NotFound("Product not found.");
            }

            // update only required fields
            product.PName = proDTO.PName;
            product.Description = proDTO.Description;
            product.Price = proDTO.Price;
            product.Stock = proDTO.Stock;

            _context.SaveChanges();
            _loggingService.LogInfo("Product with ID " + product.PId + " updated successfully.");
            return Ok("Product update successfully, with ID: " + product.PId);
        }

        // function to Get a list of products
        [HttpGet("ListOfProducts")]
        public IActionResult ListOfProducts(int page = 1)
        {
            int pageSize = 10; // each page have 10 products

            var products = _context.Products.Select(p => new ListOfProductsDTO
                                            { PName = p.PName,
                                              Stock = p.Stock,
                                              Price = p.Price
                                            })
                                           .OrderBy(p => p.PName)
                                           .Skip((page - 1) * pageSize)
                                           .Take(pageSize)
                                           .ToList();

            if (!products.Any())
            {
                _loggingService.LogWarning("No products found.");
                return NotFound("No products found.");
            }

            return Ok(products);
        }

        // function to Get product details by ID
        [HttpGet("ProductDetail")]
        public IActionResult ProductDetail(int proId)
        {
            var product = _context.Products.FirstOrDefault(p => p.PId == proId);

            if (product == null)
            {
                _loggingService.LogWarning("Product with ID " + proId + " not found.");
                return NotFound("Product not found.");
            }

            return Ok(product);
        }

    }
}
