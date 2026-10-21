using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staybnb.Data;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Staybnb.Models;
using System.Security.Claims;

namespace Staybnb.Controllers

{
    [Authorize(Policy ="RequireAdminRole")]
    [ApiController]
    [Route("api/admin/application")]

    public class ApplicationReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApplicationReviewController> _logger;
        public ApplicationReviewController(ApplicationDbContext context, ILogger<ApplicationReviewController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [HttpPost("{id}/review")]

        public async Task<IActionResult> ReviewApplication(int id, [FromBody] ReviewDto model)
        {
            var app = await _context.PropertyApplications.FindAsync(id);

            if (app == null)
            {
                return NotFound();
            }

            //app.Status = model.IsApproved ? "Approved" : "Rejected";
            app.Status = model.IsApproved
    ? ApplicationStatus.Approved
    : ApplicationStatus.Rejected;
            app.ReviewedBy = User.Identity?.Name ?? "Unknown";
            app.ReviewedAt = DateTime.Now;

            var log = new ActivityLog
            {
                UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
                UserEmail = User.Identity?.Name ?? string.Empty,
                Action = $"Application_{app.Status}",
                Details = $"Property ID {app.Id} evaluated by admin",
                IpAddres = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            _context.ActivityLogs.Add(log);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Application has been {app.Status.ToString().ToLower()}."
            });
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
