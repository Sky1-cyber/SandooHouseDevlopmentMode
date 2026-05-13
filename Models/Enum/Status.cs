using System.ComponentModel.DataAnnotations;

namespace Sandoohouse.Models.Enum;

public enum Status
{
    [Display(Name = "INACTIVE")]
    Inactive = 0,
    [Display(Name = "ACTIVE")]
    Active = 1,
    [Display(Name = "SUSPENDED")]
    Suspended = 2,
}