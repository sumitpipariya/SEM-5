using System.Text;
using Microsoft.AspNetCore.Mvc;
using My_Project.Models;
using Newtonsoft.Json;

namespace My_Project.Controllers
{
    [CheckAccess]
    public class OrdersController : Controller
    {
        private readonly HttpClient _client;
        private readonly ILogger<ProductController> _logger;
        public OrdersController(IHttpClientFactory httpClientFactory, ILogger<ProductController> logger)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7270/api/OrderAPI/");
            _logger = logger;
        }

        #region Get all Order
        public async Task<IActionResult> GetAllOrder()
        {
            var response = await _client.GetAsync("All");
            var json = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<OrderModel>>(json);
            return View(list);
        }
        #endregion

        #region Get Order Details
        public async Task<IActionResult> OrderDetails(int id)
        {
            var response = await _client.GetAsync($"{id}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var order = JsonConvert.DeserializeObject<OrderModel>(json);
                return View(order);
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
                    TempData["Success"] = "Order deleted successfully.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Delete failed for Order {id}: {errorContent}");
                    TempData["Error"] = "Something went wrong while deleting the Order!!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception while deleting Order {id}");
                TempData["Error"] = "Something went wrong while deleting the Order.";
            }

            return RedirectToAction("GetAllOrder");
        }
        #endregion

        #region AddEdit
        public async Task<IActionResult> AddEdit(int? id)
        {
            OrderModel model;

            if (id != null)
            {
                var response = await _client.GetAsync($"{id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Order not found.";
                    return RedirectToAction("GetAllOrder");
                }

                var json = await response.Content.ReadAsStringAsync();
                model = JsonConvert.DeserializeObject<OrderModel>(json) ?? new OrderModel();
            }
            else
            {
                model = new OrderModel();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(OrderModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                if (model.OrderId == 0)
                {
                    response = await _client.PostAsync("", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Create failed: {errorMsg}");
                        TempData["Error"] = "Failed to create order.";

                        return View(model);
                    }

                    TempData["Success"] = "Order created successfully.";
                }
                else
                {
                    response = await _client.PutAsync($"{model.OrderId}", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Update failed: {errorMsg}");
                        TempData["Error"] = "Failed to update order.";

                        return View(model);
                    }

                    TempData["Success"] = "Order updated successfully.";
                }

                return RedirectToAction("GetAllOrder");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving order with ID {model.OrderId}");
                TempData["Error"] = "Unable to save order.";

            }

            return View(model);
        }
        #endregion

    }
}
