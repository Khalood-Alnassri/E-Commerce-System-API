using E_Commerce_System;
using E_Commerce_System_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_System_API.Controllers
{
    [ApiController]
    [Route("api/Order")]
    public class OrderController : ControllerBase 
    {
        ApplicationDbContext context = new ApplicationDbContext();

        // function to Get all orders for a user
        [HttpGet("GetUserOrders")]
        public IActionResult GetUserOrders()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Unauthorized("Please login first.");
            }

            var orders = context.Orders.Where(o => o.UId == userId)
                                    .OrderByDescending(o => o.OrderDate)
                                    .ToList();
            if (!orders.Any())
            {
                return NotFound("No orders yet.");
            }

            for (int i = 0; i < orders.Count; i++)
            {
                return Ok(orders[i]);
            }

            return BadRequest();
        }

        // function to get Order Detail
        [HttpGet("OrderDetail")]
        public IActionResult OrderDetail(int orderID)
        {
            var orders = context.Orders.Include(o => o.OrderProducts)
                                        .ThenInclude(p => p.Product)
                                        .FirstOrDefault(o => o.OId == orderID);

            if (orders == null)
            {
                return NotFound("order not found");
            }
             
            return Ok(orders);
        }

    }
}
