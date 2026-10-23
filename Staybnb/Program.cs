using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Staybnb.Data;
using Staybnb.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services
    .AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login/Index";
    options.AccessDeniedPath = "/Login/Index";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole",
        policy => policy.RequireRole("Admin", "SuperAdmin"));

    options.AddPolicy("RequireSuperAdminRole",
        policy => policy.RequireRole("SuperAdmin"));
});
builder.Services.AddScoped<Staybnb.Services.NotificationService>();
builder.Services.AddScoped<Staybnb.Services.HostStatisticsService>();
var app = builder.Build();


app.MapGet("/api/property",
    async (string? city, string? type, ApplicationDbContext db) =>
    {
        var query = db.Properties.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(p => p.City.ToLower() == city.ToLower());

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(p => p.PropertyType.ToLower() == type.ToLower());

        var listings = await query.ToListAsync();
        return Results.Ok(listings);
    });


app.MapPost("/api/booking",
    async (BookingRequestDto request, ApplicationDbContext db, ClaimsPrincipal user) =>
    {
        var property = await db.Properties.FindAsync(request.PropertyId);

        if (property == null)
            return Results.NotFound("Property not found");

        int nights = (request.CheckOut - request.CheckIn).Days;

        if (nights <= 0)
            return Results.BadRequest("Check-out date must be after check-in date");

        if (request.Guests <= 0)
            return Results.BadRequest("At least one guest is required");

        decimal totalPrice =
            (property.PricePerNight * nights) +
            property.CleaningFee +
            property.ServiceFee;

        var guestId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(guestId))
            return Results.Unauthorized();

        var booking = new Booking
        {
            PropertyId = request.PropertyId,
            GuestId = guestId,
            CheckInDate = request.CheckIn,
            CheckOutDate = request.CheckOut,
            NumberOfGuests = request.Guests,
            TotalPrice = totalPrice,
            Status = BookingStatus.Pending
        };

        db.Bookings.Add(booking);
        await db.SaveChangesAsync();

        return Results.Created($"/api/booking/{booking.Id}", booking);
    });


app.MapPost("/api/bookings/{id}/upload-document",
    async (int id, IFormFile file, ApplicationDbContext db) =>
    {
        var booking = await db.Bookings.FindAsync(id);

        if (booking == null)
            return Results.NotFound("Booking record missing");

        if (booking.Status != BookingStatus.Approved)
            return Results.BadRequest("Booking is not approved yet");

        if (file == null || file.Length == 0)
            return Results.BadRequest("Please select a file");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return Results.BadRequest("Invalid file type");

        var secureFolder = Path.Combine(
            app.Environment.ContentRootPath,
            "SecureStorage",
            "GuestDocs");

        Directory.CreateDirectory(secureFolder);

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(secureFolder, uniqueFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        booking.GuestDocumentPath = filePath;
        booking.IsDocumentUploaded = true;

        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            message = "Document uploaded successfully. Awaiting Host verification"
        });
    });

// View/download a booking document
app.MapGet("/api/host/booking/{id}/view-document",
    async (int id, ApplicationDbContext db) =>
    {
        var booking = await db.Bookings.FindAsync(id);

        if (booking == null || string.IsNullOrEmpty(booking.GuestDocumentPath))
            return Results.NotFound("No document associated with this booking");

        if (!File.Exists(booking.GuestDocumentPath))
            return Results.NotFound("Physical file was deleted or misplaced");

        var fileBytes = await File.ReadAllBytesAsync(booking.GuestDocumentPath);

        return Results.File(
            fileBytes,
            "application/octet-stream",
            Path.GetFileName(booking.GuestDocumentPath));
    });

// Seed roles and the super admin
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
        logger.LogError(ex, "An error occurred while seeding database roles");
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

// Request DTO for the booking endpoint
public record BookingRequestDto(
    int PropertyId,
    DateTime CheckIn,
    DateTime CheckOut,
    int Guests);

