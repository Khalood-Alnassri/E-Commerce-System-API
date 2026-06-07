using E_Commerce_System;
using E_Commerce_System_API.DTOs;
using E_Commerce_System_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_System_API.Controllers
{
    [ApiController]
    [Route("api/Order")]
    public class OrderController : ControllerBase 
    {
        public ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // function to Get all orders for a user
        [HttpGet("GetUserOrders")]
        public IActionResult GetUserOrders()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Unauthorized("Please login first.");
            }

            var orders = _context.Orders
                                 .Where(o => o.UId == userId)
                                 .OrderByDescending(o => o.OrderDate)
                                 .Select(o => new GetOrdersDTO
                                               {
                                                  OId = o.OId,
                                                  OrderDate = o.OrderDate,
                                                  TotalAmount = o.TotalAmount
                                               })
                                 .ToList();

            if (!orders.Any())
            {
                return NotFound("No orders yet.");
            }

            return Ok(orders);
        }

        // function to get Order Detail
        [HttpGet("OrderDetail")]
        public IActionResult OrderDetail(int orderID)
        {
            var orders = _context.Orders.Include(o => o.OrderProducts)
                                        .ThenInclude(p => p.Product)
                                        .Where(o => o.OId == orderID)
                                        .Select(o => new OrderDetailsDTO
                                        {
                                            OId = o.OId,
                                            OrderDate = o.OrderDate,
                                            TotalAmount = o.TotalAmount,
                                            Products = o.OrderProducts
                                                        .Select(op => new OrderProductsDetailsDTO
                                                        {
                                                                     PName = op.Product.PName,
                                                                     Quantity = op.Quantity,
                                                                     Price = op.Product.Price
                                                                  })
                                                        .ToList()
                                        })
                                        .FirstOrDefault();

            if (orders == null)
            {
                return NotFound("order not found");
            }
             
            return Ok(orders);
        }

    }
}
