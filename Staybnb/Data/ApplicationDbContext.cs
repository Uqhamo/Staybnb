using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Staybnb.Models;
namespace Staybnb.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<PropertyApplication> PropertyApplications { get; set; }

        public DbSet<PropertyImage> PropertyImages { get; set; }

        public DbSet<BookingRequest> BookingRequests { get; set; }
        public DbSet<CheckInProcess> CheckInProcesses { get; set; }

        public DbSet<Property> Properties { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        public DbSet <ActivityLog> ActivityLogs { get; set; }

        public DbSet<Message> Messages { get; set; }
        public DbSet<HostApplication> HostApplications { get; set; }


    }
}
