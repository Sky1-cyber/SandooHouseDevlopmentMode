using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sandoohouse.Models;

public class Shift
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    public DateTime? EndTime { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal OpeningCash { get; set; } = 0;

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalSales { get; set; } = 0;

    public int TotalOrders { get; set; } = 0;

    public bool IsClosed { get; set; } = false;

    [MaxLength(100)]
    public string? ClosedBy { get; set; }

    [MaxLength(100)]
    public string? CashierName { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}