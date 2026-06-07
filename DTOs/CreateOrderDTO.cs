using System.ComponentModel.DataAnnotations;

namespace E_Commerce_System_API.DTOs
{
    public class CreateOrderDTO
    {
        public List<OrderItemsDTO> Items { get; set; }

    }
}
