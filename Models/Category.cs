using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Sandoohouse.Models;

[Index(nameof(CategoryImageUrl), IsUnique = true)]
public class Category
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(250)]
    [Display(Name = "Category Name")]
    public string CategoryName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(500)]
    [Display(Name = "Category Image URL")]
    public string? CategoryImageUrl { get; set; }

    [Required]
    public bool Status { get; set; } = true;

    [ForeignKey("Admin")]
    [Display(Name = "Created By")]
    public int CreatedById { get; set; }
    
    [ForeignKey("Brand")]
    [Display(Name = "Brand")]
    public int? BrandId { get; set; }
    
    [Required]
    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Updated At")]
    public DateTime? UpdatedAt { get; set; } // nullable in case it hasn't been updated

    // Optional navigation property to Admin (user) table
    public virtual Admin? Admin { get; set; }
    public virtual Brand? Brand { get; set; }
    public virtual ICollection<Menu>? Menus { get; set; }
}