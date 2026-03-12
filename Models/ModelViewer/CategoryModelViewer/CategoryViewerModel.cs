using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Sandoohouse.Models.ModelViewer.CategoryModelViewer;

public class CategoryViewerModel
{
    public int Id { get; set; }

    [Display(Name = "Category Name")]
    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Display(Name = "Category Image")]
    public string? CategoryImageUrl { get; set; }  // stored path

    [Display(Name = "Upload Image")]
    public IFormFile? ImageFile { get; set; }      // file uploaded from form

    public bool Status { get; set; }

    [Display(Name = "Created By")]
    public string? CreatedByName { get; set; }

    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Updated At")]
    public DateTime? UpdatedAt { get; set; }
    
    public string? ExistingImageUrl { get; set; }
    public string RemoveImage { get; set; } = "false";
}