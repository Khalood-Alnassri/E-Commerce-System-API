using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace E_Commerce_System_API.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PId { get; set; }

        [Required]
        public string PName { get; set; }

        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage ="Price must be grater than 0")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Stock must be 0 or Positive value")]
        public int Stock {  get; set; }

        public decimal OverallRating { get; set; } = 0;

        public virtual ICollection<Review>  ? Reviews { get;set; }

        public virtual ICollection<OrderProduct>  ? OrderProducts { get; set; }

 




    }
}
