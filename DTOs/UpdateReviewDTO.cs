using System.ComponentModel.DataAnnotations;

namespace E_Commerce_System_API.DTOs
{
    public class UpdateReviewDTO
    {
        [Range(1, 5)]
        public int? Rating { get; set; }

        public string? Comment { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
