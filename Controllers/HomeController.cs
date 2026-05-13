using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sandoohouse.ApplicationProgram;
using Sandoohouse.Helpers;
using Sandoohouse.Models;
using Sandoohouse.Models.Enum;
using Sandoohouse.Service;

namespace Sandoohouse.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly SecurityService _securityService;

    public HomeController(ApplicationDbContext applicationDbContext, IWebHostEnvironment webHostEnvironment,
        SecurityService securityService)
    {
        _applicationDbContext = applicationDbContext;
        _webHostEnvironment = webHostEnvironment;
        _securityService = securityService;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> Index()
    {
        var now = DateTime.UtcNow;

        // Load orders + their items.
        // We do NOT ThenInclude(MenuItem) here because OrderItem.MenuItemId
        // is a plain int FK, not a navigation property.
        var orders = await _applicationDbContext.Orders
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        // ── Shift KPIs ────────────────────────────────────────────
        var shifts = await _applicationDbContext.Shifts
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();

        var currentShift = shifts.FirstOrDefault(s => !s.IsClosed);

        // Today's shifts
        var todayStart = DateTime.UtcNow.Date;
        var todayShifts = shifts
            .Where(s => s.StartTime.Date == todayStart)
            .ToList();

        // This month's shifts
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthShifts = shifts
            .Where(s => s.StartTime >= monthStart)
            .ToList();

        // ── Best sale day ─────────────────────────────────────────
        var bestSaleDay = orders
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new
            {
                Date = g.Key,
                Revenue = g.Sum(o => o.TotalAmount),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Revenue)
            .FirstOrDefault();

        // ── Best sale hour ────────────────────────────────────────
        var bestHour = orders
            .GroupBy(o => o.OrderDate.Hour)
            .Select(g => new
            {
                Hour = g.Key,
                Revenue = g.Sum(o => o.TotalAmount),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Revenue)
            .FirstOrDefault();

        // ── Best selling item ─────────────────────────────────────
        var allItems = orders
            .SelectMany(o => o.OrderItems ?? new List<OrderItem>())
            .ToList();

        var bestItemGroup = allItems
            .GroupBy(i => i.MenuItemId)
            .Select(g => new
            {
                MenuItemId = g.Key,
                Qty = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.Subtotal)
            })
            .OrderByDescending(x => x.Qty)
            .FirstOrDefault();

        // Resolve menu name: use MenuName stored on OrderItem first,
        // then fall back to a DB lookup if the field is empty.
        var bestItemName = "—";
        if (bestItemGroup != null)
        {
            var sample = allItems.FirstOrDefault(i => i.MenuItemId == bestItemGroup.MenuItemId);
            bestItemName = sample?.Menu?.MenuName; // stored name on the order item

            if (string.IsNullOrWhiteSpace(bestItemName))
            {
                // Fall back: look up the menu table directly
                var menu = await _applicationDbContext.Menus
                    .FindAsync(bestItemGroup.MenuItemId);
                bestItemName = menu?.MenuName ?? "—";
            }
        }

        // ── Popular items this month ──────────────────────────────
        // Work only with this month's orders
        var monthOrdersList = orders
            .Where(o => o.OrderDate.Year == now.Year &&
                        o.OrderDate.Month == now.Month)
            .ToList();

        var monthItemGroups = monthOrdersList
            .SelectMany(o => o.OrderItems ?? new List<OrderItem>())
            .GroupBy(i => i.MenuItemId)
            .Select(g => new
            {
                MenuItemId = g.Key,
                Qty = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.Subtotal)
            })
            .OrderByDescending(x => x.Qty)
            .Take(8)
            .ToList();

        // Load all needed menu names in one DB call — avoids N+1 and
        // avoids relying on any navigation property or stored field.
        var menuIds = monthItemGroups.Select(x => x.MenuItemId).ToList();
        var menuNames = await _applicationDbContext.Menus
            .Where(m => menuIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, m => m.MenuName ?? "Item #" + m.Id);

        var popularItemsList = monthItemGroups.Select(g => new
        {
            g.MenuItemId,
            g.Qty,
            g.Revenue,
            Name = menuNames.TryGetValue(g.MenuItemId, out var n) ? n : "Item #" + g.MenuItemId
        }).ToList();

        ViewBag.PopularItemNames = popularItemsList.Select(x => x.Name).ToList();
        ViewBag.PopularItemQty = popularItemsList.Select(x => x.Qty).ToList();
        ViewBag.PopularItemRevenue = popularItemsList.Select(x => (double)x.Revenue).ToList();
        ViewBag.PopularItemTotal = popularItemsList.Sum(x => x.Qty);
        ViewBag.PopularItemCount = popularItemsList.Count;

        // ── ViewBag ───────────────────────────────────────────────
        ViewBag.CurrentShiftOpen = currentShift != null;
        ViewBag.CurrentShiftId = currentShift?.Id;
        ViewBag.CurrentShiftCashier = currentShift?.CashierName ?? currentShift?.ClosedBy ?? "—";
        ViewBag.CurrentShiftRevenue = currentShift?.TotalSales ?? 0m;
        ViewBag.CurrentShiftOrders = currentShift?.TotalOrders ?? 0;
        ViewBag.CurrentShiftOpening = currentShift?.OpeningCash ?? 0m;
        ViewBag.CurrentShiftStartTime = currentShift?.StartTime;

        ViewBag.TodayShiftCount = todayShifts.Count;
        ViewBag.TodayShiftRevenue = todayShifts.Sum(s => s.TotalSales);
        ViewBag.TodayShiftOrders = todayShifts.Sum(s => s.TotalOrders);

        ViewBag.MonthShiftCount = monthShifts.Count;
        ViewBag.MonthShiftRevenue = monthShifts.Sum(s => s.TotalSales);
        ViewBag.TotalShiftsAllTime = shifts.Count;

        ViewBag.BestSaleDayDate = bestSaleDay?.Date.ToString("dd MMM yyyy") ?? "—";
        ViewBag.BestSaleDayRevenue = bestSaleDay?.Revenue ?? 0m;
        ViewBag.BestSaleDayOrders = bestSaleDay?.Count ?? 0;
        ViewBag.BestHour = bestHour != null
            ? $"{bestHour.Hour:D2}:00 – {bestHour.Hour + 1:D2}:00"
            : "—";
        ViewBag.BestHourRevenue = bestHour?.Revenue ?? 0m;
        ViewBag.BestItemName = bestItemName;
        ViewBag.BestItemQty = bestItemGroup?.Qty ?? 0;

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
    var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

    // 🤖 Anti-bot delay
    await Task.Delay(1000);

    var ipRecord = _applicationDbContext.LoginAttempts
        .FirstOrDefault(x => x.IPAddress == ip);

    // ─── 🔒 IP-level lockout ─────────────────────────────────────────────────
    if (ipRecord != null && ipRecord.LockoutEnd.HasValue && ipRecord.LockoutEnd > DateTime.UtcNow)
    {
        var remaining = (int)Math.Ceiling((ipRecord.LockoutEnd.Value - DateTime.UtcNow).TotalSeconds);

        TempData["IsLockout"]          = true;
        TempData["LockoutSeconds"]     = remaining;          // ← consumed by JS
        TempData["ErrorMessage"]       = "Too many login attempts from this device.";

        return RedirectToAction("Login", "Home");
    }

    var admin        = _applicationDbContext.Admins.FirstOrDefault(x => x.Email == Email);
    var errorMessage = "Invalid email or password";

    // ─── ❌ Admin not found ───────────────────────────────────────────────────
    if (admin == null)
    {
        await _securityService.HandleIpFail(ipRecord, ip);
        TempData["ErrorMessage"] = errorMessage;
        return RedirectToAction("Login", "Home");
    }

    // ─── 🔒 Account-level lockout ────────────────────────────────────────────
    if (admin.LockoutEnd.HasValue && admin.LockoutEnd > DateTime.UtcNow)
    {
        var remaining = (int)Math.Ceiling((admin.LockoutEnd.Value - DateTime.UtcNow).TotalSeconds);

        TempData["IsLockout"]      = true;
        TempData["LockoutSeconds"] = remaining;              // ← consumed by JS
        TempData["ErrorMessage"]   = "This account is temporarily locked due to too many failed attempts.";

        return RedirectToAction("Login", "Home");
    }

    // ─── 🚫 Suspended ────────────────────────────────────────────────────────
    if (admin.Status == Status.Suspended)
    {
        TempData["ErrorMessage"] = "Your account has been suspended. Please contact your administrator.";
        return RedirectToAction("Login", "Home");
    }

    // ─── 🔑 Verify password ──────────────────────────────────────────────────
    var isPasswordCorrect = BCrypt.Net.BCrypt.Verify(Password, admin.Password);

    if (!isPasswordCorrect)
    {
        admin.FailedLoginAttempts++;

        // Lock account after 3 failed attempts for 5 minutes
        if (admin.FailedLoginAttempts >= 3)
        {
            admin.LockoutEnd          = DateTime.UtcNow.AddMinutes(5);
            admin.FailedLoginAttempts = 0;

            var remaining = 300; // 5 min in seconds

            _applicationDbContext.Admins.Update(admin);
            await _securityService.HandleIpFail(ipRecord, ip);
            await _applicationDbContext.SaveChangesAsync();

            TempData["IsLockout"]      = true;
            TempData["LockoutSeconds"] = remaining;
            TempData["ErrorMessage"]   = "Too many failed attempts. Your account has been locked for 5 minutes.";

            return RedirectToAction("Login", "Home");
        }

        _applicationDbContext.Admins.Update(admin);
        await _securityService.HandleIpFail(ipRecord, ip);
        await _applicationDbContext.SaveChangesAsync();

        // Tell the user how many attempts they have left
        var attemptsLeft = 3 - admin.FailedLoginAttempts;
        TempData["ErrorMessage"] = attemptsLeft == 1
            ? $"Invalid email or password. <strong>1 attempt remaining</strong> before lockout."
            : $"Invalid email or password. {attemptsLeft} attempts remaining before lockout.";

        return RedirectToAction("Login", "Home");
    }

    // ─── ✅ SUCCESS — reset counters ─────────────────────────────────────────
    admin.FailedLoginAttempts = 0;
    admin.LockoutEnd          = null;
    admin.Status              = Status.Active;

    _applicationDbContext.Admins.Update(admin);

    if (ipRecord != null)
    {
        ipRecord.AttemptCount = 0;
        ipRecord.LockoutEnd   = null;
    }

    await _applicationDbContext.SaveChangesAsync();

    // ─── 🔐 Build claims ─────────────────────────────────────────────────────
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, admin.Id.ToString()),
        new(ClaimTypes.Email,          admin.Email        ?? ""),
        new(ClaimTypes.Name,           admin.FirstName    ?? ""),
        new("LastName",                admin.LastName     ?? ""),
        new(ClaimTypes.MobilePhone,    admin.PhoneNumber  ?? ""),
        new("ProfileImageFile",        admin.ProfileImageFile ?? ""),
        new(ClaimTypes.Role,           admin.Role.ToString())
    };

    var identity = new ClaimsIdentity(claims, "MyCookieAuthenticationScheme");

    var authProperties = new AuthenticationProperties
    {
        IsPersistent = rememberMe,
        ExpiresUtc   = rememberMe
            ? DateTimeOffset.UtcNow.AddDays(30)
            : DateTimeOffset.UtcNow.AddHours(1)
    };

    await HttpContext.SignInAsync(
        "MyCookieAuthenticationScheme",
        new ClaimsPrincipal(identity),
        authProperties);

    return admin.Role switch
    {
        Role.Cashier => RedirectToAction("CreateOrder", "Order"),
        _            => RedirectToAction("Index", "Home")
    };
}

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}