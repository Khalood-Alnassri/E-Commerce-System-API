using System.ComponentModel.DataAnnotations;

namespace E_Commerce_System_API.DTOs
{
    public class UpdateProductDTO
    {
        public string? PName { get; set; }

        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be grater than 0")]
        public decimal? Price { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Stock must be 0 or Positive value")]
        public int? Stock { get; set; }
    }
}
