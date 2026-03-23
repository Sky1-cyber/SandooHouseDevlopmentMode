using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sandoohouse.ApplicationProgram;
using Sandoohouse.Helpers;
using Sandoohouse.Models;
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
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSupplier(SupplierViewModel supplierViewModel)
    {
        if (!ModelState.IsValid)
            return View(supplierViewModel);
        var fileName = await FileUploadHelper.UploadImage(
            supplierViewModel.CompanyProfileFile,
            "uploads/suppliers"
            );
        var supplier = new Supplier
        {
            CompanyName = supplierViewModel.CompanyName!,
            CompanyProfile = fileName,
            ContactPerson = supplierViewModel.ContactPerson!,
            Phone = supplierViewModel.Phone!,
            Email = supplierViewModel.Email,
            Address = supplierViewModel.Address,
            City = supplierViewModel.City,
            State = supplierViewModel.State,
            Country = supplierViewModel.Country,
            Status = supplierViewModel.Status,
            Notes = supplierViewModel.Notes,
            CreatedAt = DateTime.UtcNow
        };
        _applicationDbContext.Add(supplier);
        await _applicationDbContext.SaveChangesAsync();
        return RedirectToAction("Index", "Supplier");
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> DeleteSupplier(int? id)
    {
        var supplier = await _applicationDbContext.Suppliers.FindAsync(id);
        if (supplier == null)
            return NotFound();
        if (!string.IsNullOrEmpty(supplier.CompanyProfile))
        {
            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/uploads/suppliers",
                supplier.CompanyProfile
                );
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }
        _applicationDbContext.Remove(supplier);
        await _applicationDbContext.SaveChangesAsync();
        TempData["Message"] = "Supplier deleted successfully";
        return RedirectToAction("Index", "Supplier");
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> EditSupplier(int? id)
    {
        if (id == null)
            return NotFound();
        var supplier = await _applicationDbContext.Suppliers.FindAsync(id);
        var suppliersModel = new SupplierViewModel
        {
            SupplierId = supplier!.SupplierId,
            CompanyName = supplier.CompanyName,
            CompanyProfile = supplier.CompanyProfile,
            ContactPerson = supplier.ContactPerson,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address,
            City = supplier.City,
            State = supplier.State,
            Country = supplier.Country,
            Status = supplier.Status,
            Notes = supplier.Notes
        };
        return View(suppliersModel);
    }
    
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSupplier(SupplierViewModel supplierViewModel)
    {
        if (!ModelState.IsValid)
            return View(supplierViewModel);
        var supplier = await _applicationDbContext.Suppliers.FindAsync(supplierViewModel.SupplierId);
        if (supplier == null)
            return NotFound();
        if (supplierViewModel.CompanyProfileFile != null)
        {
            if (!string.IsNullOrEmpty(supplierViewModel.CompanyProfile))
            {
                var path = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/uploads/suppliers",
                    supplierViewModel.CompanyProfile
                    );
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }

            supplier.CompanyProfile = await FileUploadHelper.UploadImage(
                supplierViewModel.CompanyProfileFile,
                "uploads/suppliers"
                );
        }
        supplier.CompanyName = supplierViewModel.CompanyName!;
        supplier.ContactPerson = supplierViewModel.ContactPerson!;
        supplier.Phone = supplierViewModel.Phone!;
        supplier.Email = supplierViewModel.Email;
        supplier.Address = supplierViewModel.Address;
        supplier.City = supplierViewModel.City;
        supplier.State = supplierViewModel.State;
        supplier.Country = supplierViewModel.Country;
        supplier.Status = supplierViewModel.Status;
        supplier.Notes = supplierViewModel.Notes;
        supplier.UpdatedAt = DateTime.UtcNow;
        await _applicationDbContext.SaveChangesAsync();
        TempData["Message"] = "Supplier updated successfully";
        return RedirectToAction("Index", "Supplier");
    }

    public async Task<IActionResult> ViewSupplier(int? id)
    {
        if (id == null)
            return NotFound();
        var suppliers = await _applicationDbContext.Suppliers
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(s => s.SupplierId == id);
        return View(suppliers);
    }
}