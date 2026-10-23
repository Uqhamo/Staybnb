
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;
using System.Security.Claims;

namespace Staybnb.Controllers
{
    [Authorize(Roles = "Guest")]
    public class GuestDashBoardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GuestDashBoardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var guestId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(guestId))
            {
                return Challenge();
            }

            var bookings = _context.BookingRequests
                .Where(b => b.GuestId == guestId);

            ViewBag.TotalBookings = await bookings.CountAsync();

            ViewBag.PendingBookings = await bookings.CountAsync(
                b => b.Status == BookingStatus.Pending);

            ViewBag.ApprovedBookings = await bookings.CountAsync(
                b => b.Status == BookingStatus.Approved);

            ViewBag.RejectedBookings = await bookings.CountAsync(
                b => b.Status == BookingStatus.Rejected);

            return View();
        }
    }
}

