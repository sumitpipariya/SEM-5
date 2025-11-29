using System.Text;
using Microsoft.AspNetCore.Mvc;
using My_Project.Models;
using Newtonsoft.Json;

namespace My_Project.Controllers
{
    [CheckAccess]
    public class OrderItemsController : Controller
    {
        private readonly HttpClient _client;
        private readonly ILogger<ProductController> _logger;

        public OrderItemsController(IHttpClientFactory httpClientFactory, ILogger<ProductController> logger)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7270/api/OrderItemAPI/");
            _logger = logger;
        }

        #region Get all Order Items
        public async Task<IActionResult> GetAllOrderItem()
        {
            var response = await _client.GetAsync("All");
            var json = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<OrderItemModel>>(json);
            return View(list);
        }
        #endregion

        #region Get OrderItem Details
        public async Task<IActionResult> OrderItemDetails(int id)
        {
            var response = await _client.GetAsync($"{id}");

            if (response.IsSuccessStatusCode)   
            {
                var json = await response.Content.ReadAsStringAsync();
                var item = JsonConvert.DeserializeObject<OrderItemModel>(json);
                return View(item);
            }

            return NotFound("Order item not found.");
        }
        #endregion

        #region Dropdown
        private async Task<List<ProductModel>> GetAllProductsAsync()
        {
            var response = await _client.GetAsync("Product");
            if (!response.IsSuccessStatusCode)
                return new List<ProductModel>();

            var json = await response.Content.ReadAsStringAsync();
            var products = JsonConvert.DeserializeObject<List<ProductModel>>(json);
            return products ?? new List<ProductModel>();
        }
        private async Task<List<OrderModel>> GetAllOrdersAsync()
        {
            var response = await _client.GetAsync("Order");
            if (!response.IsSuccessStatusCode)
                return new List<OrderModel>();

            var json = await response.Content.ReadAsStringAsync();
            var orders = JsonConvert.DeserializeObject<List<OrderModel>>(json);
            return orders ?? new List<OrderModel>();
        }

        #endregion

        #region AddEdit OrderItem (With Product Dropdown)
        public async Task<IActionResult> AddEdit(int? id)
            {
            OrderItemModel model;

            if (id != null)
            {
                var response = await _client.GetAsync($"{id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Order Item not found.";
                    return RedirectToAction("GetAllOrderItem");
                }

                var json = await response.Content.ReadAsStringAsync();
                model = JsonConvert.DeserializeObject<OrderItemModel>(json) ?? new OrderItemModel();
            }
            else
            {
                model = new OrderItemModel();
            }

            model.Products = await GetAllProductsAsync();
            model.Orders = await GetAllOrdersAsync();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(OrderItemModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Products = await GetAllProductsAsync();
                model.Orders = await GetAllOrdersAsync();
                return View(model);
            }

            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                if (model.OrderItemId == 0)
                {
                    response = await _client.PostAsync("", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Create failed: {errorMsg}");
                        TempData["Error"] = "Failed to create Order Item.";

                        model.Products = await GetAllProductsAsync();
                        model.Orders = await GetAllOrdersAsync();
                        return View(model);
                    }

                    TempData["Success"] = "Order Item created successfully.";
                }
                else
                {
                    response = await _client.PutAsync($"{model.OrderItemId}", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Update failed: {errorMsg}");
                        TempData["Error"] = "Failed to update Order Item.";

                        model.Products = await GetAllProductsAsync();
                        model.Orders = await GetAllOrdersAsync();
                        return View(model);
                    }

                    TempData["Success"] = "Order Item updated successfully.";
                }

                return RedirectToAction("GetAllOrderItem");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving Order Item with ID {model.OrderItemId}");
                TempData["Error"] = "Unable to save Order Item.";

                model.Products = await GetAllProductsAsync();
                model.Orders = await GetAllOrdersAsync();
                return View(model);
            }
        }
        #endregion

        #region Delete Order Items
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Order Item deleted successfully.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Delete failed for Order Item {id}: {errorContent}");
                    TempData["Error"] = "Something went wrong while deleting the Order Item!!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception while deleting Order Item {id}");
                TempData["Error"] = "Something went wrong while deleting the Order Item.";
            }

            return RedirectToAction("GetAllOrderItem");
        }
        #endregion

    }
}
