using System.Text;
using Microsoft.AspNetCore.Mvc;
using My_Project.Models;
using Newtonsoft.Json;

namespace My_Project.Controllers
{
    [CheckAccess]
    public class ProductController : Controller
    {

        private readonly HttpClient _client;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IHttpClientFactory httpClientFactory, ILogger<ProductController> logger)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7270/api/ProductAPI/");
            _logger = logger;
        }


        #region Get all Product
        public async Task<IActionResult> GetAllProduct()
        {
            var response = await _client.GetAsync("All");
            var json = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<ProductModel>>(json);
            return View(list);
        }
        #endregion

        #region Get Product Details
        public async Task<IActionResult> ProductDetails(int id)
        {
            var response = await _client.GetAsync($"{id}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var product = JsonConvert.DeserializeObject<ProductModel>(json);
                return View(product);
            }

            return NotFound("Not Found");
        }
        #endregion

        #region Delete Product
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Product deleted successfully.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Delete failed for Product {id}: {errorContent}");
                    TempData["Error"] = "Something went wrong while deleting the Product!!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception while deleting product {id}");
                TempData["Error"] = "Something went wrong while deleting the product.";
            }

            return RedirectToAction("GetAllProduct");
        }
        #endregion

        #region Dropdown Helpers
        private async Task<List<BrandModel>> GetAllBrandsAsync()
        {
            var response = await _client.GetAsync("Brand");
            if (!response.IsSuccessStatusCode) return new List<BrandModel>();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<BrandModel>>(json);
        }

        private async Task<List<CategoryModel>> GetAllCategoriesAsync()
        {
            var response = await _client.GetAsync("Category");
            if (!response.IsSuccessStatusCode) return new List<CategoryModel>();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<CategoryModel>>(json);
        }

        #endregion

        #region AddEdit
        public async Task<IActionResult> AddEdit(int? id)
         {
            ProductModel model;

            if (id != null)
            {
                var response = await _client.GetAsync($"{id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("GetAllProduct");
                }

                var json = await response.Content.ReadAsStringAsync();
                model = JsonConvert.DeserializeObject<ProductModel>(json) ?? new ProductModel();
            }
            else
            {
                model = new ProductModel();
            }

            // Populate dropdowns
            model.Brands = await GetAllBrandsAsync() ?? new List<BrandModel>();
            model.Categories = await GetAllCategoriesAsync() ?? new List<CategoryModel>();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(ProductModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Brands = await GetAllBrandsAsync() ?? new List<BrandModel>();
                model.Categories = await GetAllCategoriesAsync() ?? new List<CategoryModel>();
                return View(model);
            }

            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                if (model.ProductId == 0)
                {
                    response = await _client.PostAsync("", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Create failed: {errorMsg}");
                        TempData["Error"] = "Failed to create product.";

                        model.Brands = await GetAllBrandsAsync();
                        model.Categories = await GetAllCategoriesAsync();

                        return View(model);
                    }

                    TempData["Success"] = "Product created successfully.";
                }
                else
                {
                    response = await _client.PutAsync($"{model.ProductId}", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Update failed: {errorMsg}");
                        TempData["Error"] = "Failed to update product.";

                        model.Brands = await GetAllBrandsAsync();
                        model.Categories = await GetAllCategoriesAsync();
                        

                        return View(model);
                    }

                    TempData["Success"] = "Product updated successfully.";
                }

                return RedirectToAction("GetAllProduct");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving product with ID {model.ProductId}");
                TempData["Error"] = "Unable to save product.";

                model.Brands = await GetAllBrandsAsync();
                model.Categories = await GetAllCategoriesAsync();
                

                //return View(model);
            }
            return View(model);
        }
        #endregion


    }
}
