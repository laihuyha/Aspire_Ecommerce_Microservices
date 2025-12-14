using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BuildingBlocks.Entity;
using BuildingBlocks.Errors;

namespace Catalog.Domain.ValueObjects
{
    /// <summary>
    ///     Represents a product attribute as a value object.
    ///     Attributes describe characteristics of products and variants.
    /// </summary>
    public class ProductAttr : ValueObject
    {
        private ProductAttr() { } // For ORM/deserialization

        private ProductAttr(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public string Value { get; }

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
                throw new DomainException("Attribute name cannot be empty or whitespace");
            }

            if (name.Length > 100)
            {
                throw new DomainException("Attribute name cannot exceed 100 characters");
            }

            if (!Regex.IsMatch(name, @"^[a-zA-Z0-9\s\-_]+$"))
            {
                throw new DomainException(
                    "Attribute name contains invalid characters. Only letters, numbers, spaces, hyphens, and underscores are allowed");
            }
        }

        private static void ValidateValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainException("Attribute value cannot be empty or whitespace");
            }

            if (value.Length > 500)
            {
                throw new DomainException("Attribute value cannot exceed 500 characters");
            }
        }

        public override string ToString()
        {
            return $"{Name}: {Value}";
        }
    }
}
