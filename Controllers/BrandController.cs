using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sandoohouse.ApplicationProgram;
using Sandoohouse.Helpers;
using Sandoohouse.Models;
using Sandoohouse.Models.ModelViewer.BrandModelViewer;
using Sandoohouse.Service;

namespace Sandoohouse.Controllers;

public class BrandController : Controller
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly CloudinaryService _cloudinaryService;
    public BrandController(ApplicationDbContext applicationDbContext, CloudinaryService cloudinaryService)
    {
        _applicationDbContext = applicationDbContext;
        _cloudinaryService = cloudinaryService;
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
     
            // Upload logo to Cloudinary (returns a permanent URL, not a local path)
            string? logoUrl = null;
            if (brandViewerModel.LogoFile != null)
            {
                logoUrl = await _cloudinaryService.UploadImageAsync(
                    brandViewerModel.LogoFile,
                    folder: "BrandImage"
                );
            }
     
            var brand = new Brand
            {
                BrandName    = brandViewerModel.BrandName,
                Description  = brandViewerModel.Description,
                LogoBrandUrl = logoUrl,          // stores the Cloudinary HTTPS URL
                Status       = brandViewerModel.Status,
                CreatedAt    = DateTime.UtcNow,
            };
     
            _applicationDbContext.Brands.Add(brand);
            await _applicationDbContext.SaveChangesAsync();
     
            TempData["Success"] = "Brand created successfully.";
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
    [Authorize(Roles = "SuperAdmin,Owner")]
    public async Task<IActionResult> EditBrand(BrandViewerModel brandViewerModel)
    {
        if (!ModelState.IsValid)
            return View(brandViewerModel);
 
        var brand = await _applicationDbContext.Brands.FindAsync(brandViewerModel.Id);
        if (brand == null)
            return NotFound();
 
        if (brandViewerModel.LogoFile != null)
        {
            await _cloudinaryService.DeleteImageAsync(brand.LogoBrandUrl);
 
            brand.LogoBrandUrl = await _cloudinaryService.UploadImageAsync(
                brandViewerModel.LogoFile,
                folder: "BrandImage"
            );
        }
 
        brand.BrandName   = brandViewerModel.BrandName;
        brand.Description = brandViewerModel.Description;
        brand.Status      = brandViewerModel.Status;
        brand.UpdatedAt   = DateTime.UtcNow;
        
        await _applicationDbContext.SaveChangesAsync();
        TempData["Success"] = "Brand updated successfully.";
        
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
        
        await _cloudinaryService.DeleteImageAsync(brand.LogoBrandUrl);

        _applicationDbContext.Brands.Remove(brand);
        await _applicationDbContext.SaveChangesAsync();

        return RedirectToAction("Index", "Brand");
    }
}