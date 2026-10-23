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
    public class BookingPaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingPaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display booking details and demo payment amount
        [HttpGet]
        public async Task<IActionResult> Pay(string id)
        {
            var guestId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var booking = await _context.BookingRequests
                .Include(b => b.PropertyApplication)
                .FirstOrDefaultAsync(b =>
                    b.Id == id && b.GuestId == guestId);

            if (booking == null)
                return NotFound();

            if (booking.Status != BookingStatus.Approved)
            {
                TempData["Error"] =
                    "Your booking must be approved by the host before payment.";

                return RedirectToAction("Index", "GuestBookings");
            }

            var nights = (booking.CheckOut.Date -
                          booking.CheckIn.Date).Days;

            if (nights <= 0)
                return BadRequest("The booking dates are invalid.");

            ViewBag.Nights = nights;
            ViewBag.TotalPrice =
                booking.PropertyApplication!.Price * nights;

            var payment = await _context.BookingPayments
                .FirstOrDefaultAsync(p =>
                    p.BookingRequestId == booking.Id &&
                    p.GuestId == guestId &&
                    p.Status == DemoPaymentStatus.Paid);

            if (payment != null)
            {
                TempData["Success"] = "This booking has already been paid.";
                return RedirectToAction("Index", "GuestBookings");
            }

            return View(booking);
        }

        // Simulate a successful demo payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayDemo(string bookingRequestId)
        {
            var guestId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var booking = await _context.BookingRequests
                .Include(b => b.PropertyApplication)
                .FirstOrDefaultAsync(b =>
                    b.Id == bookingRequestId &&
                    b.GuestId == guestId);

            if (booking == null)
                return NotFound();

            if (booking.Status != BookingStatus.Approved)
            {
                TempData["Error"] =
                    "Only host-approved bookings can be paid.";

                return RedirectToAction("Index", "GuestBookings");
            }

            var nights = (booking.CheckOut.Date -
                          booking.CheckIn.Date).Days;

            if (nights <= 0)
                return BadRequest("The booking dates are invalid.");

            var amount = booking.PropertyApplication!.Price * nights;

            var payment = await _context.BookingPayments
                .FirstOrDefaultAsync(p =>
                    p.BookingRequestId == booking.Id &&
                    p.GuestId == guestId);

            if (payment?.Status == DemoPaymentStatus.Paid)
            {
                TempData["Success"] =
                    "This booking has already been paid.";

                return RedirectToAction("Index", "GuestBookings");
            }

            if (payment == null)
            {
                payment = new BookingPayment
                {
                    BookingRequestId = booking.Id,
                    GuestId = guestId!,
                    Amount = amount,
                    CreatedAt = DateTime.UtcNow
                };

                _context.BookingPayments.Add(payment);
            }

            payment.Amount = amount;
            payment.Status = DemoPaymentStatus.Paid;
            payment.TransactionReference =
                "DEMO-" + Guid.NewGuid().ToString("N")
                    .Substring(0, 12).ToUpper();
            payment.PaidAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Demo payment successful! No real money was transferred.";

            return RedirectToAction("Index", "GuestBookings");
        }

        // Guest initiates check-in after payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(string bookingRequestId)
        {
            var guestId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var booking = await _context.BookingRequests
                .FirstOrDefaultAsync(b =>
                    b.Id == bookingRequestId &&
                    b.GuestId == guestId);

            if (booking == null)
                return NotFound();

            var paid = await _context.BookingPayments
                .AnyAsync(p =>
                    p.BookingRequestId == booking.Id &&
                    p.GuestId == guestId &&
                    p.Status == DemoPaymentStatus.Paid);

            if (!paid)
            {
                TempData["Error"] =
                    "Please complete payment before checking in.";

                return RedirectToAction("Index", "GuestBookings");
            }

            if (booking.Status == BookingStatus.CheckedIn)
            {
                TempData["Error"] =
                    "You have already checked in.";

                return RedirectToAction("Index", "GuestBookings");
            }

            if (booking.Status != BookingStatus.Approved)
            {
                TempData["Error"] =
                    "Your booking is not approved for check-in.";

                return RedirectToAction("Index", "GuestBookings");
            }

            if (DateTime.Today < booking.CheckIn.Date)
            {
                TempData["Error"] =
                    "You can only check in on or after your check-in date.";

                return RedirectToAction("Index", "GuestBookings");
            }

            if (DateTime.Today >= booking.CheckOut.Date)
            {
                TempData["Error"] =
                    "The booking period has ended.";

                return RedirectToAction("Index", "GuestBookings");
            }

            booking.Status = BookingStatus.CheckedIn;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Check-in successful! Enjoy your stay.";

            return RedirectToAction("Index", "GuestBookings");
        }
    }
}
