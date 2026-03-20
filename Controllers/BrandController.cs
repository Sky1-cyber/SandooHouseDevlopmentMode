using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sandoohouse.ApplicationProgram;
using Sandoohouse.Helpers;
using Sandoohouse.Models;
using Sandoohouse.Models.ModelViewer.BrandModelViewer;

namespace Sandoohouse.Controllers;

public class BrandController : Controller
{
    private readonly ApplicationDbContext _applicationDbContext;
    public BrandController(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }
    
    // GET
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var brands = await _applicationDbContext.Brands
            .OrderByDescending(b => b.CreatedAt) // Optional: order by newest first
            .ToListAsync();
        return View(brands);
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner")]
    public IActionResult CreateBrand()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Owner")]
    public async Task<IActionResult> CreateBrand(BrandViewerModel brandViewerModel)
    {
        if (!ModelState.IsValid)
            return View(brandViewerModel);
        string? filename = null;
        if (brandViewerModel.LogoFile != null)
        {
            filename = await FileUploadHelper.UploadImage(brandViewerModel.LogoFile, "BrandImage");
        }

        var brand = new Brand
        {
            BrandName = brandViewerModel.BrandName,
            Description = brandViewerModel.Description,
            LogoBrandUrl = filename,
            Status = brandViewerModel.Status,
            CreatedAt = DateTime.UtcNow,
        };
        _applicationDbContext.Brands.Add(brand);
        await _applicationDbContext.SaveChangesAsync();
        return RedirectToAction("Index", "Brand");
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner")]
    public async Task<IActionResult> ViewBrand(int? id)
    {
        if (id <= 0)
            return NotFound();
        var brand = await _applicationDbContext.Brands
            .Include(c => c.Categories)!
            .ThenInclude(m => m.Menus)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (brand == null)
            return NotFound();
        return View(brand);
    }
    
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> EditBrand(int? id)
    {
        if (id == null)
            return NotFound();
        var brand = await _applicationDbContext.Brands.FindAsync(id);
        if (brand == null)
            return NotFound();
        var brandModel = new BrandViewerModel()
        {
            Id = brand.Id,
            BrandName = brand.BrandName,
            Description = brand.Description,
            LogoBrandUrl = brand.LogoBrandUrl,
            Status = brand.Status,
            CreatedAt = brand.CreatedAt,
        };
        return View(brandModel);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> EditBrand(BrandViewerModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var brand = await _applicationDbContext.Brands.FindAsync(model.Id);

        if (brand == null)
            return NotFound();

        string? fileName = brand.LogoBrandUrl;

        // Upload new logo
        if (model.LogoFile != null)
        {
            // Delete old logo
            if (!string.IsNullOrEmpty(brand.LogoBrandUrl))
            {
                var oldPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "BrandImage",
                    brand.LogoBrandUrl
                );

                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            fileName = await FileUploadHelper.UploadImage(model.LogoFile, "BrandImage");
        }

        brand.BrandName = model.BrandName;
        brand.Description = model.Description;
        brand.LogoBrandUrl = fileName;
        brand.Status = model.Status;
        brand.UpdatedAt = DateTime.UtcNow;

        _applicationDbContext.Brands.Update(brand);
        await _applicationDbContext.SaveChangesAsync();

        return RedirectToAction("Index", "Brand");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Owner")]
    public async Task<IActionResult> DeleteBrand(int? id)
    {
        if (id == null)
            return NotFound();

        var brand = await _applicationDbContext.Brands.FindAsync(id);

        if (brand == null)
            return NotFound();

        // Delete logo from BrandImage folder
        if (!string.IsNullOrEmpty(brand.LogoBrandUrl))
        {
            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "BrandImage",
                brand.LogoBrandUrl
            );

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        _applicationDbContext.Brands.Remove(brand);
        await _applicationDbContext.SaveChangesAsync();

        return RedirectToAction("Index", "Brand");
    }
}