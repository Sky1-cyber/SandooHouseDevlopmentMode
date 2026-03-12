using System.ComponentModel.DataAnnotations;

namespace Sandoohouse.Models.Enum;

public enum SupplierStatus
{
    [Display(Name = "ACTIVE")]
    Active = 0,
    [Display(Name = "INACTIVE")]
    Inactive = 1,
    [Display(Name = "ONHOLD")]
    OnHold = 2,
    [Display(Name = "BANNED")]
    Banned = 3
}