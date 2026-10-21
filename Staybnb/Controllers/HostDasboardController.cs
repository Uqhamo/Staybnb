using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Staybnb.Data;
using Staybnb.Models;
using System.Security.Claims;

namespace Staybnb.Controllers
{
    [Authorize(Roles = "Host")]
    public class HostDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public HostDashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> MyProperties()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }

            var properties = await _context.PropertyApplications
                .Where(p => p.HostId == userId)
                .ToListAsync();

            return View(properties);
        }
        private string? HostId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public async Task<IActionResult> Index(int propertyId)
        {
            var property = await _context.PropertyApplications
                .Include(p => p.BookingRequests)
                .Include(p => p.CheckInConfig)
                .FirstOrDefaultAsync(p => p.Id == propertyId && p.HostId == HostId);

            if (property == null) return NotFound("Property not found or unauthorized");

            if (property.Status != ApplicationStatus.Approved) return View("PendingApproval");

            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int propertyId)
        {
            var property = await _context.PropertyApplications
                .FirstOrDefaultAsync(p => p.Id == propertyId && p.HostId == HostId);

            if (property != null)
            {
                property.IsActive = !property.IsActive;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { propertyId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewBooking(string bookingId, BookingStatus decision)
        {
            var booking = await _context.BookingRequests
        .Include(b => b.PropertyApplication)
        .FirstOrDefaultAsync(b =>
            b.Id == bookingId &&
            b.PropertyApplication != null &&
            b.PropertyApplication.HostId == HostId);


            if (booking == null) return NotFound();

            booking.Status = decision;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { propertyId = booking.PropertyApplicationId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfigurationCheckIn(CheckInProcess config)
        {
            var ownsProperty = await _context.PropertyApplications
                .AnyAsync(p => p.Id == config.PropertyApplicationId && p.HostId == HostId);

            if (!ownsProperty) return Unauthorized();

            var existingConfig = await _context.CheckInProcesses
                .FirstOrDefaultAsync(c => c.PropertyApplicationId == config.PropertyApplicationId);

            if (existingConfig == null)
            {
                _context.CheckInProcesses.Add(config);
            }
            else
            {
                existingConfig.HouseRules = config.HouseRules;
                existingConfig.RequiredIdUpload = config.RequiredIdUpload;
                existingConfig.RequiresRentalAgreementSigned = config.RequiresRentalAgreementSigned;
                existingConfig.CheckInInstructions = config.CheckInInstructions;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { propertyId = config.PropertyApplicationId });
        }
    }


    /*
    [Authorize(Roles = "Guest")]
    public class GuestDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }*/
}
