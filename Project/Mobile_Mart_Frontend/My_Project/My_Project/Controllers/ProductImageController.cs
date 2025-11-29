using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mobile_Mart.Models;
using My_Project.Models;
using Newtonsoft.Json;

namespace Mobile_Mart.Controllers
{
    public class ProductImageController : Controller
    {
        private readonly HttpClient _httpClient;

        public ProductImageController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            /*_httpClient.BaseAddress = new Uri("https://localhost:7270/api/ProductImageAPI/");*/
        }

        // 🔹 GET: List of Images
        public async Task<IActionResult> Index()
        {
            var images = new List<ProductImage>();

            try
            {
                var response = await _httpClient.GetAsync("https://localhost:7270/api/ProductImageAPI/Index");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    images = JsonConvert.DeserializeObject<List<ProductImage>>(result,
                        new JsonSerializerSettings
                        {
                            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                        }) ?? new();
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to fetch images from API.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return View(images);
        }

        // 🔹 GET: Add or Edit form
        public async Task<IActionResult> AddEdit(int? id)
        {
            var model = new ProductImage();

            // If editing, get existing image
            if (id.HasValue)
            {
                var response = await _httpClient.GetAsync($"GetById/{id.Value}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    model = JsonConvert.DeserializeObject<ProductImage>(result) ?? new ProductImage();
                }
            }

            // Populate Product dropdown
            ViewBag.ProductList = await GetProductsDropdown();

            // Populate User dropdown
            ViewBag.UserList = await GetUsersDropdown();

            return View(model);
        }

        // 🔹 POST: Save (Insert / Update)
        [HttpPost]
        public async Task<IActionResult> Save(ProductImage dto)
        {
            using var formData = new MultipartFormDataContent();

            if (dto.ImageFile != null)
                formData.Add(new StreamContent(dto.ImageFile.OpenReadStream()), "ImageFile", dto.ImageFile.FileName);

            if (dto.ImageId > 0)
                formData.Add(new StringContent(dto.ImageId.ToString()), "ImageId");
            if (dto.ProductId.HasValue)
                formData.Add(new StringContent(dto.ProductId.ToString()), "ProductId");
            if (dto.UserId.HasValue)
                formData.Add(new StringContent(dto.UserId.ToString()), "UserId");

            var response = await _httpClient.PostAsync("Save", formData);

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Image saved successfully!";
            else
                TempData["ErrorMessage"] = "Failed to save image.";

            return RedirectToAction("Index");
        }

        // 🔹 POST: Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int imageId)
        {
            var response = await _httpClient.DeleteAsync($"DeleteById/{imageId}");

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Image deleted successfully!";
            else
                TempData["ErrorMessage"] = "Failed to delete image.";

            return RedirectToAction("Index");
        }

        // Helper: Populate Product dropdown
        private async Task<List<SelectListItem>> GetProductsDropdown()
        {
            var list = new List<SelectListItem>();
            var response = await _httpClient.GetAsync("https://localhost:7270/api/ProductAPI/Dropdown"); // Make sure API endpoint exists
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var products = JsonConvert.DeserializeObject<List<ProductModel>>(json) ?? new();
                list = products.Select(p => new SelectListItem(p.ProductName, p.ProductId.ToString())).ToList();
            }
            return list;
        }

        // Helper: Populate User dropdown
        private async Task<List<SelectListItem>> GetUsersDropdown()
        {
            var list = new List<SelectListItem>();
            var response = await _httpClient.GetAsync("https://localhost:7270/api/UserAPI/Dropdown"); 
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<UserModel>>(json) ?? new();
                list = users.Select(u => new SelectListItem(u.FullName, u.UserID.ToString())).ToList();
            }
            return list;
        }
    }
}
