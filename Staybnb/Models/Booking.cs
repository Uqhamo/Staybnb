namespace Staybnb.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string GuestId { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

        public int NumberOfGuests { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }

        public bool IsDocumentUploaded { get; set; }
        public string? GuestDocumentPath { get; set; }
        public bool IsDocumentVerified { get; set; }

    }
}
