using System.ComponentModel.DataAnnotations;

namespace Sandoohouse.Models.Enum;

public enum Role
{
    [Display(Name = "SUPERADMIN")]
    SuperAdmin = 0,
    [Display(Name = "MANAGER")]
    Manager = 1,
    [Display(Name = "USER")]
    User = 2
}