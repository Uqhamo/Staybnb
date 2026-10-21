using System.ComponentModel.DataAnnotations;

namespace Staybnb.Models.ViewModels
{
    public class HostRegistrationViewModel
    {
        [Required]
        [Display(Name = "Business/Property Name")]
        public string BusinessName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tell us about your property")]
        public string Notes { get; set; } = string.Empty;
    }
}
