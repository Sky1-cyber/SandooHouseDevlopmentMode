using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sandoohouse.ApplicationProgram;
using Sandoohouse.Helpers;
using Sandoohouse.Models;
using Sandoohouse.Models.ModelViewer.CategoryModelViewer;

namespace Sandoohouse.Controllers;

public class CategoryController : Controller
{
    private readonly ApplicationDbContext _applicationDbContext;

    public CategoryController(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
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
    public async Task<IActionResult> CreateCategory()  // Make this async
    {
        var brands = await _applicationDbContext.Brands
            .Where(b => b.Status)
            .OrderBy(b => b.BrandName)
            .ToListAsync();
    
        // FIX: Create SelectList for the dropdown
        ViewBag.Brands = new SelectList(brands, "Id", "BrandName");
    
        var model = new CategoryViewerModel();
        return View(model);
    }

    // POST To posting data of Category to database
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Owner")]
    public async Task<IActionResult> CreateCategory(CategoryViewerModel model)
    {
        // FIX: Repopulate brands if validation fails
        if (!ModelState.IsValid)
        {
            var brands = await _applicationDbContext.Brands
                .Where(b => b.Status)
                .OrderBy(b => b.BrandName)
                .ToListAsync();
            ViewBag.Brands = new SelectList(brands, "Id", "BrandName", model.BrandId); // Pass selected value
            return View(model);
        }

        string? fileName = null;

        // Only handle uploaded file, not string
        if (model.ImageFile != null) 
            fileName = await FileUploadHelper.UploadImage(model.ImageFile, "Category");

        var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var category = new Category
        {
            CategoryName = model.CategoryName,
            Description = model.Description,
            CategoryImageUrl = fileName,
            Status = model.Status,
            CreatedById = adminId,
            BrandId = model.BrandId  // FIX: Add this line to save the BrandId
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
            // Repopulate brands if model validation fails
            var brands = await _applicationDbContext.Brands
                .Where(b => b.Status)
                .OrderBy(b => b.BrandName)
                .ToListAsync();
            ViewBag.Brands = new SelectList(brands, "Id", "BrandName", model.BrandId);

            return View(model);
        }

        // Find the existing category
        var category = await _applicationDbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == model.Id);

        if (category == null)
            return NotFound();

        var fileName = category.CategoryImageUrl;

        // Handle image removal
        if (model.RemoveImage == "true" && !string.IsNullOrEmpty(category.CategoryImageUrl))
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Category", category.CategoryImageUrl);
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            fileName = null;
        }

        // Handle new image upload
        if (model.ImageFile != null)
        {
            fileName = await FileUploadHelper.UploadImage(model.ImageFile, "Category");

            // Delete old image if exists
            if (!string.IsNullOrEmpty(category.CategoryImageUrl))
            {
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Category",
                    category.CategoryImageUrl);
                if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
            }
        }

        // Update category properties
        category.CategoryName = model.CategoryName;
        category.Description = model.Description;
        category.Status = model.Status;
        category.CategoryImageUrl = fileName;
        category.BrandId = model.BrandId; // <-- Update Brand
        category.UpdatedAt = DateTime.UtcNow;

        _applicationDbContext.Categories.Update(category);
        await _applicationDbContext.SaveChangesAsync();

        return RedirectToAction("Index", "Category");
    }

    //POST to delete category from database
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Owner")]
    public async Task<IActionResult> DeleteCategory(int? id)
    {
        if (id == null)
            return NotFound();
        var category = await _applicationDbContext.Categories
            .FindAsync(id);
        if (category == null)
            return NotFound();
        var menus = await _applicationDbContext.Menus
            .Where(m => m.CategoryId == id)
            .ToListAsync();

        foreach (var menu in menus) menu.CategoryId = null;
        if (!string.IsNullOrEmpty(category.CategoryImageUrl))
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot/Category",
                category.CategoryImageUrl);

            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
        }

        _applicationDbContext.Categories.Remove(category);
        await _applicationDbContext.SaveChangesAsync();
        return RedirectToAction("Index", "Category");
    }
}