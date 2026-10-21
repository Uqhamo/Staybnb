using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Staybnb.Data;
using Staybnb.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;

namespace Staybnb.Controllers
{
    [Authorize(Policy = "RequireSuperAdminRole")]
    [ApiController]
    [Route("api/superadmin/users")]
    public class UserManagementController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserManagementController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;

        }
        [HttpPost("promote")]
        public async Task<IActionResult> PromoteToAdmin(
     [FromBody] PromoteUserDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return NotFound("User not found");

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return BadRequest("User is already an Admin");

            var result = await _userManager.AddToRoleAsync(user, "Admin");

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var log = new ActivityLog
            {
                UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "",
                UserEmail = User.Identity?.Name ?? "",
                Action = "Role_Promotion",
                Details = $"Promoted {user.Email} to admin",
                IpAddres = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"{user.Email} has been promoted to admin successfully"
            });
        }
    }
}
