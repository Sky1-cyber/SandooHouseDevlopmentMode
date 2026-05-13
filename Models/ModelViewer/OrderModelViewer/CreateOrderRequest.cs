namespace Sandoohouse.Models.ModelViewer.OrderModelViewer;

public class CreateOrderRequest
{
    public List<OrderItemViewerModel> Items { get; set; }
    public decimal CashReceived { get; set; }
    public decimal DiscountAmount { get; set; }
}