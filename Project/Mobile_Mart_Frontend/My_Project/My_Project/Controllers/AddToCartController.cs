using Microsoft.AspNetCore.Mvc;
using My_Project.Models;
using System.Text;
using System.Text.Json;

namespace My_Project.Controllers
{
    public class AddToCartController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "https://localhost:7270"; // API base URL

        public AddToCartController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri($"{_apiBaseUrl}/api/");
        }

        #region Cart Details
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
            {
                // Redirect to login if user not logged in
                return RedirectToAction("Login", "Login");
            }

            var response = await _httpClient.GetAsync($"CartAPI/User/{userId}");
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Failed to load cart.";
                return View(new List<CartModel>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var carts = JsonSerializer.Deserialize<List<CartModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<CartModel>();

            // Convert ImageUrl to absolute URLs
            carts.ForEach(c =>
            {
                if (!string.IsNullOrEmpty(c.ImageUrl) && c.ImageUrl.StartsWith("/"))
                {
                    c.ImageUrl = _apiBaseUrl + c.ImageUrl;
                }
            });

            return View(carts);
        }
        #endregion

        #region ADD TO CART
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CartModel cart)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return Unauthorized(new { message = "Please login first!" });

            cart.UserId = userId.Value;

            var jsonData = JsonSerializer.Serialize(cart);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("CartAPI", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { message = $"Error adding to cart: {error}" });
                }

                return Json(new { message = "Product added to cart!" });
            }
            catch (Exception ex)
            {
                return Json(new { message = $"Error connecting to API: {ex.Message}" });
            }
        }
        #endregion

        #region REMOVE FROM CART
        [HttpDelete]
        public async Task<IActionResult> Remove(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return Unauthorized(new { message = "Please login first!" });

            try
            {
                var response = await _httpClient.DeleteAsync($"CartAPI/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return BadRequest(new { message = $"Failed to remove item: {error}" });
                }

                return Ok(new { message = "Item removed successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error connecting to API: {ex.Message}" });
            }
        }
        #endregion

    }
}
