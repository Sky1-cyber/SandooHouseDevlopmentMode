using Microsoft.EntityFrameworkCore;
using Sandoohouse.ApplicationProgram;
using Sandoohouse.Models;
using Sandoohouse.Models.Enum;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthentication("MyCookieAuthenticationScheme")
    .AddCookie("MyCookieAuthenticationScheme", options =>
    {
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Error/AccessDenied";
    });

builder.Services.AddDistributedMemoryCache(); // required for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// --- Default Admin Seeding ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Only create default admin if not exists
    if (!db.Admins.Any(a => a.Email == "admin@example.com"))
    {
        var defaultAdmin = new Admin
        {
            FirstName = "Default",
            LastName = "Admin",
            Email = "admin@example.com",
            PhoneNumber = "0000000000",
            Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Status = Status.Active,
            IsOnline = false,
            CreatedAt = DateTime.UtcNow
        };

        db.Admins.Add(defaultAdmin);
        db.SaveChanges();
    }
}
// ----------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

app.Run();