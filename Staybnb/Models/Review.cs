using System.ComponentModel.DataAnnotations;

namespace Staybnb.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public int PropertyApplicationId { get; set; }
        public PropertyApplication? PropertyApplication { get; set; }

        [Required]
        public string GuestId { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsApproved { get; set; } = false;
    }
}