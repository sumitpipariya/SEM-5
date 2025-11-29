using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mobile_Mart.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? Phone { get; set; }

    public string? Role { get; set; }

    public string? AddressLine { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Pincode { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    [JsonIgnore]
    public virtual ICollection<Brand> Brands { get; set; } = new List<Brand>();

    [JsonIgnore]
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();    

    [JsonIgnore]    
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    [JsonIgnore]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [JsonIgnore]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    [JsonIgnore]
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    [JsonIgnore]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}


