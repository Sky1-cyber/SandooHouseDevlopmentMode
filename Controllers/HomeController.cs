using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sandoohouse.ApplicationProgram;
using Sandoohouse.Models;
using Sandoohouse.Models.Enum;

namespace Sandoohouse.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public HomeController(ApplicationDbContext applicationDbContext, IWebHostEnvironment webHostEnvironment)
    {
        _applicationDbContext = applicationDbContext;
        _webHostEnvironment = webHostEnvironment;
    }
    
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> Index()
    {
        var orders = await _applicationDbContext.Orders
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return View(orders);
    }
    
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string Email, string Password, bool rememberMe)
    {
        var admin = _applicationDbContext.Admins
            .FirstOrDefault(x => x.Email == Email);

        if (admin == null)
        {
            TempData["ErrorMessage"] = "Invalid email or password";
            return RedirectToAction("Login", "Home");
        }

        if (admin.Status == Status.Suspended)
        {
            TempData["ErrorMessage"] = "Your account has been suspended.";
            return RedirectToAction("Login", "Home");
        }

        bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(Password, admin.Password);

        if (!isPasswordCorrect)
        {
            TempData["ErrorMessage"] = "Incorrect password";
            return RedirectToAction("Login", "Home");
        }

        admin.Status = Status.Active;
        _applicationDbContext.Admins.Update(admin);
        await _applicationDbContext.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Email, admin.Email ?? ""),
            new Claim(ClaimTypes.Name, admin.FirstName ?? ""),
            new Claim("LastName", admin.LastName ?? ""),
            new Claim(ClaimTypes.MobilePhone, admin.PhoneNumber ?? ""),
            new Claim("ProfileImageFile", admin.ProfileImageFile ?? ""),
            new Claim(ClaimTypes.Role, admin.Role.ToString())
        };

        var claimIdentity = new ClaimsIdentity(claims, "MyCookieAuthenticationScheme");

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe
                ? DateTimeOffset.UtcNow.AddDays(30)
                : DateTimeOffset.UtcNow.AddHours(1)
        };

        await HttpContext.SignInAsync(
            "MyCookieAuthenticationScheme",
            new ClaimsPrincipal(claimIdentity),
            authProperties);

        return admin.Role switch
        {
            Role.Cashier => RedirectToAction("CreateOrder", "Order"),
            _ => RedirectToAction("Index", "Home")
        };
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}