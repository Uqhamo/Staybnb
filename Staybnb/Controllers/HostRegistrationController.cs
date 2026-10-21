using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;
using Staybnb.Models.ViewModels;

namespace Staybnb.Controllers
{
    [Authorize]
    public class HostRegistrationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HostRegistrationController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new HostRegistrationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(HostRegistrationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            if (await _userManager.IsInRoleAsync(user, "Host"))
            {
                return RedirectToAction("Index", "Home");
            }

            var alreadyApplied = await _context.HostApplications
                .AnyAsync(a => a.UserId == user.Id);

            if (alreadyApplied)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "You have already submitted a host application.");

                return View(model);
            }

            var application = new HostApplication
            {
                UserId = user.Id,
                BusinessName = model.BusinessName,
                Notes = model.Notes,
                IsApproved = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.HostApplications.Add(application);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Success));
        }

        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
}


