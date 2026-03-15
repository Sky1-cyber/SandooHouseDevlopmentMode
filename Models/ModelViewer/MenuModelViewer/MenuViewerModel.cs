using System.ComponentModel.DataAnnotations;

namespace Sandoohouse.Models.ModelViewer.MenuModelViewer;

public class MenuViewerModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Menu name is required")]
    [MaxLength(200)]
    public string MenuName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required")]
    [Range(0, 999999)]
    public decimal Price { get; set; }

    [Range(0, 999999)]
    public decimal? DiscountPrice { get; set; }

    public bool Status { get; set; } = true;

    [Required(ErrorMessage = "Category is required")]
    public int? CategoryId { get; set; } 

    // Display only
    public string? CategoryName { get; set; } 

    // Upload image
    public IFormFile? ImageFile { get; set; }

    // Optional for edit / display
    public string? ImageMenuUrl { get; set; }
}