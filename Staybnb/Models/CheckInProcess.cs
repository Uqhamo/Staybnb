namespace Staybnb.Models
{
    public class CheckInProcess
    {
        public int Id { get; set; }
        public int PropertyApplicationId { get; set; }
        public string HouseRules { get; set; } = string.Empty;

        public bool RequiredIdUpload { get; set; }
        public bool RequiresRentalAgreementSigned { get; set; }

        public string CheckInInstructions { get; set; } = string.Empty;

    }
}
