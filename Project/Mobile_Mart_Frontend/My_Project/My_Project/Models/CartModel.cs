using System.Text.Json.Serialization;
using My_Project.Models;


namespace My_Project.Models
{
    public class CartModel
    {
        public int CartId { get; set; }
        public int? UserId { get; set; }
        public string? FullName { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public int? Quantity { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }

        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }

        [JsonIgnore]
        public virtual ProductModel? Product { get; set; } = null;

        [JsonIgnore]
        public virtual UserModel? User { get; set; } = null;

        [JsonIgnore]
        public List<ProductModel> Products { get; set; } = new();
    }

}
