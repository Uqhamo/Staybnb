using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;

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
            var bookings = await _context.BookingRequests
                .Include(b => b.PropertyApplication)
                .OrderByDescending(b => b.CheckIn)
                .ToListAsync();

            return View(bookings);
        }
    }
}