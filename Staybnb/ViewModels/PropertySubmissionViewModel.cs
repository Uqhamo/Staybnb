using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace Staybnb.Models.ViewModels
{
    public class PropertySubmissionViewModel
    {
        [Required] 
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        [Required]
        public string Address { get; set; } = string.Empty;

        public List<IFormFile> PropertyImages { get; set; } = new();
    }
}
