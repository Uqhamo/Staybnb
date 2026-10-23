using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;

namespace Staybnb.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public SuperAdminController(
            UserManager<IdentityUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Guests()
        {
            var guests = await _userManager.GetUsersInRoleAsync("Guest");
            return View(guests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            return RedirectToAction("Guests");
        }

        
        [HttpGet]
        public async Task<IActionResult> PropertyApprovals()
        {
            var properties = await _context.PropertyApplications
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(properties);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveProperty(int id)
        {
            var property = await _context.PropertyApplications
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
                return NotFound();

            property.Status = ApplicationStatus.Approved;
            property.IsActive = true;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Property approved successfully.";

            return RedirectToAction(nameof(PropertyApprovals));
        }
    }
}