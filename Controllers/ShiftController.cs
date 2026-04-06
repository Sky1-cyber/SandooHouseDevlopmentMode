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

    // Cambodia timezone (UTC+7)
    private static readonly TimeZoneInfo CambodiaZone =
        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    public ShiftController(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    // ── Helper: get Cambodia local time ─────────────────────────────────────
    private static DateTime NowCambodia()
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, CambodiaZone);

    // ── Helper: get the current open shift ──────────────────────────────────
    private async Task<Shift?> GetCurrentShift()
    {
        return await _applicationDbContext.Shifts
            .OrderByDescending(s => s.StartTime)
            .FirstOrDefaultAsync(s => !s.IsClosed);
    }

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

        // Convert stored StartTime to Cambodia time for display
        var startTimeCambodia = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(shift.StartTime, DateTimeKind.Utc), CambodiaZone);

        return Ok(new
        {
            isOpen       = true,
            shiftId      = shift.Id,
            startTime    = startTimeCambodia.ToString("yyyy-MM-ddTHH:mm:ss"),
            openingCash  = shift.OpeningCash,
            orderCount   = orders.Count,
            revenue
        });
    }

    // ── POST /Shift/OpenShift ───────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> OpenShift([FromBody] OpenShiftRequest request)
    {
        var existing = await GetCurrentShift();

        if (existing != null)
        {
            var existingStart = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(existing.StartTime, DateTimeKind.Utc), CambodiaZone);

            return Ok(new
            {
                success     = true,
                alreadyOpen = true,
                shiftId     = existing.Id,
                startTime   = existingStart.ToString("yyyy-MM-ddTHH:mm:ss"),
                message     = "Shift already open"
            });
        }

        var firstName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "";
        var lastName  = User.Claims.FirstOrDefault(c => c.Type == "LastName")?.Value ?? "";
        var cashier   = $"{firstName} {lastName}".Trim();

        var nowCambodia = NowCambodia();

        var shift = new Shift
        {
            // Store as UTC in DB — always best practice
            StartTime    = DateTime.UtcNow,
            IsClosed     = false,
            OpeningCash  = request.OpeningCash,
            CashierName  = cashier
        };

        _applicationDbContext.Shifts.Add(shift);
        await _applicationDbContext.SaveChangesAsync();

        return Ok(new
        {
            success   = true,
            shiftId   = shift.Id,
            // Return Cambodia local time string to frontend
            startTime = nowCambodia.ToString("yyyy-MM-ddTHH:mm:ss"),
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
        // Store UTC in DB
        shift.EndTime     = DateTime.UtcNow;
        shift.IsClosed    = true;
        shift.ClosedBy    = User.Identity?.Name;

        await _applicationDbContext.SaveChangesAsync();

        // Convert both times to Cambodia local for the response
        var startCambodia = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(shift.StartTime, DateTimeKind.Utc), CambodiaZone);

        var endCambodia = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(shift.EndTime!.Value, DateTimeKind.Utc), CambodiaZone);

        var durationMinutes = (endCambodia - startCambodia).TotalMinutes;

        return Ok(new
        {
            success         = true,
            shiftId         = shift.Id,
            totalSales      = shift.TotalSales,
            totalOrders     = shift.TotalOrders,
            openingCash     = shift.OpeningCash,
            estimatedDrawer = shift.OpeningCash + shift.TotalSales,
            // Return ISO strings in Cambodia time — no timezone confusion on JS side
            startTime       = startCambodia.ToString("yyyy-MM-ddTHH:mm:ss"),
            endTime         = endCambodia.ToString("yyyy-MM-ddTHH:mm:ss"),
            durationMinutes = Math.Round(durationMinutes, 1),
            cashier         = shift.CashierName ?? shift.ClosedBy
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

        // Convert times to Cambodia before sending to view
        ViewBag.StartTimeCambodia = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(shift.StartTime, DateTimeKind.Utc), CambodiaZone);

        ViewBag.EndTimeCambodia = shift.EndTime.HasValue
            ? TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(shift.EndTime.Value, DateTimeKind.Utc), CambodiaZone)
            : (DateTime?)null;

        return View(shift);
    }
    
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var shift = await _applicationDbContext.Shifts
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shift == null)
            {
                return NotFound(new { success = false, message = "Shift not found" });
            }

            if (!shift.IsClosed)
            {
                return BadRequest(new { success = false, message = "Cannot delete an open shift. Please close it first." });
            }

            // Check if there are related orders
            var hasOrders = await _applicationDbContext.Orders.AnyAsync(o => o.ShiftId == id);
            if (hasOrders)
            {
                return BadRequest(new { 
                    success = false, 
                    message = "Cannot delete shift with associated orders. Delete the orders first." 
                });
            }

            _applicationDbContext.Shifts.Remove(shift);
            await _applicationDbContext.SaveChangesAsync();

            return Ok(new { success = true, message = "Shift deleted successfully" });
        }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"Error deleting shift: {ex.Message}");
            return StatusCode(500, new { success = false, message = "An error occurred while deleting the shift" });
        }
    }
}

// ── Request DTO ─────────────────────────────────────────────────────────────
public class OpenShiftRequest
{
    public decimal OpeningCash { get; set; }
}