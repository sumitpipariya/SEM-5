using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mobile_Mart.Models;

public partial class Brand
{
    public int BrandId { get; set; }

    public string BrandName { get; set; } = null!;

    public int? UserId { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    [JsonIgnore]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    [JsonIgnore]
    public virtual User? User { get; set; }
}
