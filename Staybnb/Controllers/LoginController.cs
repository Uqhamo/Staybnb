using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;

namespace Staybnb.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public LoginController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager, ApplicationDbContext context
            )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;

        }

        [HttpGet]
        public IActionResult Index(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            string email,
            string password,
            string? returnUrl = null)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                password,
                isPersistent: false,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);





                if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
                {
                    return RedirectToAction("PropertyApprovals", "SuperAdmin");
                }

                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("ApplicationPending", "HostApplication");
                }


                if (await _userManager.IsInRoleAsync(user, "Host"))
                {
                    return RedirectToAction(
                        "Overview", "HostDashboard");
                }

                
                if (await _userManager.IsInRoleAsync(user, "Guest"))
                {

                    var hasPendingApplication = await _context.HostApplications
       .AnyAsync(a => a.UserId == user.Id && !a.IsApproved);

                    if (hasPendingApplication)
                    {
                        return RedirectToAction("Success", "HostRegistration");
                    }
                    return RedirectToAction(
                        "Index", "GuestDashBoard");
                }

                
                if (!string.IsNullOrEmpty(returnUrl) &&
                    Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid email or password.";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}