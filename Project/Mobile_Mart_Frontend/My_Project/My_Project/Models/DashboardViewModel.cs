using My_Project.Models;

public class DashboardViewModel
{
    // Counts / KPIs
    public int UserCount { get; set; }
    public int ProductCount { get; set; }
    public int OrderCount { get; set; }
    public int BrandCount { get; set; }
    public int CategoryCount { get; set; }
    public int CartCount { get; set; }
    public decimal Revenue { get; set; }

    // Recent Entities
    public List<UserModel> RecentUsers { get; set; } = new();
    public List<ProductModel> RecentProducts { get; set; } = new();
    public List<OrderModel> RecentOrders { get; set; } = new();

    // Charts
    public List<string> SalesMonths { get; set; } = new();
    public List<decimal> SalesData { get; set; } = new();
    public List<string> CategoryLabels { get; set; } = new();
    public List<int> CategoryData { get; set; } = new();
}
