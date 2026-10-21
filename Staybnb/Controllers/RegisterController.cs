
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
            string email, string password, string confirmPassword)
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

            if (result.Succeeded)
            {
                //await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Error = string.Join(" ", result.Errors.Select(e => e.Description));
            return View();
        }
    }
}