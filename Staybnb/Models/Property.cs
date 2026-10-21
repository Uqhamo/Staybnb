using System.ComponentModel.DataAnnotations;
namespace Staybnb.Models
{
    public class Property
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string City {  get; set; } = string.Empty;

        public string PropertyType {  get; set; } = string.Empty;
        public decimal PricePerNight {  get; set; }
        public decimal CleaningFee { get; set; }
        public decimal ServiceFee {  get; set; }
        public bool IsActive { get; set; } = true;
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
