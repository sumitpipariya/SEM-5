using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using My_Project.Models;

namespace My_Project.Controllers
{
    public class AddToOrderController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "https://localhost:7270"; // API base URL

        public AddToOrderController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri($"{_apiBaseUrl}/api/");
        }

        #region GET ORDER ITEMS BY USER
        public async Task<IActionResult> GetOrderItems()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return Unauthorized();

            try
            {
                var response = await _httpClient.GetAsync($"OrderItemAPI/MyOrders/{userId}");
                if (!response.IsSuccessStatusCode)
                {
                    return View(new List<OrderItemModel>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var orders = JsonSerializer.Deserialize<List<OrderWithItems>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (orders == null || !orders.Any())
                {
                    return View(new List<OrderItemModel>());
                }

                var allOrderItems = orders
                    .Where(o => o.Items != null)
                    .SelectMany(o => o.Items)
                    .ToList();

                return View(allOrderItems);
            }
            catch
            {
                return View(new List<OrderItemModel>());
            }
        }
        #endregion

        [HttpPost]
        public async Task<IActionResult> Buy(int productId)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return Unauthorized();

            try
            {
                // 🔹 Step 1: Get existing orders for this user
                var response = await _httpClient.GetAsync($"OrderItemAPI/MyOrders/{userId}");
                List<OrderWithItems>? orders = null;

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    orders = JsonSerializer.Deserialize<List<OrderWithItems>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                // 🔹 Step 2: Find an active/pending order
                var activeOrder = orders?.FirstOrDefault(o =>
                    o.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                    o.Status.Equals("Active", StringComparison.OrdinalIgnoreCase));

                if (activeOrder != null)
                {
                    var existingItem = activeOrder.Items?
                        .FirstOrDefault(i => i.ProductId == productId);

                    if (existingItem != null)
                    {
                        // ✅ Update quantity
                        existingItem.Quantity += 1;

                        var updateJson = JsonSerializer.Serialize(existingItem);
                        var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");

                        await _httpClient.PutAsync(
                            $"OrderItemAPI/{existingItem.OrderItemId}", updateContent);
                    }
                    else
                    {
                        // ✅ Add new item to existing order
                        var productResp = await _httpClient.GetAsync($"ProductAPI/{productId}");
                        if (!productResp.IsSuccessStatusCode)
                        {
                            TempData["OrderMessage"] = "Could not fetch product details.";
                            return RedirectToAction("GetOrderItems");
                        }

                        var productJson = await productResp.Content.ReadAsStringAsync();
                        var product = JsonSerializer.Deserialize<ProductModel>(productJson,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (product == null)
                        {
                            TempData["OrderMessage"] = "Invalid product.";
                            return RedirectToAction("GetOrderItems");
                        }

                        var newItem = new
                        {
                            OrderId = activeOrder.OrderId,
                            ProductId = productId,
                            Quantity = 1,
                            UserId = userId.Value,
                            Price = product.Price ?? 0
                        };

                        var addJson = JsonSerializer.Serialize(newItem);
                        var addContent = new StringContent(addJson, Encoding.UTF8, "application/json");

                        await _httpClient.PostAsync("OrderItemAPI", addContent);
                    }
                }
                else
                {
                    // 🔹 Step 3: No active order → call BuyNow API (corrected)
                    var request = new { UserId = userId.Value, ProductId = productId, Quantity = 1 };

                    var buyNowJson = JsonSerializer.Serialize(request);
                    var content = new StringContent(buyNowJson, Encoding.UTF8, "application/json");

                    var buyNowResp = await _httpClient.PostAsync("OrderItemAPI/BuyNow", content);

                    if (!buyNowResp.IsSuccessStatusCode)
                    {
                        TempData["OrderMessage"] = "Could not create a new order.";
                        return RedirectToAction("GetOrderItems");
                    }
                }

                return RedirectToAction("GetOrderItems");
            }
            catch
            {
                TempData["OrderMessage"] = "Something went wrong while processing your order.";
                return RedirectToAction("GetOrderItems");
            }
        }

    }

    // 🔹 Response Models
    public class BuyNowResponse
    {
        public int OrderId { get; set; }
    }

    public class OrderWithItems
    {
        public int OrderId { get; set; }
        public string Status { get; set; } = "Pending";
        public List<OrderItemModel> Items { get; set; } = new();
    }
}
