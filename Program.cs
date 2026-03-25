using Microsoft.EntityFrameworkCore;
using Sandoohouse.ApplicationProgram;
using Sandoohouse.Models;
using Sandoohouse.Models.Enum;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);

// --- Build connection string dynamically ---
string connectionString;

var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPass = Environment.GetEnvironmentVariable("DB_PASS");

if (!string.IsNullOrEmpty(dbHost) &&
    !string.IsNullOrEmpty(dbPort) &&
    !string.IsNullOrEmpty(dbName) &&
    !string.IsNullOrEmpty(dbUser) &&
    !string.IsNullOrEmpty(dbPass))
{
    connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPass};SSL Mode=Require;Trust Server Certificate=true;";
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
}

// --- Services ---
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddAuthentication("MyCookieAuthenticationScheme")
    .AddCookie("MyCookieAuthenticationScheme", options =>
    {
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Error/AccessDenied";
    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// // ✅ Render PORT fix
// var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
// app.Urls.Add($"http://*:{port}");

// --- Database Migration + Safe Seeding ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        db.Database.Migrate();

        if (db.Database.CanConnect())
        {
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
                    CreatedAt = DateTime.UtcNow
                };

                db.Admins.Add(defaultAdmin);
                db.SaveChanges();
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Database migration error: " + ex.Message);
    }
}

// --- Middleware ---
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
