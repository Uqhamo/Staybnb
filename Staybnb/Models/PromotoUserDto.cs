using System.ComponentModel.DataAnnotations;

namespace Staybnb.Models
{
    public class PromoteUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}


