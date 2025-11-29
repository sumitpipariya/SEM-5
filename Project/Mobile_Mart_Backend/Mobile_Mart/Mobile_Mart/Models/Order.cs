using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mobile_Mart.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public DateTime? OrderDate { get; set; }

    public DateTime? Modified { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Status { get; set; }

    [JsonIgnore]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [JsonIgnore]
    public virtual User? User { get; set; }
}
