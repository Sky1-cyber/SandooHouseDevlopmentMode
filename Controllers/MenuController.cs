using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sandoohouse.ApplicationProgram;
using Sandoohouse.Helpers;
using Sandoohouse.Models;
using Sandoohouse.Models.ModelViewer.MenuModelViewer;

namespace Sandoohouse.Controllers;

public class MenuController : Controller
{
    private readonly ApplicationDbContext _applicationDbContext;
    public MenuController(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }
    
    // GET
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> Index()
    {
        var menus = await _applicationDbContext.Menus
            .Include(m => m.Category)
            .Select(m => new MenuViewerModel
            {
                Id = m.Id,
                MenuName = m.MenuName,
                Price = m.Price,
                DiscountPrice = m.DiscountPrice,
                Status = m.Status,
                CategoryId = m.CategoryId,
                CategoryName = m.Category != null ? m.Category.CategoryName : "No Category",
                ImageMenuUrl = m.ImageMenuUrl
            })
            .ToListAsync();
        
        return View(menus);
    }
    
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> CreateMenu()
    {
        var categories = await _applicationDbContext.Categories
            .Where(c => c.Status)
            .ToListAsync();
        ViewBag.Categories = categories;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> CreateMenu(MenuViewerModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _applicationDbContext.Categories
                .Where(c => c.Status)
                .ToListAsync();
            return View(model);
        }

        var existingMenu = await _applicationDbContext.Menus
            .AnyAsync(m => m.MenuName == model.MenuName);

        if (existingMenu)
        {
            ModelState.AddModelError("MenuName", "Menu name already exists.");

            ViewBag.Categories = await _applicationDbContext.Categories
                .Where(c => c.Status)
                .ToListAsync();

            return View(model);
        }

        string? fileName = null;
        if (model.ImageFile != null)
        {
            fileName = await FileUploadHelper.UploadImage(model.ImageFile, "Menu");
        }

        var adminId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        var menu = new Menu
        {
            MenuName = model.MenuName,
            Price = model.Price,
            DiscountPrice = model.DiscountPrice,
            Status = model.Status,
            CategoryId = model.CategoryId,
            ImageMenuUrl = fileName,
            CreatedBy = adminId
        };

        _applicationDbContext.Menus.Add(menu);
        await _applicationDbContext.SaveChangesAsync();

        return RedirectToAction("Index", "Menu");
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> EditMenu(int? id)
    {
        if (id == null)
            return NotFound();

        var menu = await _applicationDbContext.Menus
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (menu == null)
            return NotFound();

        var modelMenu = new MenuViewerModel
        {
            Id = menu.Id,
            MenuName = menu.MenuName,
            Price = menu.Price,
            DiscountPrice = menu.DiscountPrice,
            Status = menu.Status,
            CategoryId = menu.CategoryId,
            CategoryName = menu.Category?.CategoryName,
            ImageMenuUrl = menu.ImageMenuUrl
        };

        ViewBag.Categories = await _applicationDbContext.Categories
            .Where(c => c.Status)
            .ToListAsync();

        return View(modelMenu);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> EditMenu(MenuViewerModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _applicationDbContext.Categories
                .Where(c => c.Status)
                .ToListAsync();

            return View(model);
        }

        var menu = await _applicationDbContext.Menus
            .FirstOrDefaultAsync(m => m.Id == model.Id);

        if (menu == null)
            return NotFound();

        var exists = await _applicationDbContext.Menus
            .AnyAsync(m => m.MenuName == model.MenuName && m.Id != model.Id);
        
        if (exists)
        {
            ModelState.AddModelError("MenuName", "Menu name already exists.");
            ViewBag.Categories = await _applicationDbContext.Categories
                .Where(c => c.Status)
                .ToListAsync();
            return View(model);
        }

        if (model.ImageFile != null)
        {
            if (!string.IsNullOrEmpty(menu.ImageMenuUrl))
            {
                var oldFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Menu", menu.ImageMenuUrl);
                if (System.IO.File.Exists(oldFile))
                    System.IO.File.Delete(oldFile);
            }

            menu.ImageMenuUrl = await FileUploadHelper.UploadImage(model.ImageFile, "Menu");
        }

        menu.MenuName = model.MenuName;
        menu.Price = model.Price;
        menu.DiscountPrice = model.DiscountPrice;
        menu.Status = model.Status;
        menu.CategoryId = model.CategoryId;
        menu.UpdatedAt = DateTime.UtcNow;

        _applicationDbContext.Menus.Update(menu);
        await _applicationDbContext.SaveChangesAsync();

        return RedirectToAction("Index", "Menu");
    }
    
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Owner")]
    public async Task<IActionResult> DeleteMenu(int? id)
    {
        if (id == null)
            return NotFound();

        var menu = await _applicationDbContext.Menus.FindAsync(id);
        if (menu == null)
            return NotFound();

        // Delete menu image if exists
        if (!string.IsNullOrEmpty(menu.ImageMenuUrl))
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot/Menu",
                menu.ImageMenuUrl);

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        _applicationDbContext.Menus.Remove(menu);
        await _applicationDbContext.SaveChangesAsync();

        return RedirectToAction("Index", "Menu");
    }
}