using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sandoohouse.ApplicationProgram;

namespace Sandoohouse.Controllers;

public class SaleController : Controller
{
    private readonly ApplicationDbContext _applicationDbContext;

    public SaleController(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    // GET /Sale/Index
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
    {
        var query = _applicationDbContext.Orders
            .Include(o => o.OrderItems)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            var fromUtc = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(o => o.OrderDate >= fromUtc);
        }

        if (toDate.HasValue)
        {
            var toUtc = DateTime.SpecifyKind(
                toDate.Value.Date.AddDays(1).AddTicks(-1),
                DateTimeKind.Utc);
            query = query.Where(o => o.OrderDate <= toUtc);
        }

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        ViewBag.TotalSales  = orders.Sum(o => o.TotalAmount - o.DiscountAmount);
        ViewBag.TotalOrders = orders.Count;
        ViewBag.FromDate    = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate      = toDate?.ToString("yyyy-MM-dd");

        return View(orders);
    }

    // GET /Sale/ShiftTable
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> ShiftTable(DateTime? fromDate, DateTime? toDate)
    {
        var query = _applicationDbContext.Shifts
            .AsQueryable();

        // Default: show current month if no filter supplied
        if (!fromDate.HasValue && !toDate.HasValue)
        {
            var firstOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1,
                                            0, 0, 0, DateTimeKind.Utc);
            query = query.Where(s => s.StartTime >= firstOfMonth);
        }

        if (fromDate.HasValue)
        {
            var fromUtc = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(s => s.StartTime >= fromUtc);
        }

        if (toDate.HasValue)
        {
            var toUtc = DateTime.SpecifyKind(
                toDate.Value.Date.AddDays(1).AddTicks(-1),
                DateTimeKind.Utc);
            query = query.Where(s => s.StartTime <= toUtc);
        }

        var shifts = await query
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();

        // Summary stats for the filtered range
        ViewBag.TotalShifts      = shifts.Count;
        ViewBag.TotalSales       = shifts.Sum(s => s.TotalSales);
        ViewBag.TotalOrders      = shifts.Sum(s => s.TotalOrders);
        ViewBag.TotalOpeningCash = shifts.Sum(s => s.OpeningCash);
        ViewBag.AvgSalesPerShift = shifts.Count > 0
            ? shifts.Average(s => s.TotalSales)
            : 0m;

        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate   = toDate?.ToString("yyyy-MM-dd");

        return View(shifts);
    }
}