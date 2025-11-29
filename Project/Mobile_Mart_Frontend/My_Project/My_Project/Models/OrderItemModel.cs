using System.Text.Json.Serialization;

namespace My_Project.Models
{
    public class OrderItemModel
    {
        public int OrderItemId { get; set; }

        public int? OrderId { get; set; }

        public int? ProductId { get; set; }

        public int? Quantity { get; set; }

        public decimal? Price { get; set; }

        public int? UserId { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? Modified { get; set; }

        [JsonIgnore]
        public virtual OrderModel? Order { get; set; }

        [JsonIgnore]
        public virtual ProductModel? Product { get; set; }

        [JsonIgnore]
        public virtual UserModel? User { get; set; } 

        public string? ProductName {  get; set; }

        public string? FullName {  get; set; }

        public string? OrderName { get; set; }

        public List<OrderModel> Orders { get; set; } = new();

        public List<ProductModel> Products { get; set; } = new();

        public List<UserModel> Users { get; set; } = new();

    }
    // My_Project/Models/OrderWithItems.cs
 
        public class OrderWithItems
        {
            public int OrderId { get; set; }
            public string Status { get; set; } = string.Empty;
            public DateTime Modified { get; set; }
            public List<OrderItemModel> Items { get; set; } = new();
        }
    
}
