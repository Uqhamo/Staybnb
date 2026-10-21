using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models.ViewModels;


namespace Staybnb.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class HostApplicationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HostApplicationController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> ApplicationPending()
        {
            var apps = await _context.HostApplications
                .Where(a => !a.IsApproved)
                .ToListAsync();

            var model = new List<PendingApplicationViewModel>();

            foreach (var app in apps)
            {
                var user = await _userManager.FindByIdAsync(app.UserId);
                model.Add(new PendingApplicationViewModel
                {
                    ApplicationId = app.Id,
                    Email = user?.Email ?? "Unknown"
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveApplication(int ApplicationId)
        {
            var application = await _context.HostApplications.FindAsync(ApplicationId);
            if (application == null) return NotFound();

            var user = await _userManager.FindByIdAsync(application.UserId);
            if (user == null) return NotFound();

            application.IsApproved = true;
            _context.Update(application);
            await _context.SaveChangesAsync();

            if (!await _userManager.IsInRoleAsync(user, "Host"))
                await _userManager.AddToRoleAsync(user, "Host");

            if (await _userManager.IsInRoleAsync(user, "Guest"))
                await _userManager.RemoveFromRoleAsync(user, "Guest");

            await _userManager.UpdateSecurityStampAsync(user);

            return RedirectToAction("ApplicationPending");
        }
    }
}