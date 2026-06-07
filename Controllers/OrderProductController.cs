using E_Commerce_System;
using E_Commerce_System_API.DTOs;
using E_Commerce_System_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_System_API.Controllers
{
    [ApiController]
    [Route("api/OrderProduct")]
    public class OrderProductController : ControllerBase
    {
        public ApplicationDbContext _context;

        public OrderProductController (ApplicationDbContext context)
        {
            _context = context;
        }

        // function to Place a new order
        [HttpPost("PlaceNewOrder")]
        public ActionResult PlaceNewOrder(CreateOrderDTO orderDTO)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Unauthorized("Please login first.");
            }

            if (orderDTO.Items == null || !orderDTO.Items.Any())
            {
                return BadRequest("Order must contain at least one item.");
            }

            decimal totalAmount = 0;

            // create order
            var order = new Order
            {
                UId = userId.Value,
                OrderDate = DateTime.Now
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            foreach (var itemsDTO in orderDTO.Items)
            {
                // search for product
                var product = _context.Products.FirstOrDefault(p => p.PId == itemsDTO.ProductId);

                if (product == null)
                {
                    return BadRequest("Invalid product.");
                }

                // check stock availability
                if (product.Stock < itemsDTO.Quantity)
                {
                    return BadRequest("Onle " + product.Stock + " items available.");
                }

                var orderProduct = new OrderProduct
                {
                    OId = order.OId,
                    PId = product.PId,
                    Quantity = itemsDTO.Quantity
                };

                _context.OrderProducts.Add(orderProduct); // add order in OrderProducts table

                // calculate the total amount 
                totalAmount += itemsDTO.Quantity * product.Price;

                // reduce product stock
                product.Stock -= itemsDTO.Quantity;
            }

            order .TotalAmount = totalAmount; // update total amount in order table
            _context.SaveChanges();
            return Ok( new
            {
                Message = "Order placed successfully.",
                orderId = order.OId,
                TotalAmount = totalAmount
            });

        }
    }
}
