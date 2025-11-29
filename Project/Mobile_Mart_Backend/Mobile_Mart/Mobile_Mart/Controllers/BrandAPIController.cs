using FluentValidation;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mobile_Mart.Models;

namespace Mobile_Mart.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class BrandAPIController : ControllerBase
    {
        #region Constructor & Context
        private readonly MobileMartsContext _context;
        private readonly IValidator<Brand> _validator;
        public BrandAPIController(MobileMartsContext context,IValidator<Brand> validator)
        {
            _context = context;
            _validator = validator;
        }
        #endregion

        #region Get All Brands
        [HttpGet("All")]
        public async Task<ActionResult<IEnumerable<Brand>>> GetAllBrands()
        {
            var brands = await _context.Brands
                .Include(b => b.User)
                .Select(b => new
                {
                    b.BrandId,
                    b.BrandName,

                    b.UserId,
                    FullName = b.User != null ? b.User.FullName : "N/A",

                    b.Created,
                    b.Modified
                })
                .ToListAsync();

            return Ok(brands);
        }
        #endregion

        #region Get Brand By ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Brand>> GetBrandById(int id)
        {
            var brand = await _context.Brands
                .Where(c => c.BrandId == id)
                .Select(c => new
                {
                    BrandId = c.BrandId,
                    BrandName = c.BrandName,

                    UserId = c.User != null ? c.User.UserId : 0,
                    FullName = c.User != null ? c.User.FullName : "N/A",

                    Created = c.Created,
                    Modified = c.Modified
                })
                .FirstOrDefaultAsync();
            return brand == null ? NotFound() : Ok(brand);
        }
        #endregion

        #region Create New Brand
        [HttpPost]
        public async Task<ActionResult<Brand>> CreateBrand([FromBody] Brand brand)
        {
            var validationResult = await _validator.ValidateAsync(brand);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new
                {
                    Property = e.PropertyName,
                    Error = e.ErrorMessage
                }));
            }

            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBrandById), new { id = brand.BrandId }, brand);
        }
        #endregion

        #region Update Brand
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBrand(int id, Brand brand)
        {
            if (id != brand.BrandId)
                return BadRequest("Brand ID mismatch.");

            brand.Modified = DateTime.Now;
            _context.Entry(brand).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        #region Delete Brand
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
                return NotFound();

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        #region Search Brand By Name
        [HttpGet("Search/{name}")]
        public async Task<ActionResult<IEnumerable<Brand>>> SearchBrandByName(string name)
        {
            var brands = await _context.Brands
                .Where(b => b.BrandName.Contains(name))
                .ToListAsync();

            if (brands == null || brands.Count == 0)
                return NotFound("No brands found with the given name.");

            return Ok(brands);
        }
        #endregion

        #region Dropdown Brands
        [HttpGet("Dropdown")]
        public async Task<ActionResult<IEnumerable<object>>> GetBrandDropdown()
        {
            var brands = await _context.Brands
                .Select(b => new { b.BrandId, b.BrandName })
                .ToListAsync();

            return Ok(brands);
        }
        #endregion
       
    }
}
