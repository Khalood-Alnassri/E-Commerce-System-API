using System.ComponentModel.DataAnnotations;

namespace E_Commerce_System_API.DTOs
{
    public class ProductDetailsDTO
    {
        public int PId { get; set; }

        public string PName { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int Stock { get; set; }

        public decimal OverallRating { get; set; }
    }
}
