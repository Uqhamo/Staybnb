namespace Staybnb.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string Details { get; set; } = string.Empty;

        public string IpAddres { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    }
}
