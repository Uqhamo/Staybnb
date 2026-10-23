using Staybnb.Data;
using Staybnb.Models;

namespace Staybnb.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(
            string userId,
            string message,
            string? targetUrl = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return;

            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                TargetUrl = targetUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();
        }
    }
}