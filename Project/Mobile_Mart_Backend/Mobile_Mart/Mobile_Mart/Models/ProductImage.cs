using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Mobile_Mart.Models;

public partial class ProductImage
{
    public int ImageId { get; set; }

    public int? ProductId { get; set; }

    public string? ImageUrl { get; set; }

    public int? UserId { get; set; }

    

    [NotMapped]   
    [JsonIgnore]  
    public IFormFile? ImageFile { get; set; }

    [JsonIgnore]
    public virtual Product? Product { get; set; }

    [JsonIgnore]
    public virtual User? User { get; set; }
}

public class ProductImageUploadDto
{
    public int? ImageId { get; set; }
    public int ProductId { get; set; }
    public int? UserId { get; set; }

    public IFormFile? ImageFile { get; set; }
}
