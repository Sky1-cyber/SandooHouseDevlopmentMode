using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Sandoohouse.Models;

[Index(nameof(BrandName), IsUnique = true)]
public class Brand
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public required string BrandName { get; set; }

    [Column(TypeName = "text")]
    public string? Description { get; set; }

    public string? LogoBrandUrl { get; set; }

    public bool Status { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; }

    // Navigation property: One Brand has many Categories
    public virtual ICollection<Category>? Categories { get; set; }
}