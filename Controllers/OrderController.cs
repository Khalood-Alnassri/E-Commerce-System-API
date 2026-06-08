using E_Commerce_System;
using E_Commerce_System_API.DTOs;
using E_Commerce_System_API.Serviece;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace E_Commerce_System_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Order")]
    public class OrderController : ControllerBase 
    {
        private readonly ApplicationDbContext _context;

        private readonly LoggingService _loggingService;

        public OrderController(ApplicationDbContext context, LoggingService logging)
        {
            _context = context;
            _loggingService = logging;
        }


        // function to Get all orders for a user
        [HttpGet("GetUserOrders")]
        public IActionResult GetUserOrders()
        {
            int id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var orders = _context.Orders
                                 .Where(o => o.UId == id)
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
                _loggingService.LogInfo("User with ID " + id + " has no orders.");
                return Ok(new List<GetOrdersDTO>()); // Return an empty list instead of NotFound
            }

            return Ok(orders);
        }

        // function to get Order Detail
        [HttpGet("OrderDetail")]
        public IActionResult OrderDetail(int orderID)
        {
            int id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var order = _context.Orders.Include(o => o.OrderProducts)
                                        .ThenInclude(p => p.Product)
                                        .Where(o => o.OId == orderID && o.UId == id)
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

            if (order == null)
            {
                _loggingService.LogInfo("Order with ID " + orderID + " not found.");
                return Ok(new List<GetOrdersDTO>()); // Return an empty list instead of NotFound
            }
             
            return Ok(order);
        }

    }
}
