using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;
using System.Security.Claims;

namespace Staybnb.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReviewsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Display approved reviews for a property
        [AllowAnonymous]
        public async Task<IActionResult> PropertyReviews(int propertyId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.PropertyApplication)
                .Where(r =>
                    r.PropertyApplicationId == propertyId &&
                    r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        // Submit a review
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int propertyApplicationId,
            int rating,
            string? comment)
        {
            var guestId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(guestId))
            {
                return Challenge();
            }

            if (rating < 1 || rating > 5)
            {
                ModelState.AddModelError(
                    "rating",
                    "Rating must be between 1 and 5.");
            }

            if (comment?.Length > 1000)
            {
                ModelState.AddModelError(
                    "comment",
                    "Comment cannot exceed 1000 characters.");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] =
                    "Please check your rating and comment.";

                return RedirectToAction(
                    "Details",
                    "Property",
                    new { id = propertyApplicationId });
            }

            // Check that the property exists
            var propertyExists = await _context.PropertyApplications
                .AnyAsync(p =>
                    p.Id == propertyApplicationId &&
                    p.Status == ApplicationStatus.Approved &&
                    p.IsActive);

            if (!propertyExists)
            {
                return NotFound();
            }

            // Only guests who have checked in may review
            var hasCheckedIn = await _context.BookingRequests
                .AnyAsync(b =>
                    b.PropertyApplicationId == propertyApplicationId &&
                    b.GuestId == guestId &&
                    b.Status == BookingStatus.CheckedIn);

            if (!hasCheckedIn)
            {
                return Forbid();
            }

            // Prevent duplicate reviews by the same guest
            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r =>
                    r.PropertyApplicationId == propertyApplicationId &&
                    r.GuestId == guestId);

            if (alreadyReviewed)
            {
                TempData["Error"] =
                    "You have already reviewed this property.";

                return RedirectToAction(
                    "Details",
                    "Property",
                    new { id = propertyApplicationId });
            }

            var review = new Review
            {
                PropertyApplicationId = propertyApplicationId,
                GuestId = guestId,
                Rating = rating,
                Comment = string.IsNullOrWhiteSpace(comment)
                    ? null
                    : comment.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsApproved = false
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Your review was submitted and is awaiting approval.";

            return RedirectToAction(
                "Details",
                "Property",
                new { id = propertyApplicationId });
        }
    }
}