using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mobile_Mart.Models;

public partial class Cart
{
    public int CartId { get; set; }

    public int? UserId { get; set; }

    public int? ProductId { get; set; }

    public int? Quantity { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    [JsonIgnore]
    public virtual Product? Product { get; set; }

    [JsonIgnore]
    public virtual User? User { get; set; }
}
