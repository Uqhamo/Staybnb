using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Staybnb.Data;

namespace Staybnb.Controllers

    
{
    [Authorize(Roles ="SuperAdmin")]
    public class SuperAdminController : Controller
    {
        private readonly UserManager _userManager;

        public SuperAdminController(UserManger userManger) { 
         
            _userManager = userManger;
        }
        [HttpPost]
        public async Task<IActionResult> PromoteToAdmin(string userId)
        {
            var user = await _userManager.FindAsync(userId);
            if (user == null) return NotFound();

            if (!await _userManager.IsInRoleAsync(user, "Admin")) {

                await _userManager.AddToRoleAsync(user, "Admin");
            }
            return RedirectToAction("ManageUsers");
        }
    }
}
