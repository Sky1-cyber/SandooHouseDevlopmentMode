using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sandoohouse.ApplicationProgram;
using Sandoohouse.Helpers;
using Sandoohouse.Models;
using Sandoohouse.Models.Enum;
using Sandoohouse.Models.ModelViewer.AdminModelViewer;

namespace Sandoohouse.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly ApplicationDbContext _applicationDbContext;
    public AccountController(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }
    
    // GET Profile image
    [HttpGet]
    public IActionResult Index(int id)
    {
        var admin = _applicationDbContext.Admins.FirstOrDefault(x => x.Id == id);
        if (admin == null)
            return NotFound();
        return View(admin);
    }
    
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);
        var admin = await _applicationDbContext.Admins.FindAsync(userId);
        if (admin != null)
        {
            admin.Status = Status.Inactive;
            await _applicationDbContext.SaveChangesAsync();
        }
        await HttpContext.SignOutAsync("MyCookieAuthenticationScheme");
        return RedirectToAction("Login", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(string Email)
    {
        TempData["Message"] = "Password reset link sent to your successfully!";
        return RedirectToAction("ForgotPassword",  "Account");
    }
    
    [HttpGet]
    public IActionResult CreateAccount()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var emailExist = _applicationDbContext.Admins
            .FirstOrDefault(x => x.Email == model.Email);

        if (emailExist != null)
        {
            TempData["ErrorMessage"] = "Email already exists";
            return View(model);
        }

        if (model.ProfileImageUrl != null)
        {
            string? fileName = await FileUploadHelper.UploadImage(model.ProfileImageUrl, "Admin");

            Admin admin = new Admin
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email!,
                PhoneNumber = model.PhoneNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                ProfileImageFile = fileName,
                Status = Status.Active,
                CreatedAt = DateTime.UtcNow
            };

            _applicationDbContext.Admins.Add(admin);
        }

        await _applicationDbContext.SaveChangesAsync();

        TempData["Success"] = "Register Success";

        return RedirectToAction("ListAccount", "Account");
    }
    
    // GET List of Admin in DB
    [HttpGet]
    public async Task<IActionResult> ListAccount()
    {
        int totalAdminsCount = _applicationDbContext.Admins.Count();
        ViewBag.TotalAdminsCount = totalAdminsCount;
        
        decimal totalIncomeReal = _applicationDbContext.Orders.Sum(o => o.TotalAmount) * 4100;
        decimal totalIncomeDollar = _applicationDbContext.Orders.Sum(o => o.TotalAmount);
        ViewBag.TotalIncome = totalIncomeReal;
        ViewBag.TotalIncomeDollar = totalIncomeDollar;
        var admins = await _applicationDbContext.Admins
            .OrderBy(x => x.FirstName)
            .ToListAsync();
        return View(admins);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var admin = await _applicationDbContext.Admins.FindAsync(id);

        if (admin == null)
            return NotFound();

        // Delete profile image if exists
        if (!string.IsNullOrEmpty(admin.ProfileImageFile))
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot/Admin",
                admin.ProfileImageFile);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        _applicationDbContext.Admins.Remove(admin);
        await _applicationDbContext.SaveChangesAsync();

        TempData["Message"] = "Account deleted successfully";

        return RedirectToAction("ListAccount", "Account");
    }

    [HttpGet]
    public async Task<IActionResult> EditAccount(int id)
    {
        var admin = await _applicationDbContext.Admins
            .FindAsync(id);
        if (admin == null)
            return NotFound();
        return View(admin);
    }
    
    [HttpPost]
    public async Task<IActionResult> EditAccount(int id, IFormFile? ProfileImageUrl, Admin model)
    {
        var admin = await _applicationDbContext.Admins.FindAsync(id);

        if (admin == null)
            return NotFound();

        admin.FirstName = model.FirstName;
        admin.LastName = model.LastName;
        admin.Email = model.Email;
        admin.Status = model.Status;
        admin.PhoneNumber = model.PhoneNumber;

        if (!string.IsNullOrEmpty(model.Password))
        {
            admin.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
        }
        
        if (ProfileImageUrl != null)
        {
            if (!string.IsNullOrEmpty(admin.ProfileImageFile))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(),
                    "wwwroot/Admin",
                    admin.ProfileImageFile);

                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            string? fileName = await FileUploadHelper.UploadImage(ProfileImageUrl, "Admin");

            admin.ProfileImageFile = fileName;
        }

        _applicationDbContext.Admins.Update(admin);

        await _applicationDbContext.SaveChangesAsync();

        TempData["Success"] = "Account Updated Successfully";

        return RedirectToAction("ListAccount", "Account");
    }
}