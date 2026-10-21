
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Staybnb.Controllers
{
    [Authorize(Roles = "Guest")]
    public class GuestDashBoardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
