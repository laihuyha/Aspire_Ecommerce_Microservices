using System;
using System.Collections.Generic;
using BuildingBlocks.Enity;

namespace Domain;

public class Product : BaseEntity
{
    public string Name { get; private set; }
    public IReadOnlyList<string> Categories => _categories.AsReadOnly();
    public string Description { get; private set; }
    public string ImageUrl { get; private set; }
    public decimal Price { get; private set; }

    private readonly List<string> _categories = new();

    private Product() { } // For ORM

    public static Product Create(string name, decimal price, string description = "", string imageUrl = "")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required", nameof(name));
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        return new Product
        {
            Name = name,
            Price = price,
            Description = description ?? string.Empty,
            ImageUrl = imageUrl ?? string.Empty
        };
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("Price cannot be negative", nameof(newPrice));
        Price = newPrice;
    }

    public void AddCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be empty", nameof(category));
        if (!_categories.Contains(category))
            _categories.Add(category);
    }

    public void UpdateDetails(string name, string description, string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required", nameof(name));
        
        Name = name;
        Description = description ?? string.Empty;
        ImageUrl = imageUrl ?? string.Empty;
    }
}