using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sandoohouse.ApplicationProgram;
using Sandoohouse.Helpers;
using Sandoohouse.Models;
using Sandoohouse.Models.ModelViewer.CategoryModelViewer;
using Sandoohouse.Service;

namespace Sandoohouse.Controllers;

public class CategoryController : Controller
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly CloudinaryService _cloudinaryService;
    public CategoryController(ApplicationDbContext applicationDbContext, CloudinaryService cloudinaryService)
    {
        _applicationDbContext = applicationDbContext;
        _cloudinaryService = cloudinaryService;
    }

    // GET to get list of category
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> Index()
    {
        var categories = await _applicationDbContext.Categories
            .Include(c => c.Brand) 
            .OrderBy(c => c.Id)
            .ToListAsync();
        return View(categories);
    }

    // GET From to create category
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner")]
    public async Task<IActionResult> CreateCategory()
    {
        var brands = await _applicationDbContext.Brands
            .Where(b => b.Status)
            .OrderBy(b => b.BrandName)
            .ToListAsync();
    
        ViewBag.Brands = new SelectList(brands, "Id", "BrandName");
    
        var model = new CategoryViewerModel();
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Owner")]
    public async Task<IActionResult> CreateCategory(CategoryViewerModel model)
    {
        if (!ModelState.IsValid)
        {
            var brandsRetry = await _applicationDbContext.Brands
                .Where(b => b.Status)
                .OrderBy(b => b.BrandName)
                .ToListAsync();
            ViewBag.Brands = new SelectList(brandsRetry, "Id", "BrandName", model.BrandId);
            return View(model);
        }

        // Upload image to Cloudinary
        string? imageUrl = null;
        if (model.ImageFile != null)
        {
            imageUrl = await _cloudinaryService.UploadImageAsync(
                model.ImageFile,
                folder: "Category"
            );
        }

        var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var category = new Category
        {
            CategoryName     = model.CategoryName,
            Description      = model.Description,
            CategoryImageUrl = imageUrl,
            Status           = model.Status,
            CreatedById      = adminId,
            BrandId          = model.BrandId
        };

        _applicationDbContext.Categories.Add(category);
        await _applicationDbContext.SaveChangesAsync();

        return RedirectToAction("Index", "Category");
    }
    
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> ViewCategory(int? id)
    {
        if (id <= 0)
            return NotFound();
        var category = await  _applicationDbContext.Categories
            .Include(b => b.Brand)
            .Include(m => m.Menus)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            return NotFound();
        return View(category);
    }
    
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> EditCategory(int id)
    {
        var category = await _applicationDbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            return NotFound();

        var brands = await _applicationDbContext.Brands
            .Where(b => b.Status)
            .OrderBy(b => b.BrandName)
            .ToListAsync();

        ViewBag.Brands = new SelectList(brands, "Id", "BrandName");

        var model = new CategoryViewerModel
        {
            Id = category.Id,
            CategoryName = category.CategoryName,
            Description = category.Description,
            Status = category.Status,
            CategoryImageUrl = category.CategoryImageUrl,
            BrandId = category.BrandId,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            CreatedByName = await _applicationDbContext.Admins
                .Where(a => a.Id == category.CreatedById)
                .Select(a => a.Email)
                .FirstOrDefaultAsync()
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> EditCategory(CategoryViewerModel model)
    {
        if (!ModelState.IsValid)
        {
            var brands = await _applicationDbContext.Brands
                .Where(b => b.Status)
                .OrderBy(b => b.BrandName)
                .ToListAsync();
            ViewBag.Brands = new SelectList(brands, "Id", "BrandName", model.BrandId);
            return View(model);
        }

        var category = await _applicationDbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == model.Id);

        if (category == null)
            return NotFound();

        if (model.RemoveImage == "true" && !string.IsNullOrEmpty(category.CategoryImageUrl))
        {
            await _cloudinaryService.DeleteImageAsync(category.CategoryImageUrl);
            category.CategoryImageUrl = null;
        }

        if (model.ImageFile != null)
        {
            await _cloudinaryService.DeleteImageAsync(category.CategoryImageUrl);

            category.CategoryImageUrl = await _cloudinaryService.UploadImageAsync(
                model.ImageFile,
                folder: "Category"
            );
        }

        category.CategoryName = model.CategoryName;
        category.Description  = model.Description;
        category.Status       = model.Status;
        category.BrandId      = model.BrandId;
        category.UpdatedAt    = DateTime.UtcNow;

        _applicationDbContext.Categories.Update(category);
        await _applicationDbContext.SaveChangesAsync();

        return RedirectToAction("Index", "Category");
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Owner")]
    public async Task<IActionResult> DeleteCategory(int? id)
    {
        if (id == null)
            return NotFound();

        var category = await _applicationDbContext.Categories.FindAsync(id);
        if (category == null)
            return NotFound();

        var menus = await _applicationDbContext.Menus
            .Where(m => m.CategoryId == id)
            .ToListAsync();

        foreach (var menu in menus) menu.CategoryId = null;

        await _cloudinaryService.DeleteImageAsync(category.CategoryImageUrl);

        _applicationDbContext.Categories.Remove(category);
        await _applicationDbContext.SaveChangesAsync();

        return RedirectToAction("Index", "Category");
    }
}