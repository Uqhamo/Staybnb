using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Staybnb.Data;
using Microsoft.AspNetCore.Authorization;
using Staybnb.Models.ViewModels;
using Staybnb.Models;


namespace Staybnb.Controllers
{
    public class PropertyController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PropertyController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult BecomeAHost()
        {
            return View(new PropertySubmissionViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BecomeAHost(PropertySubmissionViewModel model)
        {
            if (model.PropertyImages == null || !model.PropertyImages.Any(f => f.Length > 0))
            {
                ModelState.AddModelError("PropertyImages", "AN application is incompelte and  cannot be " +
                    "subbitted without at leat one property image");
            }
            if (ModelState.IsValid) {
            return View(model);
            }
            var newProperty = new PropertyApplication { 
            
            Title = model.Title,
            Description = model.Description,
            Price = model.Price,
            Address = model.Address,
            HostId = User.Identity.Name,
            Status = ApplicationStatus.Pending
            };
            foreach(var file in model.PropertyImages)
            {
                if(file.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    newProperty.Images.Add(new PropertyImage { ImageUrl = $"/upload/{file}" });
                }
            }


            return View(newProperty);

        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
