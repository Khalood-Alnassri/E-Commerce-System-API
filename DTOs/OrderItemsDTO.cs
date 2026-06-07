using System.ComponentModel.DataAnnotations;

namespace E_Commerce_System_API.DTOs
{
    public class OrderItemsDTO
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be grater than 0")]
        public int Quantity { get; set; }
    }
}
