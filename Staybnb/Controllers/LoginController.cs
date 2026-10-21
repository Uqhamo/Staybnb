using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Staybnb.Controllers
{
    public class LoginController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public LoginController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
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
             
                if (await _userManager.IsInRoleAsync(user, "Admin") ||
                    await _userManager.IsInRoleAsync(user, "SuperAdmin"))
                {
                    return RedirectToAction(
                        "ApplicationPending", "HostApplication");
                }

                
                if (await _userManager.IsInRoleAsync(user, "Host"))
                {
                    return RedirectToAction(
                        "MyProperties", "HostDashboard");
                }

                
                if (await _userManager.IsInRoleAsync(user, "Guest"))
                {
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