using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace E_Commerce_System_API.Models
{
    public class Review
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RId { get; set; }

        [Required]
        [Range(1,5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime? ReviewDate { get; set; }

        public DateTime? UpdatedDate { get; set; } // New property to track the last update time of the review

        [ForeignKey("User")]
        public int UId { get; set; }
        public virtual User User { get; set; }

        [ForeignKey("Product")]
        public int PId { get; set; }
        public virtual Product Product { get; set; }



    }
}
