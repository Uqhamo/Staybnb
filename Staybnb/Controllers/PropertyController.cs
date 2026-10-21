using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Staybnb.Data;
using Staybnb.Models;
using Staybnb.Models.ViewModels;

namespace Staybnb.Controllers
{
    public class PropertyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PropertyController(ApplicationDbContext context)
        {
            _context = context;
        }

       
        [Authorize]
        [HttpGet]
        public IActionResult BecomeAHost()
        {
            return View(new PropertySubmissionViewModel());
        }

       
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BecomeAHost(PropertySubmissionViewModel model)
        {
            if (model.PropertyImages == null ||
                !model.PropertyImages.Any(f => f.Length > 0))
            {
                ModelState.AddModelError(
                    nameof(model.PropertyImages),
                    "You must upload at least one property image.");
            }

           
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(hostId))
            {
                return Challenge();
            }

            var uploadFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads");

            Directory.CreateDirectory(uploadFolder);

            var newProperty = new PropertyApplication
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                Address = model.Address,
                HostId = hostId,
                Status = ApplicationStatus.Pending
            };

            foreach (var file in model.PropertyImages!)
            {
                if (file == null || file.Length == 0)
                {
                    continue;
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        nameof(model.PropertyImages),
                        "Only JPG, JPEG, PNG and WEBP images are allowed.");

                    return View(model);
                }

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                newProperty.Images.Add(new PropertyImage
                {
                    ImageUrl = "/uploads/" + fileName
                });
            }

            _context.PropertyApplications.Add(newProperty);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Your property application has been submitted for review.";

            return RedirectToAction(nameof(BecomeAHost));
        }

        
        [HttpGet]
        public async Task<IActionResult> Index(string? city)
        {
            var query = _context.PropertyApplications
                .Include(p => p.Images)
                .Where(p =>
                    p.Status == ApplicationStatus.Approved &&
                    p.IsActive);

            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(p => p.Address.Contains(city));
            }

            ViewBag.City = city;

            var properties = await query.ToListAsync();

            return View(properties);
        }
    }
}