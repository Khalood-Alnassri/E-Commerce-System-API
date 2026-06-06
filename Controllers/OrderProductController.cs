using E_Commerce_System;
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
        public ActionResult PlaceNewOrder(int productId, int qty)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Unauthorized("Please login first.");
            }

            // search for product
            var product = _context.Products.FirstOrDefault(p => p.PId == productId);

            if (product == null)
            {
                return BadRequest("Invalid product.");
            }

            // check quantity
            if (qty <= 0)
            {
                return BadRequest("Quantity must be greater than zero.");
            } 

            // check product stock
            if (product.Stock < qty)
            {
                return BadRequest($"Cannot place order. Only {product.Stock} items available in stock.");
            }

            // create order
            var order = new Order
            {
                UId = userId.Value,
                OrderDate = DateTime.Now
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            // create relation
            var orderProduct = new OrderProduct
            {
                OId = order.OId,
                PId = product.PId,
                Quantity = qty
            };

            _context.OrderProducts.Add(orderProduct); // add order in OrderProducts table

            // calculate the total amount 
            order.TotalAmount = order.OrderProducts
                     .Sum(op => op.Quantity * op.Product.Price);

            // reduce product stock
            product.Stock -= qty;

            _context.SaveChanges();
            return Ok("Order placed successfully.");

        }
    }
}
