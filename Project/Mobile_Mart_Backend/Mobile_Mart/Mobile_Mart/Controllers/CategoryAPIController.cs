using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mobile_Mart.Models;

namespace Mobile_Mart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryAPIController : ControllerBase
    {
        private readonly MobileMartsContext _context;
        private readonly IValidator<Category> _validator;
        public CategoryAPIController(MobileMartsContext context, IValidator<Category> validator)
        {
            _context = context;
            _validator = validator;
        }

        #region Get All Categories
        [HttpGet("All")]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories()
        {
            var category = await _context.Categories
           .Include(oi => oi.User)
           .Select(oi => new
           {
               oi.CategoryId,
               oi.CategoryName,

               oi.UserId,
               FullName = oi.User != null ? oi.User.FullName : "N/A",

               oi.Created,
               oi.Modified
           })
           .OrderByDescending(oi => oi.Created)
           .ToListAsync();

            return Ok(category);
        }
        #endregion

        #region Get Category by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategoryById(int id)
        {
            var item = await _context.Categories
                  .Where(o => o.CategoryId == id)
                  .Select(o => new
                  {
                      CategoryId = o.CategoryId,
                      CategoryName = o.CategoryName,

                      UserId = o.User != null ? o.User.UserId : 0,
                      FullName = o.User != null ? o.User.FullName : "N/A",

                      Created = o.Created,
                      Modified = o.Modified
                  })
                  .FirstOrDefaultAsync();

            return item == null ? NotFound() : Ok(item);
        }
        #endregion

        #region Create Category
        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] Category category)
        {
            var validationResult = await _validator.ValidateAsync(category);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new
                {
                    Property = e.PropertyName,
                    Error = e.ErrorMessage
                }));
            }

            var userExists = await _context.Users.AnyAsync(u => u.UserId == category.UserId);
            if (!userExists)
            {
                return BadRequest($"User with ID {category.UserId} does not exist.");
            }

            category.Created = DateTime.Now;
            category.Modified = DateTime.Now;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoryById), new { id = category.CategoryId }, category);
        }
        #endregion

        #region Update Category
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, Category category)
        {
            if (id != category.CategoryId)
                return BadRequest("Category ID mismatch.");

            category.Modified = DateTime.Now;
            _context.Entry(category).State = EntityState.Modified;

            await _context.SaveChangesAsync();
          

            return NoContent();
        }
        #endregion

        #region Delete Category
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        #region Search by Name
        [HttpGet("Search/{name}")]
        public async Task<ActionResult<IEnumerable<Category>>> SearchCategory(string name)
        {
            var categories = await _context.Categories
                .Where(c => c.CategoryName.Contains(name))
                .ToListAsync();

            return categories.Count == 0 ? NotFound() : Ok(categories);
        }
        #endregion

        #region Dropdown
        [HttpGet("Dropdown")]
        public async Task<ActionResult<IEnumerable<object>>> GetCategoryDropdown()
        {
            var list = await _context.Categories
                .Select(c => new { c.CategoryId, c.CategoryName })
                .ToListAsync();

            return Ok(list);
        }
        #endregion
    }
}
