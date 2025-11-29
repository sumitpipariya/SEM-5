using System.Text.Json.Serialization;

namespace My_Project.Models
{
    public class BrandModel
    {
        public int BrandId { get; set; }

        public string BrandName { get; set; } = null!;

        public int? UserId { get; set; }

        public string? FullName { get; set; } 

        public DateTime? Created { get; set; }

        public DateTime? Modified { get; set; }

        [JsonIgnore]
        public virtual ICollection<ProductModel> Products { get; set; } = new List<ProductModel>();

        [JsonIgnore]
        public virtual UserModel? User { get; set; }

    }
}
