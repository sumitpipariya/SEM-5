using System.Text.Json.Serialization;

namespace My_Project.Models
{
    public class CategoryModel
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = null!;

        public int? UserId { get; set; }

        public string? FullName { get; set; }

        public DateTime? Created { get; set; } 

        public DateTime? Modified { get; set; }

        [JsonIgnore]
        public virtual UserModel? User { get; set; } = null;
    }
}
