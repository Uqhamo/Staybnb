using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Staybnb.Data;
using Staybnb.Models;

namespace Staybnb.Controllers
{
    [Authorize(Roles ="Host")]
    public class HostDasboardController: Controller
    {
        private readonly ApplicationDbContext _context;
        public HostDasboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int propertyId) {
            var currenrHost = User.Identity.Name;
            var property = await _context.PropertYApplications
                           .Include(p => p.BookingRequests)
                           .Include(p => p.CheckInConfig)
                           .FirstOrDefaultAsync(p => p.Id == propertyId && p.HostId == User.Identity.Name);
            if(property == null) return NotFound("Property not found or unauthorized ");
            
            if(property.Status!=Models.ApplicationStatus.Approved) return View("PendingApproval");
            return View(property);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int propertyId) {
            var property = await _context.PropertYApplications
                             
                           
                              .FirstOrDefaultAsync(p => p.Id == propertyId && p.HostId == User.Identity.Name);
            if (property != null) { 
            property.IsActive = !property.IsActive;
                await _context.SaveChangesAsync();
            } 
                return RedirectToAction(nameof(Index), new {propertyId});

        }
        [HttpPost]
        public async Task<IActionResult> ReviewBooking(int bookingId, BookingStatus decision) {
            var booking = await _context.BookingRequests
                               .Include(b => b.PropertYApplication)
                              
                               .FirstOrDefaultAsync(b => b.Id == bookingId && b.PropertyApplication == User.Identity.Name);
            if (booking == null) return NotFound();
            booking.Status = decision;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { propertyId = booking.PropertYAplication });
            

        }
        [HttpPost]
        public async Task<IActionResult> ConfigurationCheckIn(CheckInProcess config) {
            var property = await _context.CheckInProcesses
                    .FirstOrDefaultAsync(c => c.PropertyApplication == config.PropertyApplication);

        
        }
    }

    [Authorize(Roles = "Guest")]
    public class GuestDasboardController : Controller {
        public IActionResult Index()
        {
            return View();
        }

    }
}
