using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Sandoohouse.Models.Enum;

namespace Sandoohouse.Models;

[Index(nameof(OrderNumber), IsUnique = true)]
public class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public required string OrderNumber { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal DiscountAmount { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal CashReceived { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal ChangeAmount { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.Now;

    [Required]
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Paid;

    public int? ShiftId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    // Navigation: One order has many order items
    public virtual Shift? Shift { get; set; }
    public virtual ICollection<OrderItem>? OrderItems { get; set; }
}