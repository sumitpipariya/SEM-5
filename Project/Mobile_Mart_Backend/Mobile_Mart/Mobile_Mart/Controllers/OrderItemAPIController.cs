using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mobile_Mart.Models;

namespace Mobile_Mart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemAPIController : ControllerBase
    {

        private readonly MobileMartsContext _context;
        private readonly IValidator<OrderItem> _validator;
        public OrderItemAPIController(MobileMartsContext context, IValidator<OrderItem> validator)
        {
            _context = context;
            _validator = validator;
        }

        #region Get All OrderItems
        [HttpGet("All")]
    public async Task<ActionResult<IEnumerable<object>>> GetAllOrderItems()
    {
        var orderItems = await _context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .Include(oi => oi.User)
            .Select(oi => new
            {
                oi.OrderItemId,
                oi.OrderId,
                OrderNumber = oi.Order != null ? $"ORD-{oi.Order.OrderId}" : "N/A",
            
                oi.ProductId,
                ProductName = oi.Product != null ? oi.Product.ProductName : "N/A",

                oi.Quantity,
                oi.Price,
                Total = (oi.Price ?? 0) * (oi.Quantity ?? 0),

                oi.UserId,
                FullName = oi.User != null ? oi.User.FullName : "N/A",

                oi.Created,
                oi.Modified
            })
            .OrderByDescending(oi => oi.Created)
            .ToListAsync();

        return Ok(orderItems);
    }
        #endregion

        #region Get OrderItem by ID (with ID, Name, Price, Quantity, Dates)
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetOrderItemById(int id)
        {
            var item = await _context.OrderItems
                .Where(o => o.OrderItemId == id)
                .Select(o => new
                {
                    OrderItemId = o.OrderItemId,

                    ProductId = o.Product != null ? o.Product.ProductId : 0,
                    ProductName = o.Product != null ? o.Product.ProductName : "N/A",
                    Price = o.Product != null ? o.Product.Price : 0,

                    Quantity = o.Quantity,

                    UserId = o.User != null ? o.User.UserId : 0,
                    FullName = o.User != null ? o.User.FullName : "N/A",

                    OrderId = o.Order != null ? o.Order.OrderId : 0,

                    Created = o.Created,
                    Modified = o.Modified
                })
                .FirstOrDefaultAsync();

            return item == null ? NotFound() : Ok(item);
        }
        #endregion

        #region Insert Order items
        [HttpPost]
        public async Task<IActionResult> CreateOrderItem([FromBody] OrderItem item)
        {
            var validationResult = await _validator.ValidateAsync(item);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new
                {
                    Property = e.PropertyName,
                    Error = e.ErrorMessage
                }));
            }

            _context.OrderItems.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrderItemById), new { id = item.OrderItemId }, item);
        }
        #endregion

        #region Update OrderItem (OrderId, ProductId, Quantity, Price)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrderItem(int id, OrderItem updatedItem)
        {
            if (id != updatedItem.OrderItemId)
                return BadRequest("OrderItem ID mismatch.");

            var existingItem = await _context.OrderItems.FindAsync(id);
            if (existingItem == null)
                return NotFound("OrderItem not found.");

            // Update fields
            existingItem.OrderId = updatedItem.OrderId;
            existingItem.ProductId = updatedItem.ProductId;
            existingItem.Quantity = updatedItem.Quantity;

            // Option A: Use posted Price
            existingItem.Price = updatedItem.Price;

            // Option B: (if you want to fetch price from Product table)
            // var product = await _context.Products.FindAsync(updatedItem.ProductId);
            // existingItem.Price = product?.Price ?? 0;

            existingItem.Modified = DateTime.Now;

            _context.Entry(existingItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        #region Delete OrderItem
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderItem(int id)
        {
            var item = await _context.OrderItems.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.OrderItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        #region Get Products (Id + Name only)
        [HttpGet("Product")]
        public async Task<ActionResult<IEnumerable<object>>> GetProductDropdown()
        {
            var products = await _context.Products
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductName
                })
                .ToListAsync();

            return Ok(products);
        }
        #endregion

        #region Get Orders (Id + Display Name for dropdown)
        [HttpGet("Order")]
        public async Task<ActionResult<IEnumerable<object>>> GetOrderDropdown()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Select(o => new
                {
                    o.OrderId,
                    OrderName = "Order #" + o.OrderId + " - " + (o.User != null ? o.User.FullName : "Unknown")
                })
                .ToListAsync();

            return Ok(orders);
        }
        #endregion

        #region Get OrderItems by OrderId (with product + user details)
        [HttpGet("Order/{orderId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetOrderItemsByOrderId(int orderId)
        {
            var items = await _context.OrderItems
                .Include(oi => oi.Product)
                .Include(oi => oi.User)
                .Where(oi => oi.OrderId == orderId)
                .Select(oi => new
                {
                    oi.OrderItemId,
                    oi.OrderId,
                    ProductId = oi.Product != null ? oi.Product.ProductId : 0,
                    ProductName = oi.Product != null ? oi.Product.ProductName : "N/A",
                    Price = oi.Product != null ? oi.Product.Price : 0,
                    oi.Quantity,
                    Total = (oi.Price ?? 0) * (oi.Quantity ?? 0),
                    UserId = oi.User != null ? oi.User.UserId : 0,
                    FullName = oi.User != null ? oi.User.FullName : "N/A",
                    oi.Created,
                    oi.Modified,
                    Status = oi.Order != null ? oi.Order.Status : "Pending"
                })
                .ToListAsync();

            if (items == null || !items.Any())
            {
                return NotFound("No items found for this order.");
            }

            return Ok(items);
        }
        #endregion

        #region Buy Now (Create Order + Item)
        [HttpPost("BuyNow")]
        public async Task<IActionResult> BuyNow([FromBody] BuyNowRequest request)
        {
            if (request == null || request.UserId <= 0 || request.ProductId <= 0)
                return BadRequest("Invalid request data.");

            // ✅ Check user
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return NotFound("User not found.");

            // ✅ Check product (with image)
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == request.ProductId);

            if (product == null) return NotFound("Product not found.");

            // ✅ Create new order
            var order = new Order
            {
                UserId = request.UserId,
                Status = "Pending",
                Modified = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // ✅ Add order item (price comes from Product table, not request)
            var orderItem = new OrderItem
            {
                OrderId = order.OrderId,
                ProductId = product.ProductId,
                Quantity = request.Quantity > 0 ? request.Quantity : 1,
                Price = product.Price ?? 0,  // ✅ real price from Product
                UserId = request.UserId,
                Created = DateTime.Now,
                Modified = DateTime.Now
            };

            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();

            // ✅ Return created order with item + product info + image
            var result = new
            {
                order.OrderId,
                order.Status,
                order.UserId,
                UserName = user.FullName,
                Items = new[]
                {
            new {
                orderItem.OrderItemId,
                orderItem.ProductId,
                ProductName = product.ProductName,
                orderItem.Quantity,
                Price = product.Price,   // ✅ real price
                Total = (product.Price ?? 0) * orderItem.Quantity,

                // ✅ Send first product image if available
                ImageUrl = product.ProductImages.Any()
                    ? $"{Request.Scheme}://{Request.Host}/{product.ProductImages.First().ImageUrl.TrimStart('/')}"
                    : null
            }
        }
            };

            return Ok(result);
        }
        #endregion

        #region Get My Orders
        [HttpGet("MyOrders/{userId}")]
        public async Task<IActionResult> GetMyOrders(int userId)
        {
            // Check user exists
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var baseUrl = $"{Request.Scheme}://{Request.Host}/";

            // Fetch orders with items + product details + images
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductImages)
                
                .Select(o => new
                {
                    o.OrderId,
                    o.Status,
                    o.UserId,
                    UserName = user.FullName,
                    o.Modified,
                    Items = o.OrderItems.Select(oi => new
                    {
                        oi.OrderItemId,
                        oi.ProductId,
                        ProductName = oi.Product != null ? oi.Product.ProductName : "N/A",
                        Price = oi.Product != null ? oi.Product.Price : 0, // ✅ Real product price
                        oi.Quantity,
                        Total = (oi.Product != null ? oi.Product.Price ?? 0 : 0) * (oi.Quantity ?? 0),

                      
                    })
                })
                .ToListAsync();

            if (orders == null || !orders.Any())
                return NotFound("No orders found for this user.");

            return Ok(orders);
        }
        #endregion

        #region Delete Order (Soft Delete - Recommended)
        [HttpDelete("DeleteOrder/{orderId}")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            // ✅ Find order with user info
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return NotFound($"Order with ID {orderId} not found.");

            // ✅ Optional: Check if user is authorized (if you have auth context)
            // Example: if (order.UserId != currentUserId) return Forbid();

            // ✅ Soft delete: Mark as Cancelled instead of deleting
            order.Status = "Cancelled";
            order.Modified = DateTime.UtcNow;

            // ✅ Optionally mark order items as cancelled too
            foreach (var item in order.OrderItems)
            {
                item.Modified = DateTime.UtcNow;
                // You can add item.Status = "Cancelled"; if your OrderItem has Status
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Order successfully cancelled.",
                OrderId = order.OrderId,
                Status = order.Status,
                CancelledAt = order.Modified
            });
        }
        #endregion

    }

    // DTO for BuyNow request
    public class BuyNowRequest
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }


}
