using System;
using System.Collections.Generic;
using BuildingBlocks.Entity;

namespace Catalog.Domain.Aggregates.Product;

// Domain-specific exceptions for better error handling
public class InvalidProductNameException : ArgumentException
{
    public InvalidProductNameException(string message) : base(message) { }
}

public class InvalidProductPriceException : ArgumentException
{
    public InvalidProductPriceException(string message) : base(message) { }
}

public class InvalidCategoryException : ArgumentException
{
    public InvalidCategoryException(string message) : base(message) { }
}

public class Product : BaseEntity<Guid>
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
        ValidateName(name);
        ValidatePrice(price);

        return new Product
        {
            Name = name.Trim(),
            Price = price,
            Description = description?.Trim() ?? string.Empty,
            ImageUrl = imageUrl?.Trim() ?? string.Empty
        };
    }

    public void UpdatePrice(decimal newPrice)
    {
        ValidatePrice(newPrice);
        Price = newPrice;
    }

    public void AddCategory(string category)
    {
        ValidateCategory(category);
        if (!_categories.Contains(category.Trim()))
        {
            _categories.Add(category.Trim());
        }
    }

    public void UpdateDetails(string name, string description, string imageUrl)
    {
        ValidateName(name);
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        ImageUrl = imageUrl?.Trim() ?? string.Empty;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidProductNameException("Product name cannot be empty or whitespace");
        }

        if (name.Length > 200)
        {
            throw new InvalidProductNameException("Product name cannot exceed 200 characters");
        }
    }

    private static void ValidatePrice(decimal price)
    {
        if (price < 0)
        {
            throw new InvalidProductPriceException("Product price cannot be negative");
        }

        if (price > 999999.99m)
        {
            throw new InvalidProductPriceException("Product price cannot exceed 999,999.99");
        }
    }

    private static void ValidateCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            throw new InvalidCategoryException("Category cannot be empty or whitespace");
        }

        if (category.Length > 50)
        {
            throw new InvalidCategoryException("Category cannot exceed 50 characters");
        }
    }
}
