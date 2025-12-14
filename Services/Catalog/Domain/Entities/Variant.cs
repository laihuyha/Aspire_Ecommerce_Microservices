using System;
using System.Collections.Generic;
using System.Linq;
using BuildingBlocks.Entity;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities;

// Domain-specific exceptions for better error handling
public class InvalidVariantNameException : Exception
{
    public InvalidVariantNameException(string message) : base(message) { }
}

public class InvalidVariantSKUException : Exception
{
    public InvalidVariantSKUException(string message) : base(message) { }
}

public class VariantQuantityException : Exception
{
    public VariantQuantityException(string message) : base(message) { }
}

public class VariantPriceException : Exception
{
    public VariantPriceException(string message) : base(message) { }
}

// Variant is now an Entity within the Product Aggregate
// Its identity is scoped to the Product aggregate boundary
public class Variant : BaseEntity<Guid>
{
    public string Name { get; private set; }
    public string SKU { get; private set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<ProductAttr> Attributes => _attributes.AsReadOnly();

    private readonly List<ProductAttr> _attributes = new();

    // Note: ProductId is removed since Variant exists only within Product aggregate
    // Its relationship to Product is implicit through containment

    private Variant() { } // For ORM

    private Variant(string name, string sku, decimal price, int stockQuantity)
    {
        Name = name;
        SKU = sku;
        Price = price;
        StockQuantity = stockQuantity;
        IsActive = true;
    }

    public static Variant Create(
        string name,
        string sku,
        decimal price,
        int stockQuantity,
        IEnumerable<ProductAttr> attributes = null)
    {
        ValidateName(name);
        ValidateSKU(sku);
        ValidatePrice(price);
        ValidateStockQuantity(stockQuantity);

        var variant = new Variant(name.Trim(), sku.Trim(), price, stockQuantity);

        if (attributes != null)
        {
            foreach (var attribute in attributes)
            {
                variant.AddAttribute(attribute);
            }
        }

        return variant;
    }

    public void UpdateDetails(string name, string sku, IEnumerable<ProductAttr> attributes)
    {
        ValidateName(name);
        ValidateSKU(sku);

        Name = name.Trim();
        SKU = sku.Trim();

        // Update attributes - replace all existing attributes
        _attributes.Clear();
        foreach (var attribute in attributes ?? Enumerable.Empty<ProductAttr>())
        {
            AddAttribute(attribute);
        }
    }

    public void UpdatePrice(decimal newPrice)
    {
        ValidatePrice(newPrice);
        Price = newPrice;
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new VariantQuantityException("Stock quantity to add must be positive");
        }

        StockQuantity += quantity;
    }

    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new VariantQuantityException("Stock quantity to remove must be positive");
        }

        if (quantity > StockQuantity)
        {
            throw new VariantQuantityException($"Insufficient stock. Available: {StockQuantity}, Requested: {quantity}");
        }

        StockQuantity -= quantity;
    }

    public void SetStock(int newStockQuantity)
    {
        ValidateStockQuantity(newStockQuantity);
        StockQuantity = newStockQuantity;
    }

    public void AddAttribute(ProductAttr attribute)
    {
        // Business rule: Cannot have duplicate attribute names
        if (_attributes.Any(a => a.Name.Equals(attribute.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Attribute '{attribute.Name}' already exists for this variant");
        }

        _attributes.Add(attribute);
    }

    public void RemoveAttribute(string attributeName)
    {
        var attribute = _attributes.FirstOrDefault(a =>
            a.Name.Equals(attributeName, StringComparison.OrdinalIgnoreCase));

        if (attribute != null)
        {
            _attributes.Remove(attribute);
        }
    }

    public void UpdateAttribute(ProductAttr updatedAttribute)
    {
        var existingAttribute = _attributes.FirstOrDefault(a =>
            a.Name.Equals(updatedAttribute.Name, StringComparison.OrdinalIgnoreCase));

        if (existingAttribute == null)
        {
            throw new InvalidOperationException($"Attribute '{updatedAttribute.Name}' does not exist for this variant");
        }

        var index = _attributes.IndexOf(existingAttribute);
        _attributes[index] = updatedAttribute;
    }

    public ProductAttr GetAttribute(string attributeName)
    {
        return _attributes.FirstOrDefault(a =>
            a.Name.Equals(attributeName, StringComparison.OrdinalIgnoreCase));
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public bool IsInStock()
    {
        return StockQuantity > 0 && IsActive;
    }

    public bool CanFulfill(int requestedQuantity)
    {
        return IsActive && StockQuantity >= requestedQuantity;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidVariantNameException("Variant name cannot be empty or whitespace");
        }

        if (name.Length > 200)
        {
            throw new InvalidVariantNameException("Variant name cannot exceed 200 characters");
        }
    }

    private static void ValidateSKU(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new InvalidVariantSKUException("SKU cannot be empty or whitespace");
        }

        if (sku.Length > 50)
        {
            throw new InvalidVariantSKUException("SKU cannot exceed 50 characters");
        }

        // SKU should be alphanumeric with allowed special chars
        if (!System.Text.RegularExpressions.Regex.IsMatch(sku, @"^[a-zA-Z0-9\-_]+$"))
        {
            throw new InvalidVariantSKUException("SKU can only contain letters, numbers, hyphens, and underscores");
        }
    }

    private static void ValidatePrice(decimal price)
    {
        if (price < 0)
        {
            throw new VariantPriceException("Variant price cannot be negative");
        }

        if (price > 999999.99m)
        {
            throw new VariantPriceException("Variant price cannot exceed 999,999.99");
        }
    }

    private static void ValidateStockQuantity(int quantity)
    {
        if (quantity < 0)
        {
            throw new VariantQuantityException("Stock quantity cannot be negative");
        }

        if (quantity > 999999)
        {
            throw new VariantQuantityException("Stock quantity cannot exceed 999,999");
        }
    }
}
