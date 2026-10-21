using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;

    


namespace Staybnb.Controllers
{
    public class MessagingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public MessagingController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var messages = await _context.Messages
                       .Where(m => m.ReceiverId == userId)
                       .OrderByDescending(m => m.SentAt)
                       .ToListAsync();

            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> Reply(int parentMessageId, string content) {
            var userId = _userManager.GetUserId(User);
            var originalMessage = await _context.Messages.FindAsync(parentMessageId);
            if (originalMessage == null || originalMessage.ReceiverId != userId) return NotFound();

            var reply = new Message
            {
                SenderId = userId,
                ReceiverId = originalMessage.SenderId,
                Subject =$"Re {originalMessage.Subject}",
                Content = content,
                ParentMessageId = parentMessageId
            };

            _context.Messages.Add(reply);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
