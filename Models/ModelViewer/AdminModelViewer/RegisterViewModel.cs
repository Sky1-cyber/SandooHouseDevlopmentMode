using System.ComponentModel.DataAnnotations;

namespace Sandoohouse.Models.ModelViewer.AdminModelViewer;

public class RegisterViewModel
{
    [Required(ErrorMessage = "First name is required")]
    [MaxLength(50)]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(50)]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Confirm password is required")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string? ConfirmPassword { get; set; }

    public IFormFile? ProfileImageUrl { get; set; }
}