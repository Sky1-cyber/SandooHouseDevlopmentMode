using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sandoohouse.ApplicationProgram;
using Sandoohouse.Models.ModelViewer.SupplierModelViewer;

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
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> Index()
    {
        var suppliers = await _applicationDbContext.Suppliers
            .OrderBy(s => s.CompanyName)
            .Select(s => new SupplierViewModel
            {
                SupplierId = s.SupplierId,
                CompanyName = s.CompanyName,
                CompanyProfile = s.CompanyProfile,
                ContactPerson = s.ContactPerson,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                City = s.City,
                State = s.State,
                Country = s.Country,
                Status = s.Status,
                Notes = s.Notes,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync();

        return View(suppliers);
    }
    
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public IActionResult AddSupplier()
    {
        
        return RedirectToAction("Index", "ComingSoon");
    }
}