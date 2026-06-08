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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = new Product
            {
                PName = productDto.PName,
                Description = productDto.Description,
                Price = productDto.Price,
                Stock = productDto.Stock
            };

            _context.Products.Add(product);
            _context.SaveChanges();

            _loggingService.LogInfo("Product added successfully with ID: " + product.PId);
            return Ok("Product added successfully.");
        }

        // function to Update product details
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateProduct")]
        public IActionResult UpdateProduct(UpdateProductDTO proDTO , int proId)
        {
            var product = _context.Products.FirstOrDefault(p => p.PId == proId);

            if (product == null)
            {
                _loggingService.LogWarning("Product with ID " + proId + " not found.");
                return NotFound("Product not found.");
            }

            // update only required fields
            if (!string.IsNullOrWhiteSpace(proDTO.PName))
            {
                product.PName = proDTO.PName;
            }

            if (!string.IsNullOrWhiteSpace(proDTO.Description))
            {
                product.Description = proDTO.Description;
            }

            if (proDTO.Price.HasValue)
            {
                product.Price = proDTO.Price.Value;
            }

            if (proDTO.Stock.HasValue)
            {
                product.Stock = proDTO.Stock.Value;
            }

            _context.SaveChanges();
            _loggingService.LogInfo("Product with ID " + product.PId + " updated successfully.");
            return Ok("Product update successfully, with ID: " + product.PId);
        }

        // function to Get a list of products
        [AllowAnonymous]
        [HttpGet("ListOfProducts")]
        public IActionResult ListOfProducts(FilteringProductDTO filtering, int page = 1)
        {
            int pageSize = 10; // each page have 10 products

            var products = _context.Products.Where(p => (string.IsNullOrWhiteSpace(filtering.PName) || p.PName.Contains(filtering.PName)) &&
                                                               (!filtering.MinPrice.HasValue || p.Price >= filtering.MinPrice.Value) &&
                                                               (!filtering.MaxPrice.HasValue || p.Price <= filtering.MaxPrice.Value))
                                            .OrderBy(p => p.PName)
                                            .Select(p => new ListOfProductsDTO
                                            { PName = p.PName,
                                              Stock = p.Stock,
                                              Price = p.Price
                                            })
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
        [Authorize(Roles = "Admin")]
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
