using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Staybnb.Data;
using Staybnb.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Staybnb.Controllers
{
    [Authorize(Policy = "RequireAdminRole")]
    [ApiController]
    [Route("api/admin/aduit")]
    public class SystemAuditController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        public SystemAuditController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost("logs")]
        public async Task<IActionResult> GetActivityLogs([FromQuery] int page =1, [FromQuery] int pageSize = 50)
        {
            var logs = await _context.ActivityLogs
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(logs);
        }


        
    }
}
