using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity().AddEntityFramework<ApplicationDbContext>();
builder.Services.AddAuthentication(options => {
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin", "SuperAdmin"));
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("SuperAdmin"));

});
var app = builder.Build();

app.MapGet("/api/property",async (string? city, string? type, ApplicationDbContext db) =>
{
    var query = db.Properties.Where(p => p.IsActive);
    if(!string.IsNullOrEmpty(city))
        query = query.Where(p => p.City.ToLower() == city.ToLower());

    if(!string.IsNullOrEmpty(type))
        query = query.Where(p => p.PropertyType.ToLower() == type.ToLower());
    var listings = await query.ToListAsync();
    return Results.Ok(listings);
});

public record BookingRequest(int PropertyId, DateTime CheckIn, DateTime CheckOut, int Guests);

app.MapPost("/api/booking", async (BookingRequest request, ApplicationDbContext db, ClaimsPrincipal user) => {
    var property = await db.Properties.FindAsync(request.PropertyId);
    if (property == null) return Results.NotFound("Property not found");

    int nights = (request.CheckOut - request.CheckIn).Days;
    if (nights <= 0) return Results.BadRequest("Check-out date must be after check-in date");

    decimal totalPrice = (property.PricePerNight * nights) + property.CleaningFee + property.ServiceFee;

    var booking = new Booking
    {
        PropertyId = request.PropertyId,
        GuestId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Guest_1",
        CheckInDate = request.CheckInDate,
        CheckOutDate = request.CheckOut,
        NumberOfGuests = request.Guests,
        TotalPrice = totalPrice,
        Status = BookingStatus.Pending,
    };
    db.Booking.Add(booking);
    await db.SaveChangesAsync();
    return Results.Created($"/api/booking/{booking.Id}", booking);
});

app.MapPost("/api/bookings/{id}/upload-document", async (int id, IFormFile file, ApplicationDbContext db) => {

    var booking = await db.Bookings.FindAsync(id);
    if(booking == null) return Results.NotFound("Booking record missing");
    if (booking.Status != BookingStatus.Approved) return Results.BadRequest("Booking is not approved yet");

    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
    var extensions = Path.GetExtension(file.FileName).ToLowerInvariant();
    if (!allowedExtensions.Contains(extensions)) return Results.BadRequest("Invalid file type");

    var secureFolder = Path.Combine(Directory.GetCurrentDirectory(), "SecureStorage", "GuestDocs");
    Directory.CreateDirectory(secureFolder);

    var unigueFileName = $"{Guid.NewGuid()}{extensions}";
    var filePath = Path.Combine(secureFolder, unigueFileName);

    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }

    booking.GuestDocumentPath = filePath;
    booking.IsDocumentUploaded = true;
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "Document uploaded successfully. Awaiting Host verification" });


});

app.MapGet("/api/host/booking/{id}/view-document", async (int id, ApplicationDbContext db) => {

    var booking = await db.Bookings.FindAsync(id);
    if (booking == null || string.IsNullOrEmpty(booking.GuestDocumentPath))
        return Results.NotFound("No document associated with this booking");

    if (!File.Exists(booking.GuestDocumentPath))
        return Results.NotFound("Physical file was deleted or misplaced");

    var fileBytes = await File.ReadAllBytesAsync(booking.GuestDocumentPath);

    return Results.File(fileBytes, "application/octet-stream", Path.GetFileName(booking.GuestDocumentPath));
});
    
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await RoleSeeder.SeedRolesAndSuperAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database roles");
    }

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRouting();

    app.UseAuthorization();

    app.MapStaticAssets();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();


    app.Run();
}
