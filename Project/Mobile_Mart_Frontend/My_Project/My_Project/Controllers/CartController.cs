using System.Text;
using Microsoft.AspNetCore.Mvc;
using My_Project.Models;
using Newtonsoft.Json;

namespace My_Project.Controllers
{
    [CheckAccess]
    public class CartController : Controller
    {
        private readonly HttpClient _client;
        private readonly ILogger<CartController> _logger;

        public CartController(IHttpClientFactory httpClientFactory, ILogger<CartController> logger)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7270/api/CartAPI/");
            _logger = logger;
        }


        #region Get all Cart
        public async Task<IActionResult> GetAllCarts()
        {
            var response = await _client.GetAsync("All");
            var json = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<CartModel>>(json);
            return View(list);
        }
        #endregion

        #region Get Cart Details
        public async Task<IActionResult> CartDetails(int id)
        {
            var response = await _client.GetAsync($"{id}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var cart = JsonConvert.DeserializeObject<CartModel>(json);
                return View(cart);
            }

            return NotFound("Not Found");
        }
        #endregion

        #region Dropdown Helpers
        private async Task<List<ProductModel>> GetAllProductsAsync()
        {
            var response = await _client.GetAsync("Product");
            if (!response.IsSuccessStatusCode) return new List<ProductModel>();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ProductModel>>(json);
        }
        #endregion

        #region AddEdit
        public async Task<IActionResult> AddEdit(int? id)
        {
            CartModel model = new CartModel();

            if (id.HasValue && id.Value > 0)
            {
                var response = await _client.GetAsync($"{id.Value}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Cart not found.";
                    return RedirectToAction("GetAllCarts");
                }

                var json = await response.Content.ReadAsStringAsync();
                model = JsonConvert.DeserializeObject<CartModel>(json) ?? new CartModel();
            }

            model.Products = await GetAllProductsAsync() ?? new List<ProductModel>();

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> AddEdit(CartModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Products = await GetAllProductsAsync() ?? new List<ProductModel>();
                return View(model);
            }

            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                if (model.CartId == 0)
                {
                    response = await _client.PostAsync("", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Create failed: {errorMsg}");
                        TempData["Error"] = "Failed to create cart.";

                        model.Products = await GetAllProductsAsync();
                        return View(model);
                    }

                    TempData["Success"] = "Cart created successfully.";
                }
                else
                {
                    response = await _client.PutAsync($"{model.CartId}", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Update failed: {errorMsg}");
                        TempData["Error"] = "Failed to update cart.";

                        model.Products = await GetAllProductsAsync();
                        return View(model);
                    }

                    TempData["Success"] = "Cart updated successfully.";
                }

                return RedirectToAction("GetAllCarts");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving cart with ID {model.CartId}");
                TempData["Error"] = "Unexpected error occurred while saving cart.";

                
            }
            return View(model);
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
                    TempData["Success"] = "Cart deleted successfully.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Delete failed for Cart {id}: {errorContent}");
                    TempData["Error"] = "Something went wrong while deleting the Cart!!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception while deleting Cart {id}");
                TempData["Error"] = "Something went wrong while deleting the Cart.";
            }

            return RedirectToAction("GetAllCarts");
        }
        #endregion
    }
}
