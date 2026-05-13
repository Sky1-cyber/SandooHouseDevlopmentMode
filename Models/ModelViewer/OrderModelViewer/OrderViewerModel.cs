using System.ComponentModel.DataAnnotations;

namespace Sandoohouse.Models.ModelViewer.OrderModelViewer
{
    public class OrderItemViewerModel
    {
        public int Id { get; set; }

        [Display(Name = "Order ID")]
        public int PosOrderId { get; set; }

        [Display(Name = "Menu Item")]
        public int MenuItemId { get; set; }

        [Display(Name = "Brand")]
        public int BrandId { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Subtotal")]
        [DataType(DataType.Currency)]
        public decimal Subtotal { get; set; }

        // Optional: include related navigation info for display
        [Display(Name = "Menu Item Name")]
        public string? MenuName { get; set; }

        [Display(Name = "Brand Name")]
        public string? BrandName { get; set; }
    }
}