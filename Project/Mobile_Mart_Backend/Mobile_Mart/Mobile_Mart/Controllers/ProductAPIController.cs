using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mobile_Mart.Models;

namespace Mobile_Mart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductAPIController : ControllerBase
    {
        private readonly MobileMartsContext _context;
        private readonly IValidator<Product> _validator;
        public ProductAPIController(MobileMartsContext context, IValidator<Product> validator)
        {
            _context = context;
            _validator = validator;
        }

        #region Get All Products
        [HttpGet("All")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllProducts()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}/"; // ✅ dynamic base URL (http/https + host + port)

            var products = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.User)
                .Include(p => p.ProductImages) // ✅ include images
                .Select(x => new
                {
                    x.ProductId,
                    x.ProductName,

                    x.BrandId,
                    BrandName = x.Brand != null ? x.Brand.BrandName : "N/A",

                    x.CategoryId,
                    CategoryName = x.Category != null ? x.Category.CategoryName : "N/A",

                    x.UserId,
                    FullName = x.User != null ? x.User.FullName : "N/A",

                    x.Price,
                    x.Description,
                    x.Created,
                    x.Modified,

                    // ✅ send the first image's full URL if available, else null
                    ImageUrl = x.ProductImages.Any()
                        ? baseUrl + x.ProductImages.FirstOrDefault().ImageUrl
                        : null
                })
                .ToListAsync();

            return Ok(products);
        }
        #endregion

        #region Get Product by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .Include(p => p.User)
                    .Where(p => p.ProductId == id)
                    .Select(p => new
                    {
                        p.ProductId,
                        p.ProductName,
                        p.BrandId,
                        BrandName = p.Brand.BrandName,
                        p.CategoryId,
                        CategoryName = p.Category.CategoryName,
                        p.UserId,
                        FullName = p.User.FullName,
                        p.Price,
                        p.Description,
                        p.Created,
                        p.Modified,

                        // 👇 Fetch first image URL
                        ImageUrl = _context.ProductImages
                            .Where(i => i.ProductId == p.ProductId)
                            .Select(i => $"{Request.Scheme}://{Request.Host}/{i.ImageUrl.TrimStart('/')}")
                            .FirstOrDefault(),

                        // 👇 Fetch all images for gallery
                        Images = _context.ProductImages
                            .Where(i => i.ProductId == p.ProductId)
                            .Select(i => new
                            {
                                i.ImageId,
                                ImageUrl = $"{Request.Scheme}://{Request.Host}/{i.ImageUrl.TrimStart('/')}"
                            })
                            .ToList()
                    })
                    .FirstOrDefaultAsync();

                if (product == null)
                    return NotFound(new { message = "Product not found." });

                return Ok(product);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching product: {ex.Message}");
                return StatusCode(500, new { message = "Internal Server Error", details = ex.Message });
            }
        }
        #endregion

        #region Create Product
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
        {
            var validationResult = await _validator.ValidateAsync(product);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new
                {
                    Property = e.PropertyName,
                    Error = e.ErrorMessage
                }));
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductById), new { id = product.ProductId }, product);
        }
        #endregion

        #region Update Product
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.ProductId)
                return BadRequest("Product ID mismatch.");

            product.Modified = DateTime.Now;
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
         
            return NoContent();
        }
        #endregion

        #region Delete Product
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.OrderItems)
                    .Include(p => p.Carts)
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                    return NotFound("Product not found.");

                // Delete related OrderItems first
                if (product.OrderItems.Any())
                {
                    _context.OrderItems.RemoveRange(product.OrderItems);
                }

                // Delete related Cart items
                if (product.Carts.Any())
                {
                    _context.Carts.RemoveRange(product.Carts);
                }

                // Delete related ProductImages
                if (product.ProductImages.Any())
                {
                    _context.ProductImages.RemoveRange(product.ProductImages);
                }

                // Now delete the product
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting product: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        #endregion

        #region Search by Name
        [HttpGet("Search/{name}")]
        public async Task<ActionResult<IEnumerable<Product>>> SearchProduct(string name)
        {
            var products = await _context.Products
                .Where(p => p.ProductName.Contains(name))
                .ToListAsync();

            return products.Count == 0 ? NotFound() : Ok(products);
        }
        #endregion

        #region Dropdown Product
        [HttpGet("Dropdown")]
        public async Task<ActionResult<IEnumerable<object>>> GetProductDropdown()
        {
            var products = await _context.Products
                .Select(p => new { p.ProductId, p.ProductName })
                .ToListAsync();

            return Ok(products);
        }
        #endregion

        #region brand dropdown
        [HttpGet("Brand")]
        public async Task<ActionResult<IEnumerable<Brand>>> GetBrands()
        {
            var brands = await _context.Brands
                .Select(b => new
                {
                    b.BrandId,
                    b.BrandName
                })
                .ToListAsync();

            return Ok(brands);
        }
        #endregion

        #region category dropdown
        [HttpGet("Category")]
        public async Task<ActionResult<IEnumerable<object>>> GetCategories()
        {
            var categories = await _context.Categories
                .Select(c => new
                {
                    c.CategoryId,
                    c.CategoryName
                })
                .ToListAsync();

            return Ok(categories);
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
