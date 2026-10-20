using Microsoft.EntityFrameworkCore;
using Staybnb.Models;
namespace Staybnb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<PropertyApplication> PropertYApplications { get; set; }

        public DbSet<PropertyImage> PropertyImages { get; set; }

        public DbSet<BookingRequest> BookingRequests { get; set; }
        public DbSet<CheckInProcess> CheckInProcesses { get; set; }

        public DbSet<Property> Properties { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        public DbSet <ActivityLog> ActivityLogs { get; set; }


    }
}
