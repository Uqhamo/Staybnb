namespace Staybnb.Models
{
    public class BookingRequest
    {
        public string Id { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }

        public string GuestName { get; set; } = string.Empty;

        public BookingStatus Status { get; set; }

        public int PropertyApplicationId { get; set; }

    }
    public enum BookingStatus
    {
        Pending,Approved,Rejected
    }
}
