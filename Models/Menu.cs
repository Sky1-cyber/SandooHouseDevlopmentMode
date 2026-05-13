using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Sandoohouse.Models;

[Index(nameof(MenuName), IsUnique = true)]
public class Menu
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string MenuName { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? DiscountPrice { get; set; }

    [Column(TypeName = "text")]
    public string? ImageMenuUrl { get; set; }

    public bool Status { get; set; } = true;

    [ForeignKey("Admin")]
    public int? CreatedBy { get; set; }

    [ForeignKey("Category")]
    public int? CategoryId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Admin? Admin { get; set; }
    public virtual Category? Category { get; set; }
}