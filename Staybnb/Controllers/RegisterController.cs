
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Staybnb.Controllers
{
    public class RegisterController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public RegisterController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            string email,
            string password,
            string confirmPassword,
            string accountType)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Enter your email and password.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            if (accountType != "Guest" && accountType != "Host")
            {
                ViewBag.Error = "Please select a valid account type.";
                return View();
            }

            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                ViewBag.Error = "This email is already registered.";
                return View();
            }

            var user = new IdentityUser
            {
                UserName = email,
                Email = email
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                ViewBag.Error = string.Join(
                    " ", result.Errors.Select(e => e.Description));

                return View();
            }

            
            var roleResult = await _userManager.AddToRoleAsync(user, "Guest");

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                ViewBag.Error = string.Join(
                    " ", roleResult.Errors.Select(e => e.Description));

                return View();
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            
            if (accountType == "Host")
            {
                return RedirectToAction("Index", "HostRegistration");
            }

            
            return RedirectToAction("Index", "GuestDashBoard");
        }
    }
}
