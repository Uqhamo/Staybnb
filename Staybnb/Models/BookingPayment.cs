using System.ComponentModel.DataAnnotations;

namespace Staybnb.Models
{
    public class BookingPayment
    {
        public int Id { get; set; }

        [Required]
        public string BookingRequestId { get; set; } = string.Empty;

        [Required]
        public string GuestId { get; set; } = string.Empty;

        [Range(0.01, 10000000)]
        public decimal Amount { get; set; }

        public DemoPaymentStatus Status { get; set; }
            = DemoPaymentStatus.Pending;

        public string? TransactionReference { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PaidAt { get; set; }
    }

    public enum DemoPaymentStatus
    {
        Pending,
        Paid,
        Failed
    }
}