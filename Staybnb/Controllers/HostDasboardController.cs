using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Staybnb.Data;
using Staybnb.Models;
using Staybnb.Services;
using System.Security.Claims;

namespace Staybnb.Controllers
{
    [Authorize(Roles = "Host")]
    public class HostDashboardController : Controller
    {
        private readonly HostStatisticsService _statisticsService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public HostDashboardController(
    ApplicationDbContext context,
    UserManager<IdentityUser> userManager,
    HostStatisticsService statisticsService)
        {
            _context = context;
            _userManager = userManager;
            _statisticsService = statisticsService;
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

        [HttpGet]
        public async Task<IActionResult> Index(int? propertyId)
        {
            var hostId = _userManager.GetUserId(User);

            if (hostId == null)
            {
                return Challenge();
            }

            var query = _context.PropertyApplications
                .Include(p => p.BookingRequests)
                .Include(p => p.CheckInConfig)
                .Where(p => p.HostId == hostId);

            var property = propertyId.HasValue
                ? await query.FirstOrDefaultAsync(p => p.Id == propertyId.Value)
                : await query.FirstOrDefaultAsync();

            if (property == null)
            {
                return RedirectToAction("BecomeAHost", "Property");
            }

            if (property.Status != ApplicationStatus.Approved)
            {
                return View("PendingApproval", property);
            }

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
            var hostId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(hostId))
            {
                return Challenge();
            }

            
            if (decision != BookingStatus.Approved &&
                decision != BookingStatus.Rejected)
            {
                return BadRequest("Invalid booking decision.");
            }

            var booking = await _context.BookingRequests
                .Include(b => b.PropertyApplication)
                .FirstOrDefaultAsync(b =>
                    b.Id == bookingId &&
                    b.PropertyApplication != null &&
                    b.PropertyApplication.HostId == hostId);

            if (booking == null)
            {
                return NotFound();
            }

            
            if (booking.Status != BookingStatus.Pending)
            {
                TempData["Error"] =
                    "This booking request has already been processed.";

                return RedirectToAction(
                    nameof(BookingRequests),
                    new { propertyId = booking.PropertyApplicationId });
            }

            booking.Status = decision;

            await _context.SaveChangesAsync();

            TempData["Success"] = decision == BookingStatus.Approved
                ? "Booking request approved successfully."
                : "Booking request rejected successfully.";

            return RedirectToAction(
                nameof(BookingRequests),
                new { propertyId = booking.PropertyApplicationId });

            //return RedirectToAction(nameof(Index), new { propertyId = booking.PropertyApplicationId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
       
       
        public async Task<IActionResult> ConfigurationCheckIn(CheckInProcess config)
        {
            var hostId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(hostId))
            {
                return Challenge();
            }

            var ownsProperty = await _context.PropertyApplications
                .AnyAsync(p =>
                    p.Id == config.PropertyApplicationId &&
                    p.HostId == hostId);

            if (!ownsProperty)
            {
                return Unauthorized();
            }

            var existingConfig = await _context.CheckInProcesses
                .FirstOrDefaultAsync(c =>
                    c.PropertyApplicationId == config.PropertyApplicationId);

            if (existingConfig == null)
            {
                _context.CheckInProcesses.Add(config);
            }
            else
            {
                existingConfig.HouseRules = config.HouseRules;
                existingConfig.RequiredIdUpload = config.RequiredIdUpload;
                existingConfig.RequiresRentalAgreementSigned =
                    config.RequiresRentalAgreementSigned;
                existingConfig.CheckInInstructions = config.CheckInInstructions;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Index),
                new { propertyId = config.PropertyApplicationId });
        }

        [HttpGet]

        public async Task<IActionResult> Overview()
        {
            var hostId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(hostId))
            {
                return Challenge();
            }

            var properties = await _context.PropertyApplications
                .Where(p => p.HostId == hostId)
                .OrderByDescending(p => p.SubmittedAt)
                .ToListAsync();

            var propertyIds = properties.Select(p => p.Id).ToList();

            var requests = _context.BookingRequests
                .Where(b => propertyIds.Contains(b.PropertyApplicationId));

            ViewBag.Properties = properties;
            ViewBag.TotalProperties = properties.Count;
            ViewBag.ApprovedProperties = properties.Count(
                p => p.Status == ApplicationStatus.Approved);
            ViewBag.PendingProperties = properties.Count(
                p => p.Status == ApplicationStatus.Pending);

            ViewBag.TotalRequests = await requests.CountAsync();
            ViewBag.PendingRequests = await requests.CountAsync(
                b => b.Status == BookingStatus.Pending);
            ViewBag.ApprovedRequests = await requests.CountAsync(
                b => b.Status == BookingStatus.Approved);

            
            ViewBag.Earnings =
                await _statisticsService.GetEarningsAsync(hostId);

            ViewBag.AverageRating =
                await _statisticsService.GetAverageRatingAsync(hostId);

            ViewBag.ReviewCount =
                await _statisticsService.GetReviewCountAsync(hostId);

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> BookingRequests(int propertyId)
        {
            var hostId = _userManager.GetUserId(User);

            var property = await _context.PropertyApplications
                .Include(p => p.BookingRequests)
                .FirstOrDefaultAsync(p =>
                    p.Id == propertyId && p.HostId == hostId);

            if (property == null)
            {
                return NotFound();
            }

            return View(property.BookingRequests);
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
