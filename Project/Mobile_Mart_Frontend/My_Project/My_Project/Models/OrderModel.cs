using System.Text.Json.Serialization;

namespace My_Project.Models
{
    public class OrderModel
    {
        public int OrderId { get; set; }

        public int? UserId { get; set; }

        public DateTime? OrderDate { get; set; }

        public DateTime? Modified { get; set; }

        public decimal? TotalAmount { get; set; }

        public string? Status { get; set; }

        public string? FullName { get; set; }

        public string? OrderName { get; set; }

        [JsonIgnore]
        public  UserModel? User { get; set; }

        public List<UserModel> Users { get; set; } = new();

        public OrderModel()
        {
            Users = new List<UserModel>();
            OrderDate = DateTime.Now;
            Status = "Pending";
        }
    }
}
