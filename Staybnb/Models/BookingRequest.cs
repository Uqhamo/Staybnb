namespace Staybnb.Models
{
    public class BookingRequest
    {
        public string Id { get; set; } = string.Empty;
        public string GuestId { get; set; } = string.Empty;
        public DateTime CheckIn { get; set; }

        public DateTime CheckOut { get; set; }

        public string GuestName { get; set; } = string.Empty;

        public BookingStatus Status { get; set; }

        public int PropertyApplicationId { get; set; }

        public PropertyApplication? PropertyApplication { get; set; }
    }

    public enum BookingStatus
    {
        Pending,
        Approved,
        Rejected,
        CheckedIn
    }
}