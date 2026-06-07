using System.ComponentModel.DataAnnotations;

namespace E_Commerce_System_API.DTOs
{
    public class AddReviewDTO
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        [Required]
        public int PId { get; set; }
    }
}
