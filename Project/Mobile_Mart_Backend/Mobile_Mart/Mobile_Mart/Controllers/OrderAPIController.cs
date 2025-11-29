using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mobile_Mart.Models;

namespace Mobile_Mart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderAPIController : ControllerBase
    {
        private readonly MobileMartsContext _context;
        private readonly IValidator<Order> _validator;
        public OrderAPIController(MobileMartsContext context, IValidator<Order> validator)
        {
            _context = context;
            _validator = validator;
        }

        #region Get All Orders
        [HttpGet("All")]
        public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
        {
            var orders = await _context.Orders
            .Include(o => o.User)
            .Select(o => new
            {
                o.OrderId,
                o.UserId,
                FullName = o.User != null ? o.User.FullName : "N/A",

                o.OrderDate,
                o.Modified,
                o.TotalAmount,
                o.Status
            })
            .ToListAsync();

            return Ok(orders);
        }
        #endregion

        #region Get Order by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrderById(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            return order == null ? NotFound() : Ok(order);
        }
        #endregion

        #region Create Order
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] Order order)
        {
            var validationResult = await _validator.ValidateAsync(order);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new
                {
                    Property = e.PropertyName,
                    Error = e.ErrorMessage
                }));
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, order);
        }
        #endregion

        #region Update Order
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, Order order)
        {
            if (id != order.OrderId)
                return BadRequest("Order ID mismatch.");

            order.Modified = DateTime.Now;
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        #region Delete Order
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null)
                    return NotFound("Order not found.");

                // Delete related OrderItems first
                if (order.OrderItems.Any())
                {
                    _context.OrderItems.RemoveRange(order.OrderItems);
                }

                // Now delete the order
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting order: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        #endregion

        #region Clean Up Empty Orders
        [HttpDelete("Cleanup/Empty")]
        public async Task<IActionResult> CleanupEmptyOrders()
        {
            try
            {
                // Find orders that have no order items
                var emptyOrders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o => !o.OrderItems.Any())
                    .ToListAsync();

                if (emptyOrders.Any())
                {
                    _context.Orders.RemoveRange(emptyOrders);
                    await _context.SaveChangesAsync();
                    return Ok($"Cleaned up {emptyOrders.Count} empty orders.");
                }

                return Ok("No empty orders found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up empty orders: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        #endregion

        #region Get Orders by UserId
        [HttpGet("User/{userId}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByUserId(int userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ToListAsync();

            return Ok(orders);
        }
        #endregion

        #region Search by Status
        [HttpGet("Search/{status}")]
        public async Task<ActionResult<IEnumerable<Order>>> SearchByStatus(string status)
        {
            var orders = await _context.Orders
                .Where(o => o.Status.Contains(status))
                .ToListAsync();

            return orders.Count == 0 ? NotFound() : Ok(orders);
        }
        #endregion

        #region user dropdown
        [HttpGet("User")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.UserId,
                    FullName = u.FullName
                })
                .ToListAsync();

            return Ok(users);
        }
        #endregion
    }
}
