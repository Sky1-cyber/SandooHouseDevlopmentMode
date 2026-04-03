using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sandoohouse.ApplicationProgram;
using Sandoohouse.Models;

namespace Sandoohouse.Controllers;

[Authorize]
public class ShiftController : Controller
{
    private readonly ApplicationDbContext _applicationDbContext;

    public ShiftController(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    // ── Helper: get the current open shift ──────────────────────────────────
    private async Task<Shift?> GetCurrentShift()
    {
        return await _applicationDbContext.Shifts
            .OrderByDescending(s => s.StartTime)
            .FirstOrDefaultAsync(s => !s.IsClosed);
    }

    // ── Helper: force DateTime to have UTC kind so JSON serialiser
    //    always appends "Z" — without this, ASP.NET may emit
    //    "2025-04-03T08:00:00" (no Z) which browsers read as LOCAL time.
    private static DateTime AsUtc(DateTime dt)
        => DateTime.SpecifyKind(dt, DateTimeKind.Utc);

    // ── GET /Shift/GetCurrentShiftInfo ──────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetCurrentShiftInfo()
    {
        var shift = await GetCurrentShift();

        if (shift == null)
            return Ok(new { isOpen = false });

        var orders = await _applicationDbContext.Orders
            .Where(o => o.ShiftId == shift.Id)
            .ToListAsync();

        var revenue = orders.Sum(o => (decimal?)o.TotalAmount ?? 0);

        return Ok(new
        {
            isOpen      = true,
            shiftId     = shift.Id,
            startTime   = AsUtc(shift.StartTime),   // ✅ "Z" suffix guaranteed
            openingCash = shift.OpeningCash,
            orderCount  = orders.Count,
            revenue
        });
    }

    // ── POST /Shift/OpenShift ───────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> OpenShift([FromBody] OpenShiftRequest request)
    {
        var existing = await GetCurrentShift();

        if (existing != null)
            return Ok(new
            {
                success     = true,
                alreadyOpen = true,
                shiftId     = existing.Id,
                startTime   = AsUtc(existing.StartTime), // ✅ send with Z
                message     = "Shift already open"
            });

        var firstName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "";
        var lastName  = User.Claims.FirstOrDefault(c => c.Type == "LastName")?.Value ?? "";
        var cashier   = $"{firstName} {lastName}".Trim();

        var shift = new Shift
        {
            StartTime   = DateTime.UtcNow,  // ✅ always UTC
            IsClosed    = false,
            OpeningCash = request.OpeningCash,
            CashierName = cashier
        };

        _applicationDbContext.Shifts.Add(shift);
        await _applicationDbContext.SaveChangesAsync();

        return Ok(new
        {
            success   = true,
            shiftId   = shift.Id,
            startTime = AsUtc(shift.StartTime), // ✅ JS parseUTC gets "Z" suffix
            message   = "Shift opened"
        });
    }

    // ── POST /Shift/CloseShift ──────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> CloseShift()
    {
        var shift = await GetCurrentShift();

        if (shift == null)
            return Ok(new { success = false, message = "No active shift found" });

        var orders = await _applicationDbContext.Orders
            .Where(o => o.ShiftId == shift.Id)
            .ToListAsync();

        shift.TotalSales  = orders.Sum(o => (decimal?)o.TotalAmount ?? 0);
        shift.TotalOrders = orders.Count;
        shift.EndTime     = DateTime.UtcNow;    // ✅ always UTC
        shift.IsClosed    = true;
        shift.ClosedBy    = User.Identity?.Name;

        await _applicationDbContext.SaveChangesAsync();

        var durationMinutes = (shift.EndTime.Value - shift.StartTime).TotalMinutes;

        return Ok(new
        {
            success          = true,
            shiftId          = shift.Id,
            totalSales       = shift.TotalSales,
            totalOrders      = shift.TotalOrders,
            openingCash      = shift.OpeningCash,
            estimatedDrawer  = shift.OpeningCash + shift.TotalSales,
            startTime        = AsUtc(shift.StartTime),    // ✅
            endTime          = AsUtc(shift.EndTime.Value), // ✅
            durationMinutes  = Math.Round(durationMinutes, 1),
            cashier          = shift.CashierName ?? shift.ClosedBy
        });
    }

    // ── GET /Shift/ShiftSummary/{id} ────────────────────────────────────────
    public async Task<IActionResult> ShiftSummary(int id)
    {
        var shift = await _applicationDbContext.Shifts
            .Include(s => s.Orders)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (shift == null)
            return NotFound();

        return View(shift);
    }
}

// ── Request DTO ─────────────────────────────────────────────────────────────
public class OpenShiftRequest
{
    public decimal OpeningCash { get; set; }
}