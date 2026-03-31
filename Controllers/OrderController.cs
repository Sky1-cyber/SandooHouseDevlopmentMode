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

    // [HttpGet]
    // [Authorize(Roles = "SuperAdmin,Owner,Manager,Cashier")]
    // public async Task<IActionResult> Index()
    // {
    //     var orders = await _applicationDbContext.Orders
    //         .Include(o => o.OrderItems)
    //         .OrderByDescending(o => o.CreatedAt)
    //         .ToListAsync();
    //     return View(orders);
    // }

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
            .OrderByDescending(o => o.Id)
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
            var shift = await _applicationDbContext.Shifts
                .FirstOrDefaultAsync(s => !s.IsClosed);
            if (shift == null)
                return BadRequest(new { error = "Shift not found." });
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
                ShiftId = shift.Id,
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

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> GetOrderDetails(int? id)
    {
        if (id == null)
            return NotFound();

        var order = await _applicationDbContext.Orders
            .Include(o => o.OrderItems)!
            .ThenInclude(oi => oi.Menu)
            .ThenInclude(m => m!.Category)
            .ThenInclude(c => c!.Brand)
            .Include(o => o.Shift)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        return View(order);
    }
    
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> EditOrder(int? id)
    {
        if (id == null) return NotFound();

        var order = await _applicationDbContext.Orders
            .Include(o => o.OrderItems)!
            .ThenInclude(oi => oi.Menu) // Include Menu navigation property
            .Include(o => o.OrderItems)!
            .ThenInclude(oi => oi.Brand) // Include Brand navigation property
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        // Check if order can be edited (only pending orders can be edited)
        if (order.OrderStatus == OrderStatus.Canceled)
        {
            TempData["ErrorMessage"] = "Paid or cancelled orders cannot be edited.";
            return RedirectToAction("Index", "Home");
        }

        // Get data for dropdowns
        ViewBag.Brands = await _applicationDbContext.Brands
            .Where(b => b.Status == true)
            .ToListAsync();

        ViewBag.Categories = await _applicationDbContext.Categories
            .Include(c => c.Brand)
            .Where(c => c.Status == true)
            .ToListAsync();

        var menus = await _applicationDbContext.Menus
            .Include(m => m.Category)
            .ThenInclude(c => c.Brand)
            .Where(m => m.Status == true)
            .ToListAsync();

        ViewBag.Menus = menus;

        // Create view model
        var viewModel = new EditOrderViewModel
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            OrderStatus = order.OrderStatus,
            TotalAmount = order.TotalAmount,
            DiscountAmount = order.DiscountAmount,
            CashReceived = order.CashReceived,
            ChangeAmount = order.ChangeAmount,
            Items = order.OrderItems.Select(oi => new EditOrderItemViewModel
            {
                Id = oi.Id,
                MenuItemId = oi.MenuItemId,
                MenuItemName = oi.Menu?.MenuName ?? "Unknown",
                BrandId = oi.BrandId,
                BrandName = oi.Brand?.BrandName ?? "Unknown",
                Quantity = oi.Quantity,
                Price = oi.Price,
                Subtotal = oi.Subtotal
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    [Authorize(Roles = "SuperAdmin,Owner,Manager")]
    public async Task<IActionResult> EditOrder([FromBody] EditOrderRequest request)
    {
        try
        {
            if (request == null) return BadRequest(new { error = "Invalid request." });

            // Validate items
            if (request.Items == null || !request.Items.Any())
                return BadRequest(new { error = "Order must have at least one item." });

            // Find existing order with items
            var existingOrder = await _applicationDbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == request.Id);

            if (existingOrder == null) return NotFound(new { error = "Order not found." });


            // Begin transaction to ensure data consistency
            using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                // Calculate new totals
                decimal totalAmount = 0;
                var requestItemIds = new HashSet<int>();

                // Process each item from the request
                foreach (var item in request.Items)
                {
                    // Validate item data
                    if (item.MenuItemId <= 0 || item.BrandId <= 0 || item.Quantity <= 0 || item.Price <= 0)
                        return BadRequest(new { error = "Invalid item data." });

                    // Calculate subtotal
                    item.Subtotal = item.Price * item.Quantity;
                    totalAmount += item.Subtotal;

                    if (item.Id > 0) // Existing item (OrderItem Id)
                    {
                        requestItemIds.Add(item.Id);

                        var existingItem = existingOrder.OrderItems
                            .FirstOrDefault(oi => oi.Id == item.Id);

                        if (existingItem != null)
                        {
                            // Update existing item
                            existingItem.MenuItemId = item.MenuItemId;
                            existingItem.BrandId = item.BrandId;
                            existingItem.Quantity = item.Quantity;
                            existingItem.Price = item.Price;
                            existingItem.Subtotal = item.Subtotal;

                            // Mark as modified
                            _applicationDbContext.Entry(existingItem).State = EntityState.Modified;
                        }
                        else
                        {
                            // Item ID provided but not found - might be from another order
                            await transaction.RollbackAsync();
                            return BadRequest(new { error = $"Order item with ID {item.Id} not found in this order." });
                        }
                    }
                    else // New item (Id = 0)
                    {
                        var newItem = new OrderItem
                        {
                            PosOrderId = existingOrder.Id, // Set the foreign key
                            MenuItemId = item.MenuItemId,
                            BrandId = item.BrandId,
                            Quantity = item.Quantity,
                            Price = item.Price,
                            Subtotal = item.Subtotal
                        };

                        existingOrder.OrderItems.Add(newItem);
                    }
                }

                // Find and remove items that are in the database but not in the request
                var itemsToRemove = existingOrder.OrderItems
                    .Where(oi => !requestItemIds.Contains(oi.Id))
                    .ToList();

                foreach (var itemToRemove in itemsToRemove)
                {
                    existingOrder.OrderItems.Remove(itemToRemove);
                    _applicationDbContext.OrderItems.Remove(itemToRemove);
                }

                // Apply discount
                totalAmount -= request.DiscountAmount;

                // Validate total amount is not negative
                if (totalAmount < 0)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { error = "Total amount cannot be negative." });
                }

                // Validate cash received is sufficient (optional - you can make this a warning instead)
                if (request.CashReceived < totalAmount && request.CashReceived > 0)
                    // You can still proceed, but log it
                    Console.WriteLine(
                        $"Warning: Cash received (${request.CashReceived}) is less than total (${totalAmount})");

                // Update order properties
                existingOrder.TotalAmount = totalAmount;
                existingOrder.DiscountAmount = request.DiscountAmount;
                existingOrder.CashReceived = request.CashReceived;
                existingOrder.ChangeAmount =
                    request.CashReceived >= totalAmount ? request.CashReceived - totalAmount : 0;
                existingOrder.OrderStatus = request.OrderStatus;
                existingOrder.UpdatedAt = DateTime.UtcNow;

                // Mark order as modified
                _applicationDbContext.Entry(existingOrder).State = EntityState.Modified;

                // Save all changes
                var saveResult = await _applicationDbContext.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                Console.WriteLine($"Order updated: {saveResult} rows affected, Order ID: {existingOrder.Id}");

                return Ok(new
                {
                    success = true,
                    orderId = existingOrder.Id,
                    orderNumber = existingOrder.OrderNumber,
                    totalAmount = existingOrder.TotalAmount,
                    changeAmount = existingOrder.ChangeAmount,
                    message = "Order updated successfully."
                });
            }
            catch (Exception ex)
            {
                // Rollback transaction if any error occurs
                await transaction.RollbackAsync();
                Console.WriteLine($"Error in transaction: {ex.Message}");
                throw;
            }
        }
        catch (DbUpdateException dbEx)
        {
            Console.WriteLine($"Database error updating order: {dbEx.Message}");
            Console.WriteLine($"Inner Exception: {dbEx.InnerException?.Message}");
            return StatusCode(500,
                new
                {
                    error = "A database error occurred while updating the order. Please check your data and try again."
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating order: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");

            return StatusCode(500, new { error = "An error occurred while updating the order. Please try again." });
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

            if (order == null) return NotFound(new { error = "Order not found" });

            // Remove order items first (optional if cascade delete enabled)
            if (order.OrderItems != null && order.OrderItems.Any())
                _applicationDbContext.OrderItems.RemoveRange(order.OrderItems);

            // Remove order
            _applicationDbContext.Orders.Remove(order);

            await _applicationDbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}