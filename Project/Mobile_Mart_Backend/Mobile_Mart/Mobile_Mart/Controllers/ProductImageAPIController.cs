using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mobile_Mart.Models;

namespace Mobile_Mart.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProductImageAPIController : ControllerBase
    {
        // temporary in-memory store
        static List<ProductImage> productImages = new List<ProductImage>();

        private readonly MobileMartsContext _context;

        public ProductImageAPIController(MobileMartsContext context)
        {
            _context = context;
        }

        #region Get All Images
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var images = await _context.ProductImages.ToListAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host}/";
            foreach (var img in images)
            {
                if (!string.IsNullOrEmpty(img.ImageUrl))
                    img.ImageUrl = baseUrl + img.ImageUrl.TrimStart('/');
            }

            return Ok(images);
        }
        #endregion

        #region Get Image By ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var image = await _context.ProductImages.FirstOrDefaultAsync(x => x.ImageId == id);

            if (image == null)
                return NotFound();

            var baseUrl = $"{Request.Scheme}://{Request.Host}/";
            image.ImageUrl = baseUrl + (image.ImageUrl?.TrimStart('/') ?? string.Empty);

            return Ok(image);
        }

        #endregion

        #region Insert the Image
        [HttpPost]
        public async Task<IActionResult> Save([FromForm] ProductImageUploadDto dto)
        {
            try
            {
                if (dto.ImageFile == null)
                    return BadRequest(new { message = "Image file is required." });

                // Save image
                string savedPath = await ImageHelper.SaveFileAsync(dto.ImageFile);

                if (string.IsNullOrEmpty(savedPath))
                    return BadRequest(new { message = "Failed to upload image." });

                // Map DTO to Entity
                var productImage = new ProductImage
                {
                    ImageId = dto.ImageId ?? 0,
                    ProductId = dto.ProductId,
                    UserId = dto.UserId,
                    ImageUrl = savedPath
                };

                if (dto.ImageId.HasValue && dto.ImageId > 0)
                    _context.ProductImages.Update(productImage);
                else
                    _context.ProductImages.Add(productImage);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Image saved successfully.",
                    data = productImage
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = "Database update failed.", details = dbEx.InnerException?.Message ?? dbEx.Message });
            }
            catch (IOException ioEx)
            {
                return StatusCode(500, new { message = "File system error.", details = ioEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }



        #endregion

        #region DELETE THE IMAGE
        [HttpDelete("{imageId}")]
        public async Task<IActionResult> DeleteById(int imageId)
        {
            try
            {
                var productImage = await _context.ProductImages.FindAsync(imageId);
                if (productImage == null)
                    return NotFound(new { message = "Image not found." });

                if (!string.IsNullOrEmpty(productImage.ImageUrl))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", productImage.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.ProductImages.Remove(productImage);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Image deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the image.", details = ex.Message });
            }
        }
        #endregion


    }
}
