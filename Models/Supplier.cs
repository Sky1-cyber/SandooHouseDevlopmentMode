using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Sandoohouse.Models.Enum;

namespace Sandoohouse.Models;

[Index(nameof(CompanyName), IsUnique = true)]
[Index(nameof(ContactPerson), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class Supplier
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SupplierId { get; set; }
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public required string CompanyName { get; set; }
    public string? CompanyProfile { get; set; }
    
    [Required, MaxLength(50)]
    public required string ContactPerson { get; set; }

    [Required, Phone]
    public required string Phone { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }

    public SupplierStatus Status { get; set; } = SupplierStatus.Active;
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}