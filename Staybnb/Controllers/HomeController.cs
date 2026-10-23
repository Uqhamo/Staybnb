using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;
using Staybnb.Models.ViewModels;
using System.Diagnostics;

namespace Staybnb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true &&
                User.IsInRole("Host"))
            {
                var hostId = User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(hostId))
                {
                    var properties = _context.PropertyApplications
                        .Where(p => p.HostId == hostId);

                    var propertyIds = await properties
                        .Select(p => p.Id)
                        .ToListAsync();

                    ViewBag.TotalProperties = propertyIds.Count;

                    ViewBag.TotalRequests = await _context.BookingRequests
                        .CountAsync(b =>
                            propertyIds.Contains(b.PropertyApplicationId));

                    ViewBag.TotalReviews = await _context.Reviews
                        .CountAsync(r =>
                            propertyIds.Contains(r.PropertyApplicationId));

                    
                   
                    ViewBag.TotalEarnings = 0m;
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ??
                    HttpContext.TraceIdentifier
            });
        }
    }
}