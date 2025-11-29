using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mobile_Mart.Models;

namespace Mobile_Mart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        #region Constructor & Context
        private readonly MobileMartsContext _context;
        private readonly IValidator<Cart> _validator;
        public CartAPIController(MobileMartsContext context, IValidator<Cart> validator)
        {
            _context = context;
            _validator = validator;
        }
        #endregion

        #region Get All Carts
        [HttpGet("All")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllCarts()
        {
            var carts = await _context.Carts
                .Include(c => c.Product)
                .Include(c => c.User)
                .Select(c => new
                {
                    c.CartId,

                    c.UserId,
                    FullName = c.User != null ? c.User.FullName : "N/A",

                    c.ProductId,
                    ProductName = c.Product != null ? c.Product.ProductName : "N/A",

                    c.Quantity,

                    c.Created,
                    c.Modified
                })
                .ToListAsync();

            return Ok(carts);
        }
        #endregion

        #region Get Cart by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetCartById(int id)
        {
            var item = await _context.Carts
                .Where(c => c.CartId == id)
                .Select(c => new
                {
                    CartId = c.CartId,

                    ProductId = c.Product != null ? c.Product.ProductId : 0,
                    ProductName = c.Product != null ? c.Product.ProductName : "N/A",

                    Quantity = c.Quantity,

                    UserId = c.User != null ? c.User.UserId : 0,
                    FullName = c.User != null ? c.User.FullName : "N/A",

                    Created = c.Created,
                    Modified = c.Modified
                })
                .FirstOrDefaultAsync();

            return item == null ? NotFound() : Ok(item);
        }
        #endregion

        #region Get Carts by User ID
        [HttpGet("User/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetCartsByUserId(int userId)
        {
            var carts = await _context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                    .ThenInclude(p => p.ProductImages) // 👈 include images
                .Select(c => new
                {
                    c.CartId,
                    c.UserId,
                    FullName = c.User != null ? c.User.FullName : "N/A",

                    c.ProductId,
                    ProductName = c.Product != null ? c.Product.ProductName : "N/A",
                    Price = c.Product != null ? c.Product.Price ?? 0 : 0,

                    ImageUrl = c.Product != null && c.Product.ProductImages.Any()
                        ? c.Product.ProductImages.FirstOrDefault().ImageUrl
                        : "/images/no-image.png",

                    c.Quantity,
                    c.Created,
                    c.Modified
                })
                .ToListAsync();

            return Ok(carts);
        }
        #endregion

        #region Add Item to Cart
        [HttpPost]
        public async Task<ActionResult<Cart>> AddToCart([FromBody] Cart cart)
        {
            var validationResult = await _validator.ValidateAsync(cart);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new
                {
                    Property = e.PropertyName,
                    Error = e.ErrorMessage
                }));
            }

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCartById), new { id = cart.CartId }, cart);
        }
        #endregion

        #region Update Cart
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCart(int id, Cart cart)
        {
            if (id != cart.CartId)
                return BadRequest("Cart ID mismatch.");

            cart.Modified = DateTime.Now;
            _context.Entry(cart).State = EntityState.Modified;
             await _context.SaveChangesAsync();
           
            return NoContent();
        }
        #endregion

        #region Delete Cart
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCart(int id)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
                return NotFound();

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        #region Product dropdown
        [HttpGet("Product")]
        public async Task<ActionResult<IEnumerable<object>>> GetProducts()
        {
            var product = await _context.Products
                .Select(u => new
                {
                    u.ProductId,
                    ProductName = u.ProductName
                })
                .ToListAsync();

            return Ok(product);
        }
        #endregion

    }
}
