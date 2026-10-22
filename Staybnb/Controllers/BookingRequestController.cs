using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;
using System.Security.Claims;

namespace Staybnb.Controllers
{
    [Authorize]
    public class BookingRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BookingRequestController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingRequest model)
        {
            var property = await _context.PropertyApplications
                .FirstOrDefaultAsync(p =>
                    p.Id == model.PropertyApplicationId &&
                    p.Status == ApplicationStatus.Approved &&
                    p.IsActive);

            if (property == null)
            {
                return NotFound("This property is unavailable for booking.");
            }

            var guestId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(guestId))
            {
                return Challenge();
            }

            if (model.CheckIn.Date < DateTime.Today)
            {
                TempData["Error"] = "Check-in cannot be in the past.";

                return RedirectToAction(
                    "Details", "Property",
                    new { id = model.PropertyApplicationId });
            }

            if (model.CheckOut.Date <= model.CheckIn.Date)
            {
                TempData["Error"] =
                    "Check-out must be after the check-in date.";

                return RedirectToAction(
                    "Details", "Property",
                    new { id = model.PropertyApplicationId });
            }

            var guest = await _userManager.FindByIdAsync(guestId);

            if (guest == null)
            {
                return Challenge();
            }

            var bookingRequest = new BookingRequest
            {
                Id = Guid.NewGuid().ToString(),
                GuestId = guestId,
                GuestName = guest.UserName ?? guest.Email ?? "Guest",
                CheckIn = model.CheckIn,
                CheckOut = model.CheckOut,
                PropertyApplicationId = property.Id,
                Status = BookingStatus.Pending
            };

            _context.BookingRequests.Add(bookingRequest);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Your booking request has been submitted successfully.";

            return RedirectToAction(
                "Details", "Property",
                new { id = property.Id });
        }
    }
}
