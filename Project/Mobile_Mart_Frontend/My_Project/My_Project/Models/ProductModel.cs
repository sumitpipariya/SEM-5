using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Mobile_Mart.Models;

namespace My_Project.Models
{
    public class ProductModel
    {
        public int ProductId { get; set; }

        public string? ProductName { get; set; }

        public int? BrandId { get; set; }

        public int? CategoryId { get; set; }

        public int? UserId { get; set; }

        public decimal? Price { get; set; }

        public string? Description { get; set; }

      
        public DateTime? Created { get; set; }

        public DateTime? Modified { get; set; }

        [JsonIgnore]
        public CategoryModel? Category { get; set; }

        [JsonIgnore]
        public UserModel? User { get; set; }

        [JsonIgnore]
        public BrandModel? Brand { get; set; }

       
        public string? BrandName { get; set; }

        public string? CategoryName { get; set; }
        
        public string? FullName { get; set; }

        public List<BrandModel> Brands { get; set; } = new();

        public List<CategoryModel> Categories { get; set; } = new();

        public List<UserModel> Users { get; set; } = new();

        public string? ImageUrl { get; set; }

        public List<ProductImage>? Images { get; set; }

    }
}
