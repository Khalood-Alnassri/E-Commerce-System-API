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
    [Route("api/OrderProduct")]
    public class OrderProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly LoggingService _loggingService;

        public OrderProductController(ApplicationDbContext context, LoggingService logging)
        {
            _context = context;
            _loggingService = logging;
        }

        // function to Place a new order
        [HttpPost("PlaceNewOrder")]
        public ActionResult PlaceNewOrder(CreateOrderDTO orderDTO)
        {
            var userClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userClaim))
            {
                _loggingService.LogWarning("Unauthorized access attempt to place an order.");
                return Unauthorized("Please login first.");
            }

            int userId = int.Parse(userClaim);
            if (orderDTO.Items == null || !orderDTO.Items.Any())
            {
                _loggingService.LogWarning("User " + userId + " attempted to place an order with no items.");
                return BadRequest("Order must contain at least one item.");
            }

            //merge duplicate products
            var items = orderDTO.Items
                   .GroupBy(i => i.ProductId)
                   .Select(g => new
                   {
                       ProductId = g.Key,
                       Quantity = g.Sum(x => x.Quantity)
                   })
                   .ToList();

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                decimal totalAmount = 0;

                var validatedProducts = new List<(Product Product, int Quantity)>();

                foreach (var item in items)
                {
                    if (item.Quantity <= 0)
                    {
                        _loggingService.LogWarning("User " + userId + " attempted to place an order with invalid quantity for product " + item.ProductId);
                        return BadRequest("Quantity must be greater than zero.");
                    }

                    var product = _context.Products
                                          .Find(item.ProductId);

                    if (product == null)
                    {
                        _loggingService.LogWarning("Product " + item.ProductId + " not found.");
                        return BadRequest("Invalid product.");
                    }

                    if (product.Stock < item.Quantity)
                    {
                        return BadRequest("Only " + product.Stock + " available for " + product.PName);
                    }

                    validatedProducts.Add((product, item.Quantity));
                }

                // create order
                var order = new Order
                {
                    UId = userId,
                    OrderDate = DateTime.Now
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                // create order-product relationship
                foreach (var item in validatedProducts)
                {
                    var orderProduct = new OrderProduct
                    {
                        OId = order.OId,
                        PId = item.Product.PId,
                        Quantity = item.Quantity
                    };

                    _context.OrderProducts.Add(orderProduct); // add order in OrderProducts table

                    // calculate the total amount 
                    totalAmount += item.Quantity * item.Product.Price;

                    // reduce product stock
                    item.Product.Stock -= item.Quantity;
                }

                order.TotalAmount = totalAmount; // update total amount in order table
                _context.SaveChanges();

                transaction.Commit();

                _loggingService.LogInfo("User " + userId + " placed an order with ID " + order.OId + " and total amount " + totalAmount);
                return Ok(new
                {
                    Message = "Order placed successfully.",
                    orderId = order.OId,
                    TotalAmount = totalAmount
                });
            }

            catch (Exception ex)
            {
                transaction.Rollback();
                _loggingService.LogError("User " + userId + " failed to place an order. Error: " + ex.Message);
                return StatusCode(500, "An error occurred while placing the order.");
            }
        }
    }
}
