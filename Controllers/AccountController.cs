using System.Security.Claims;
using System.Security.Cryptography;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MimeKit.Text;
using Sandoohouse.ApplicationProgram;
using Sandoohouse.Helpers;
using Sandoohouse.Models;
using Sandoohouse.Models.Enum;
using Sandoohouse.Models.ModelViewer.AdminModelViewer;
using Sandoohouse.Service;

namespace Sandoohouse.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly IConfiguration _configuration;
    private readonly CloudinaryService _cloudinaryService;

    public AccountController(ApplicationDbContext applicationDbContext, IConfiguration configuration, CloudinaryService cloudinaryService)
    {
        _applicationDbContext = applicationDbContext;
        _configuration = configuration;
        _cloudinaryService = cloudinaryService;
    }

    // GET Profile image
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
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
    public async Task<IActionResult> ForgotPassword(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            TempData["Error"] = "Please enter a valid email";
            return View();
        }

        var admin = await _applicationDbContext.Admins
            .FirstOrDefaultAsync(x => x.Email == email);
        if (admin == null)
            return RedirectToAction("ForgotPassword", "Account");

        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        admin.ResetToken = Convert.ToBase64String(tokenBytes);
        admin.ResetTokenExpires = DateTime.UtcNow.AddMinutes(30);
        await _applicationDbContext.SaveChangesAsync();

        var resetLink = Url.Action(
            "ResetPassword",
            "Account",
            new { token = admin.ResetToken, email = admin.Email },
            Request.Scheme
        );
        await SendResetEmail(admin.Email, resetLink);
        TempData["Message"] = "Password reset link sent to your successfully!";
        return RedirectToAction("ForgotPassword", "Account");
    }

    private async Task SendResetEmail(string toEmail, string resetLink)
{
    if (string.IsNullOrWhiteSpace(toEmail))
        throw new ArgumentException("Recipient email cannot be empty.", nameof(toEmail));
    
    var fromEmail = Environment.GetEnvironmentVariable("EMAIL_FROM")
                     ?? _configuration["EmailSettings:From"];

    var smtpHost = Environment.GetEnvironmentVariable("EMAIL_HOST")
                     ?? _configuration["EmailSettings:SmtpHost"];

    var smtpPortString = Environment.GetEnvironmentVariable("EMAIL_PORT")
                         ?? _configuration["EmailSettings:SmtpPort"];

    var username = Environment.GetEnvironmentVariable("EMAIL_USER")
                   ?? _configuration["EmailSettings:Username"];

    var password = Environment.GetEnvironmentVariable("EMAIL_PASS")
                   ?? _configuration["EmailSettings:Password"];
    
    if (string.IsNullOrWhiteSpace(fromEmail) ||
        string.IsNullOrWhiteSpace(smtpHost) ||
        string.IsNullOrWhiteSpace(smtpPortString) ||
        string.IsNullOrWhiteSpace(username) ||
        string.IsNullOrWhiteSpace(password))
    {
        throw new InvalidOperationException("Email settings are not properly configured.");
    }

    if (!int.TryParse(smtpPortString, out var smtpPort))
        throw new InvalidOperationException("SMTP port is not valid.");

    var email = new MimeMessage();

    email.From.Add(MailboxAddress.Parse(fromEmail));
    email.To.Add(MailboxAddress.Parse(toEmail));
    email.Subject = "Reset Your Password";

    email.Body = new TextPart(TextFormat.Html)
    {
        Text = $@"
<!DOCTYPE html>
<html>
<body style='font-family:Arial,sans-serif; background-color:#f4f6f8; padding:20px;'>

  <table align='center' width='100%' cellpadding='0' cellspacing='0' style='max-width:600px;'>
    <tr>
      <td align='center' style='padding:20px 0;'>
        <h2 style='color:#333;'>Sandoo Kitchen</h2>
      </td>
    </tr>
    <tr>
      <td style='background:#fff; border-radius:8px; padding:30px;'>
        <p>Hello,</p>
        <p>You requested a password reset. Click the button below to create a new password.</p>

        <p style='text-align:center; margin:30px 0;'>
          <a href='{resetLink}'
             style='background:#0d6efd; color:#fff; text-decoration:none; padding:12px 24px; border-radius:6px; display:inline-block;'>
            Reset Password
          </a>
        </p>

        <p style='color:#555;'>This link will expire in 30 minutes. If you didn't request it, ignore this email.</p>
        <p>Regards,<br/>Sandoo Kitchen Team</p>
      </td>
    </tr>
    <tr>
      <td align='center' style='padding-top:20px; font-size:12px; color:#999;'>
        © {DateTime.Now.Year} Sandoo Kitchen. All rights reserved.
      </td>
    </tr>
  </table>

</body>
</html>"
    };

    // ============================
    // SEND EMAIL
    // ============================
    try
    {
        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(username, password);

        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
    catch (AuthenticationException)
    {
        throw new InvalidOperationException(
            "SMTP authentication failed. Use a valid Gmail App Password.");
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
    }
}

    [HttpGet]
    public IActionResult ResetPassword(string token, string email)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            return RedirectToAction("Login", "Home");

        var model = new ResetPasswordViewModel { Token = token, Email = email };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var admin = await _applicationDbContext.Admins.FirstOrDefaultAsync(a =>
            a.Email == model.Email &&
            a.ResetToken == model.Token &&
            a.ResetTokenExpires > DateTime.UtcNow);

        if (admin == null)
        {
            ModelState.AddModelError("", "Invalid or expired token.");
            return View(model);
        }

        // Hash the password (recommended)
        admin.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

        admin.ResetToken = null;
        admin.ResetTokenExpires = null;
        admin.UpdatedAt = DateTime.UtcNow;

        await _applicationDbContext.SaveChangesAsync();

        TempData["Message"] = "Password reset successfully!";
        return RedirectToAction("Login", "Home");
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner")]
    public IActionResult CreateAccount()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Owner")]
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

        // Upload profile image to Cloudinary
        string? imageUrl = null;
        if (model.ProfileImageUrl != null)
        {
            imageUrl = await _cloudinaryService.UploadImageAsync(
                model.ProfileImageUrl,
                folder: "Admin"
            );
        }

        var admin = new Admin
        {
            FirstName        = model.FirstName,
            LastName         = model.LastName,
            Email            = model.Email!,
            PhoneNumber      = model.PhoneNumber,
            Password         = BCrypt.Net.BCrypt.HashPassword(model.Password),
            ProfileImageFile = imageUrl,
            Status           = Status.Active,
            CreatedAt        = DateTime.UtcNow
        };

        _applicationDbContext.Admins.Add(admin);
        await _applicationDbContext.SaveChangesAsync();

        TempData["Success"] = "Register Success";
        return RedirectToAction("ListAccount", "Account");
    }

    // GET List of Admin in DB
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> ListAccount()
    {
        var totalAdminsCount = _applicationDbContext.Admins.Count();
        ViewBag.TotalAdminsCount = totalAdminsCount;

        var totalIncomeReal = _applicationDbContext.Orders.Sum(o => o.TotalAmount) * 4100;
        var totalIncomeDollar = _applicationDbContext.Orders.Sum(o => o.TotalAmount);
        ViewBag.TotalIncome = totalIncomeReal;
        ViewBag.TotalIncomeDollar = totalIncomeDollar;
        var admins = await _applicationDbContext.Admins
            .OrderBy(x => x.FirstName)
            .ToListAsync();
        return View(admins);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var admin = await _applicationDbContext.Admins.FindAsync(id);

        if (admin == null)
            return NotFound();

        // Delete profile image from Cloudinary
        await _cloudinaryService.DeleteImageAsync(admin.ProfileImageFile);

        _applicationDbContext.Admins.Remove(admin);
        await _applicationDbContext.SaveChangesAsync();

        TempData["Message"] = "Account deleted successfully";
        return RedirectToAction("ListAccount", "Account");
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> EditAccount(int id)
    {
        var admin = await _applicationDbContext.Admins
            .FindAsync(id);
        if (admin == null)
            return NotFound();
        return View(admin);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> EditAccount(int id, IFormFile? ProfileImageUrl, Admin model)
    {
        var admin = await _applicationDbContext.Admins.FindAsync(id);

        if (admin == null)
            return NotFound();

        admin.FirstName   = model.FirstName;
        admin.LastName    = model.LastName;
        admin.Email       = model.Email;
        admin.Role        = model.Role;
        admin.Status      = model.Status;
        admin.PhoneNumber = model.PhoneNumber;

        if (!string.IsNullOrEmpty(model.Password))
            admin.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

        // If a new profile image was uploaded, delete the old one and upload the new one
        if (ProfileImageUrl != null)
        {
            // Delete old image from Cloudinary
            await _cloudinaryService.DeleteImageAsync(admin.ProfileImageFile);

            // Upload new image
            admin.ProfileImageFile = await _cloudinaryService.UploadImageAsync(
                ProfileImageUrl,
                folder: "Admin"
            );
        }

        _applicationDbContext.Admins.Update(admin);
        await _applicationDbContext.SaveChangesAsync();

        TempData["Success"] = "Account Updated Successfully";
        return RedirectToAction("ListAccount", "Account");
    }
}