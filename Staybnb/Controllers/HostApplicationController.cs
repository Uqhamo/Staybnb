using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Staybnb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HostApplicationController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager _userManager;
        public HostApplicationController(ApplicationDbContext _context,UserManger userManger)
        {
            _context = _context;
            _userManager = userManger;
        }
        [HttpPost]
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
            {

                await _userManager.AddToRoleAsync(user, "Host");
            }
            if (!await _userManager.IsInRoleAsync(user, "Guest"))
            {

                await _userManager.AddToRoleAsync(user, "Guest");
            }
            return RedirectToAction("Application Pending");
        }
    }
}

