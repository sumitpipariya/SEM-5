using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Mobile_Mart.Models
{
    public class ProductImage
    {
        public int ImageId { get; set; }

        public int? ProductId { get; set; }

        public string? ImageUrl { get; set; }

        public int? UserId { get; set; }

        [NotMapped]
        public string? ProductName { get; set; }

        [NotMapped]
        public string? FullName { get; set; }

        public IFormFile? ImageFile { get; set; }

        public List<ProductImage>? Images { get; set; }

    }
    public class ProductImageDto
    {
        public int ImageId { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public int? UserId { get; set; }

        [JsonProperty("fullName")]
        public string? FullName { get; set; }
        public string? ImageUrl { get; set; }
    }

}

