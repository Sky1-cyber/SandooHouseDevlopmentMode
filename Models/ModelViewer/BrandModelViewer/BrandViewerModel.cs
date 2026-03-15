using System.ComponentModel.DataAnnotations;

namespace Sandoohouse.Models.ModelViewer.BrandModelViewer;

public class BrandViewerModel
{
    public int Id { get; set; }

    [Display(Name = "Brand Name")]
    [Required(ErrorMessage = "Brand name is required")]
    [StringLength(50)]
    public string BrandName { get; set; } = string.Empty;

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Brand Logo")]
    public string? LogoBrandUrl { get; set; }

    // For uploading logo
    public IFormFile? LogoFile { get; set; }

    public bool Status { get; set; } = true;

    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Updated At")]
    public DateTime? UpdatedAt { get; set; }
}