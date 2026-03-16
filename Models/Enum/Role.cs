using System.ComponentModel.DataAnnotations;

namespace Sandoohouse.Models.Enum;

public enum Role
{
    /// <summary>
    /// Highest level role.
    /// Has full control over the entire system including
    /// users, database, settings, and all reports.
    /// Usually used by system developer or IT administrator.
    /// </summary>
    [Display(Name = "SUPER ADMIN")]
    SuperAdmin = 0,

    /// <summary>
    /// Business owner role.
    /// Can view financial reports, manage employees,
    /// products, and store settings but cannot modify
    /// core system configuration.
    /// </summary>
    [Display(Name = "OWNER")]
    Owner = 1,

    /// <summary>
    /// Store manager role.
    /// Responsible for daily store operations such as
    /// managing products, inventory, employees, and
    /// viewing sales reports.
    /// </summary>
    [Display(Name = "MANAGER")]
    Manager = 2,

    /// <summary>
    /// Cashier role used for POS operation.
    /// Can create orders, process payments,
    /// and print receipts.
    /// Limited access to reports and system settings.
    /// </summary>
    [Display(Name = "CASHIER")]
    Cashier = 3,

    /// <summary>
    /// General staff role.
    /// Can assist with store operations such as
    /// checking inventory or viewing product lists
    /// but has very limited permissions.
    /// </summary>
    [Display(Name = "STAFF")]
    Staff = 4
}