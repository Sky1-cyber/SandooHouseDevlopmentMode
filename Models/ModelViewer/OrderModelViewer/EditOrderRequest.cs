using Sandoohouse.Models.Enum;

namespace Sandoohouse.Models.ModelViewer.OrderModelViewer;

public class EditOrderViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal CashReceived { get; set; }
    public decimal ChangeAmount { get; set; }
    public List<EditOrderItemViewModel> Items { get; set; }
}

public class EditOrderItemViewModel
{
    public int Id { get; set; } // OrderItem Id
    public int MenuItemId { get; set; }
    public string MenuItemName { get; set; }
    public int BrandId { get; set; }
    public string BrandName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Subtotal { get; set; }
}

public class EditOrderRequest
{
    public int Id { get; set; }
    public List<EditOrderItemRequest> Items { get; set; }
    public decimal CashReceived { get; set; }
    public decimal DiscountAmount { get; set; }
    public OrderStatus OrderStatus { get; set; }
}

public class EditOrderItemRequest
{
    public int Id { get; set; } // OrderItem Id (0 for new items)
    public int MenuItemId { get; set; }
    public int BrandId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Subtotal { get; set; }
}