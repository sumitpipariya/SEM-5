using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mobile_Mart.Models;

public partial class OrderItem
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
    public virtual Order? Order { get; set; }

    [JsonIgnore]
    public virtual Product? Product { get; set; }

    [JsonIgnore]
    public virtual User? User { get; set; }
}
