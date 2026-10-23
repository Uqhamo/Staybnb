using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;

namespace Staybnb.Services
{
    public class HostStatisticsService
    {
        private readonly ApplicationDbContext _context;

        public HostStatisticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetEarningsAsync(string hostId)
        {
            var earnings = await _context.Bookings
                .Where(b =>
                    b.Status == BookingStatus.CheckedIn &&
                    _context.PropertyApplications.Any(p =>
                        p.Id == b.PropertyId &&
                        p.HostId == hostId))
                .SumAsync(b => (decimal?)b.TotalPrice);

            return earnings ?? 0m;
        }

        public async Task<double> GetAverageRatingAsync(string hostId)
        {
            var averageRating = await _context.Reviews
                .Where(r =>
                    r.IsApproved &&
                    _context.PropertyApplications.Any(p =>
                        p.Id == r.PropertyApplicationId &&
                        p.HostId == hostId))
                .Select(r => (double?)r.Rating)
                .AverageAsync();

            return averageRating ?? 0.0;
        }

        public async Task<int> GetReviewCountAsync(string hostId)
        {
            return await _context.Reviews
                .CountAsync(r =>
                    r.IsApproved &&
                    _context.PropertyApplications.Any(p =>
                        p.Id == r.PropertyApplicationId &&
                        p.HostId == hostId));
        }
    }
}