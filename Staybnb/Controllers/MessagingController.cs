using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;

namespace Staybnb.Controllers
{
    [Authorize]
    public class MessagingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MessagingController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

       
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var messages = await _context.Messages
                .Where(m => m.ReceiverId == userId ||
                            m.SenderId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            return View(messages);
        }

        
        [HttpGet]
        public IActionResult Compose()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Compose(
            string receiverEmail,
            string subject,
            string content)
        {
            var senderId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(receiverEmail) ||
                string.IsNullOrWhiteSpace(subject) ||
                string.IsNullOrWhiteSpace(content))
            {
                ViewBag.Error = "Please complete all fields.";
                return View();
            }

            var receiver = await _userManager.FindByEmailAsync(
                receiverEmail.Trim());

            if (receiver == null)
            {
                ViewBag.Error = "Recipient email was not found.";
                return View();
            }

            if (receiver.Id == senderId)
            {
                ViewBag.Error = "You cannot message yourself.";
                return View();
            }

            var message = new Message
            {
                SenderId = senderId!,
                ReceiverId = receiver.Id,
                Subject = subject.Trim(),
                Content = content.Trim(),
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);

            _context.Notifications.Add(new Notification
            {
                UserId = receiver.Id,
                Message = "You received a new message: " + subject.Trim(),
                TargetUrl = "/Messaging/Index",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Message sent successfully.";

            return RedirectToAction(nameof(Index));
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(
            int parentMessageId,
            string content)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Please enter a reply.";
                return RedirectToAction(nameof(Index));
            }

            var originalMessage = await _context.Messages
                .FirstOrDefaultAsync(m =>
                    m.Id == parentMessageId &&
                    m.ReceiverId == userId);

            if (originalMessage == null)
            {
                return NotFound();
            }

            var reply = new Message
            {
                SenderId = userId!,
                ReceiverId = originalMessage.SenderId,
                Subject = "Re: " + originalMessage.Subject,
                Content = content.Trim(),
                SentAt = DateTime.UtcNow,
                IsRead = false,
                ParentMessageId = originalMessage.Id
            };

            _context.Messages.Add(reply);

            _context.Notifications.Add(new Notification
            {
                UserId = originalMessage.SenderId,
                Message = "You received a reply: " + reply.Subject,
                TargetUrl = "/Messaging/Index",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Reply sent successfully.";

            return RedirectToAction(nameof(Index));
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = _userManager.GetUserId(User);

            var message = await _context.Messages
                .FirstOrDefaultAsync(m =>
                    m.Id == id &&
                    m.ReceiverId == userId);

            if (message == null)
            {
                return NotFound();
            }

            message.IsRead = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}