using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;
using System.Security.Claims;

namespace Staybnb.Controllers
{
    [Authorize]
    public class GuestCheckInController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GuestCheckInController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string bookingId)
        {
            if (string.IsNullOrWhiteSpace(bookingId))
            {
                return BadRequest("Booking ID is required.");
            }

            var guestId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var booking = await _context.BookingRequests
                .Include(b => b.PropertyApplication)
                .FirstOrDefaultAsync(b =>
                    b.Id == bookingId &&
                    b.GuestId == guestId);

            if (booking == null)
            {
                return NotFound("Booking not found.");
            }

            if (booking.Status != BookingStatus.Approved)
            {
                return BadRequest("Only approved bookings can check in.");
            }

            return View(booking);
        }
    }
}
