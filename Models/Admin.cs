using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Sandoohouse.Models.Enum;

namespace Sandoohouse.Models;

[Index(nameof(Email), IsUnique = true)]
public class Admin
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(50)]
    public string? FirstName { get; set; }

    [MaxLength(50)]
    public string? LastName { get; set; }

    [Column(TypeName = "text")]
    public string? ProfileImageFile { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Email { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Password { get; set; }

    public Role Role { get; set; } = Role.Manager;
    
    public Status Status { get; set; } = Status.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }
    
    public virtual ICollection<Category>? Categories { get; set; }
    public virtual ICollection<Menu>? Menus { get; set; }
}