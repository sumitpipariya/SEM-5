using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mobile_Mart.Models;

public partial class Product
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
    public virtual Brand? Brand { get; set; } = null;

    [JsonIgnore]
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    [JsonIgnore]
    public virtual Category? Category { get; set; } = null;

    [JsonIgnore]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [JsonIgnore]
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    [JsonIgnore]
    public virtual User? User { get; set; } = null;

   
}
