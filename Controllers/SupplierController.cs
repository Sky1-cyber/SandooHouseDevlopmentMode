using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sandoohouse.ApplicationProgram;

namespace Sandoohouse.Controllers;

[Authorize]
public class SupplierController : Controller
{
    private readonly ApplicationDbContext _applicationDbContext;
    public SupplierController(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }
    
    // GET Supplier List
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var suppliers = await _applicationDbContext.Suppliers
            .OrderBy(s => s.CompanyName)
            .ToListAsync();
        return View(suppliers);
    }
    
    public IActionResult AddSupplier()
    {
        return View();
    }
}