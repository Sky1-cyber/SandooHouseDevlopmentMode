using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sandoohouse.Models.Enum;

namespace Sandoohouse.Models.ModelViewer.SupplierModelViewer;

public class SupplierViewModel
{
    public int SupplierId { get; set; }

    [Required]
    public string? CompanyName { get; set; }

    public string? CompanyProfile { get; set; }

    [NotMapped]
    public IFormFile? CompanyProfileFile { get; set; }

    [Required]
    public string? ContactPerson { get; set; }

    [Required]
    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }

    public SupplierStatus Status { get; set; } = SupplierStatus.Active;

    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}