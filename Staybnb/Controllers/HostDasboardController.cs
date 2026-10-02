using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Staybnb.Controllers
{
    [Authorize(Roles ="Host")]
    public class HostDasboardController: Controller
    {
        public IActionResult Index() { 
         return View();
        }
    }

    [Authorize(Roles = "Guest")]
    public class GuestDasboardController : Controller {
        public IActionResult Index()
        {
            return View();
        }

    }
}
