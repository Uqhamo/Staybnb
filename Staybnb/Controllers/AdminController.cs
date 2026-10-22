using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;

namespace Staybnb.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AdminPropertyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminPropertyController(ApplicationDbContext context)
        {
            _context = context;
        }

         
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var properties = await _context.PropertyApplications
                .Where(p => p.Status == ApplicationStatus.Pending)
                .ToListAsync();

            return View(properties);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var property = await _context.PropertyApplications
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return NotFound();
            }

            property.Status = ApplicationStatus.Approved;
            property.IsActive = true;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Property approved successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}