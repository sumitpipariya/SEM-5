using Microsoft.AspNetCore.Mvc;

using My_Project.Models;
using System.Net.Http.Headers;

[CheckAccess]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly HttpClient _httpClient;

    
    public HomeController(
        ILogger<HomeController> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();

        // 🔑 Set JWT token if available
        var token = httpContextAccessor.HttpContext?.Session.GetString("JWTToken");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _logger.LogWarning("JWT Token not found in session.");
        }
    }
    
    public async Task<IActionResult> Index()
{
    var vm = new DashboardViewModel();

    try
    {
        var token = HttpContext.Session.GetString("JWTToken");
        if (string.IsNullOrEmpty(token))
        {
            TempData["Error"] = "Please login again.";
            return RedirectToAction("Login", "Account");
        }

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Fetch Data from APIs
        var users = await _httpClient.GetFromJsonAsync<List<UserModel>>("https://localhost:7270/api/UserAPI/User");
        var products = await _httpClient.GetFromJsonAsync<List<ProductModel>>("https://localhost:7270/api/ProductAPI/All");
        var orders = await _httpClient.GetFromJsonAsync<List<OrderModel>>("https://localhost:7270/api/OrderAPI/All");
        var brands = await _httpClient.GetFromJsonAsync<List<BrandModel>>("https://localhost:7270/api/BrandAPI/All");
        var categories = await _httpClient.GetFromJsonAsync<List<CategoryModel>>("https://localhost:7270/api/CategoryAPI/All");
        var carts = await _httpClient.GetFromJsonAsync<List<CartModel>>("https://localhost:7270/api/CartAPI/All");

        // KPI Counts
        vm.UserCount = users?.Count ?? 0;
        vm.ProductCount = products?.Count ?? 0;
        vm.OrderCount = orders?.Count ?? 0;
        vm.BrandCount = brands?.Count ?? 0;
        vm.CategoryCount = categories?.Count ?? 0;
        vm.CartCount = carts?.Count ?? 0;
        vm.Revenue = orders?.Sum(o => o.TotalAmount ?? 0) ?? 0;

        // Recent data
        vm.RecentUsers = users?.OrderByDescending(u => u.Created).Take(5).ToList() ?? new();
        vm.RecentProducts = products?.OrderByDescending(p => p.Created).Take(5).ToList() ?? new();
        vm.RecentOrders = orders?.OrderByDescending(o => o.OrderDate).Take(5).ToList() ?? new();

        // Sales Chart Data
        vm.SalesMonths = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };
        vm.SalesData = new List<decimal> { 1200, 2000, 3200, 2800, 3500, 4500 };

        // Orders by Category
        var catGroup = products?.GroupBy(p => p.CategoryName)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToList();

        if (catGroup != null)
        {
            vm.CategoryLabels = catGroup.Select(c => c.Category ?? "Unknown").ToList();
            vm.CategoryData = catGroup.Select(c => c.Count).ToList();
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error loading dashboard");
        TempData["Error"] = "Could not load dashboard data.";
    }

    return View(vm);
}



}
