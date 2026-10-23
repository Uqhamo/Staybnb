using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;
using System.Security.Claims;

namespace Staybnb.Controllers
{
    [Authorize(Roles = "Guest")]
    public class GuestBookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GuestBookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var guestId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(guestId))
            {
                return Challenge();
            }

            var bookings = await _context.BookingRequests
                .Include(b => b.PropertyApplication)
                .Where(b => b.GuestId == guestId)
                .OrderByDescending(b => b.CheckIn)
                .ToListAsync();

            return View(bookings);
        }
    }
}