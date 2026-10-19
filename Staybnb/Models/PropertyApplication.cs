using System.ComponentModel.DataAnnotations
namespace Staybnb.Models
{
    public class PropertyApplication
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage ="Price is required")]
        [Range(0.01, 100000,ErrorMessage ="Price must be greater than 0")]
        public decimal Price {  get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;

        public string HostId { get; set; } = string.Empty;
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
        public bool IsActive { get; set; } = false;

        public List<PropertyImage> Images { get; set; } = new();
        public List<BookingRequest> BookingRequests { get; set; } = new();
        public CheckInProcess? CheckInConfig { get; set; }

    }
    public class PropertyImage {
        [Key]
        public int Id { get; set; }
        [Required]
        public string ImageUrl { get; set; } = string.Empty;
        public int PropertyApplicationId { get; set; }
    
    }
    public enum ApplicationStatus
    {
        Pending,Approved,Rejected
    }
}
