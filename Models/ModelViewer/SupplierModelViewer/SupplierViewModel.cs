using System.ComponentModel.DataAnnotations;
using Sandoohouse.Models.Enum;

namespace Sandoohouse.Models.ModelViewer.SupplierModelViewer
{
    public class SupplierViewModel
    {
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Company name is required")]
        [MaxLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        [Display(Name = "Company Profile")]
        public string? CompanyProfile { get; set; }

        [Required(ErrorMessage = "Contact person is required")]
        [MaxLength(50, ErrorMessage = "Contact person cannot exceed 50 characters")]
        [Display(Name = "Contact Person")]
        public string? ContactPerson { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }

        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }

        [Display(Name = "Status")]
        public SupplierStatus Status { get; set; } = SupplierStatus.Active;

        public string? Notes { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}