using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sandoohouse.Models;

public class OrderItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int PosOrderId { get; set; }
    public int MenuItemId { get; set; }
    public int BrandId { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Subtotal { get; set; }

    // Navigation Properties
    [ForeignKey("PosOrderId")]
    public virtual Order? Order { get; set; }

    [ForeignKey("MenuItemId")]
    public virtual Menu? Menu { get; set; }

    [ForeignKey("BrandId")]
    public virtual Brand? Brand { get; set; }
}