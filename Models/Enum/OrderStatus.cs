using System.ComponentModel.DataAnnotations;

namespace Sandoohouse.Models.Enum;

public enum OrderStatus
{
    [Display(Name = "PENDING")]
    Pending = 0,
    [Display(Name = "PAID")]
    Paid = 1,
    [Display(Name = "CANCELLED")]
    Canceled = 2
}