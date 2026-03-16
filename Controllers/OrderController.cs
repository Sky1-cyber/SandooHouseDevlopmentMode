using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sandoohouse.ApplicationProgram;
using Sandoohouse.Models;
using Sandoohouse.Models.Enum;
using Sandoohouse.Models.ModelViewer.OrderModelViewer;

namespace Sandoohouse.Controllers;

[Authorize]
public class OrderController : Controller
{
    private readonly ApplicationDbContext _applicationDbContext;

    public OrderController(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }
    
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner,Manager,Cashier")]
    public async Task<IActionResult> Index()
    {
        var orders = await _applicationDbContext.Orders
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return View(orders);
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner,Manager,Cashier")]
    public async Task<IActionResult> CreateOrder()
    {
        ViewBag.Brands = await _applicationDbContext.Brands
            .ToListAsync();

        ViewBag.Categories = await _applicationDbContext.Categories
            .ToListAsync();

        var menus = await _applicationDbContext.Menus
            .Include(m => m.Category)
            .ThenInclude(c => c.Brand) // Include brand through category
            .Where(m => m.Status == true)
            .ToListAsync();

        ViewBag.Menus = menus;

        return View();
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    [Authorize(Roles = "SuperAdmin,Owner,Manager,Cashier")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var items = request.Items;
            var cashReceived = request.CashReceived;
            var discountAmount = request.DiscountAmount;

            if (items == null || !items.Any())
            {
                Console.WriteLine("No items in order");
                return BadRequest(new { error = "Order has no items." });
            }

            Console.WriteLine($"Items count: {items.Count}");
            Console.WriteLine($"Cash received: {cashReceived}");
            Console.WriteLine($"Discount: {discountAmount}");

            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in items)
            {
                item.Subtotal = item.Price * item.Quantity;
                totalAmount += item.Subtotal;

                var orderItem = new OrderItem
                {
                    MenuItemId = item.MenuItemId,
                    BrandId = item.BrandId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Subtotal = item.Subtotal
                };
                orderItems.Add(orderItem);

                Console.WriteLine(
                    $"Item: {item.MenuItemId}, Price: {item.Price}, Qty: {item.Quantity}, Subtotal: {item.Subtotal}");
            }

            totalAmount -= discountAmount;
            Console.WriteLine($"Total amount: {totalAmount}");

            var order = new Order
            {
                OrderNumber = "ORD-" + DateTime.Now.Ticks,
                TotalAmount = totalAmount,
                DiscountAmount = discountAmount,
                CashReceived = cashReceived,
                ChangeAmount = cashReceived - totalAmount,
                OrderStatus = OrderStatus.Paid,
                OrderDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                OrderItems = orderItems // Add items directly to the order
            };

            _applicationDbContext.Orders.Add(order);

            // Single save operation - Entity Framework will save both order and items
            var saveResult = await _applicationDbContext.SaveChangesAsync();
            Console.WriteLine($"Order and items saved: {saveResult} rows affected, Order ID: {order.Id}");

            return Ok(new { success = true, orderId = order.Id, orderNumber = order.OrderNumber });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null) Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    [Authorize(Roles = "SuperAdmin,Owner")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        try
        {
            var order = await _applicationDbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound(new { error = "Order not found" });
            }

            // Remove order items first (optional if cascade delete enabled)
            if (order.OrderItems != null && order.OrderItems.Any())
            {
                _applicationDbContext.OrderItems.RemoveRange(order.OrderItems);
            }

            // Remove order
            _applicationDbContext.Orders.Remove(order);

            await _applicationDbContext.SaveChangesAsync();

            return RedirectToAction("Index",  "Home");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}