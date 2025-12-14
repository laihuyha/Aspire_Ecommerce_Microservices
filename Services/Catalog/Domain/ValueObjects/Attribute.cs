using System;
using System.Collections.Generic;
using System.Globalization;
using BuildingBlocks.Entity;

namespace Catalog.Domain.ValueObjects;

/// <summary>
/// Domain exceptions for Attribute value object.
/// </summary>
public class InvalidAttributeNameDomainException : Exception
{
    public InvalidAttributeNameDomainException(string message) : base(message) { }
}

public class InvalidAttributeValueDomainException : Exception
{
    public InvalidAttributeValueDomainException(string message) : base(message) { }
}

/// <summary>
/// Represents a product attribute as a value object.
/// Attributes describe characteristics of products and variants.
/// </summary>
public class ProductAttr : ValueObject
{
    public string Name { get; private set; }
    public string Value { get; private set; }

    private ProductAttr() { } // For ORM/deserialization

    private ProductAttr(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public static ProductAttr Create(string name, string value)
    {
        ValidateName(name);
        ValidateValue(value);

        return new ProductAttr(
            name.Trim(),
            value.Trim()
        );
    }

    public static ProductAttr CreateColorAttribute(string color)
    {
        ValidateValue(color);
        return new ProductAttr("Color", color.Trim());
    }

    public static ProductAttr CreateSizeAttribute(string size)
    {
        ValidateValue(size);
        return new ProductAttr("Size", size.Trim());
    }

    public static ProductAttr CreateMaterialAttribute(string material)
    {
        ValidateValue(material);
        return new ProductAttr("Material", material.Trim());
    }

    public ProductAttr UpdateValue(string newValue)
    {
        ValidateValue(newValue);
        return new ProductAttr(Name, newValue.Trim());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name.ToLowerInvariant();
        yield return Value.ToLowerInvariant();
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidAttributeNameDomainException("Attribute name cannot be empty or whitespace");
        }

        if (name.Length > 100)
        {
            throw new InvalidAttributeNameDomainException("Attribute name cannot exceed 100 characters");
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z0-9\s\-_]+$"))
        {
            throw new InvalidAttributeNameDomainException("Attribute name contains invalid characters. Only letters, numbers, spaces, hyphens, and underscores are allowed");
        }
    }

    private static void ValidateValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidAttributeValueDomainException("Attribute value cannot be empty or whitespace");
        }

        if (value.Length > 500)
        {
            throw new InvalidAttributeValueDomainException("Attribute value cannot exceed 500 characters");
        }
    }

    public override string ToString()
    {
        return $"{Name}: {Value}";
    }
}
