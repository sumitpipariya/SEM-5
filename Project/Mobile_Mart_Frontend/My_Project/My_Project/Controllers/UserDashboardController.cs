using Microsoft.AspNetCore.Mvc;
using My_Project.Models;
using Newtonsoft.Json;

namespace My_Project.Controllers
{
    public class UserDashboardController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UserDashboardController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        #region Home page of The User
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:7270/api/ProductAPI/All"); 

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var products = JsonConvert.DeserializeObject<List<ProductModel>>(json);

                return View(products);
            }

            return View(new List<ProductModel>());
        }
        #endregion

        #region Product Details
        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://localhost:7270/api/ProductAPI/{id}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var product = JsonConvert.DeserializeObject<ProductModel>(json);

                if (product != null)
                    return View(product);
            }

            return NotFound(); // If product not found
        }
        #endregion
    }
}
