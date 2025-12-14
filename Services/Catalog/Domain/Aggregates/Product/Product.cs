using System;
using System.Collections.Generic;
using System.Linq;
using BuildingBlocks.Entity;
using BuildingBlocks.Errors;
using Catalog.Domain.Aggregates.Product.Events;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Aggregates.Product
{
    public class Product : BaseEntity<Guid>, IAggregateRoot
    {
        private readonly List<ProductAttr> _attributes = new();

        private readonly List<ProductCategory> _categories = new();
        private readonly List<Variant> _variants = new();

        private Product() { } // For ORM

        private Product(string name, string description, string imageUrl, decimal? basePrice)
        {
            Name = name;
            Description = description;
            ImageUrl = imageUrl;
            BasePrice = basePrice;
        }

        public string Name { get; private set; }
        public string Description { get; private set; }
        public string ImageUrl { get; private set; }

        // Product properties (base price - variants may override)
        public decimal? BasePrice { get; private set; }

        // Relationships
        public IReadOnlyCollection<ProductCategory> Categories => _categories.AsReadOnly();
        public IReadOnlyCollection<ProductAttr> Attributes => _attributes.AsReadOnly();
        public IReadOnlyCollection<Variant> Variants => _variants.AsReadOnly();

        public static Product Create(string name, string description = "", string imageUrl = "",
            decimal? basePrice = null)
        {
            ValidateName(name);
            ValidateDescription(description);
            ValidateImageUrl(imageUrl);

            Product product = new(
                name.Trim(),
                description?.Trim() ?? string.Empty,
                imageUrl?.Trim() ?? string.Empty,
                basePrice
            );

            // Raise domain event for product creation
            product.AddDomainEvent(new ProductCreatedDomainEvent(product.Id));

            return product;
        }

        public void UpdateBasicInfo(string name, string description, string imageUrl)
        {
            ValidateName(name);
            ValidateDescription(description);
            ValidateImageUrl(imageUrl);

            Name = name.Trim();
            Description = description?.Trim() ?? string.Empty;
            ImageUrl = imageUrl?.Trim() ?? string.Empty;
        }

        public void SetBasePrice(decimal? newPrice)
        {
            if (newPrice.HasValue)
            {
                ValidatePrice(newPrice.Value);
            }

            BasePrice = newPrice;
        }

        // Category relationship management (n-n with Category)
        public void AddCategory(Guid categoryId, string categoryName)
        {
            ValidateCategoryId(categoryId);
            ValidateCategoryName(categoryName);

            // Business rule: Cannot add duplicate categories
            if (_categories.Any(c => c.CategoryId == categoryId))
            {
                return; // Silently ignore duplicate (idempotent operation)
            }

            _categories.Add(new ProductCategory(categoryId, categoryName.Trim()));
        }

        public void RemoveCategory(Guid categoryId)
        {
            ProductCategory categoryToRemove = _categories.FirstOrDefault(c => c.CategoryId == categoryId);
            if (categoryToRemove != null)
            {
                _categories.Remove(categoryToRemove);
            }
        }

        public void UpdateCategories(IEnumerable<ProductCategory> newCategories)
        {
            _categories.Clear();
            foreach (ProductCategory category in newCategories ?? Enumerable.Empty<ProductCategory>())
            {
                AddCategory(category.CategoryId, category.CategoryName);
            }
        }

        // Attribute relationship management (1-n with ProductAttr)
        public void AddAttribute(ProductAttr attribute)
        {
            // Business rule: Cannot have duplicate attribute names
            if (_attributes.Any(a => a.Name.Equals(attribute.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new DomainException($"Attribute '{attribute.Name}' already exists for this product");
            }

            _attributes.Add(attribute);
        }

        public void RemoveAttribute(string attributeName)
        {
            ProductAttr attribute = _attributes.FirstOrDefault(a =>
                a.Name.Equals(attributeName, StringComparison.OrdinalIgnoreCase));

            if (attribute != null)
            {
                _attributes.Remove(attribute);
            }
        }

        public void UpdateAttribute(ProductAttr updatedAttribute)
        {
            ProductAttr existingAttribute = _attributes.FirstOrDefault(a =>
                a.Name.Equals(updatedAttribute.Name, StringComparison.OrdinalIgnoreCase));

            if (existingAttribute == null)
            {
                throw new DomainException($"Attribute '{updatedAttribute.Name}' does not exist for this product");
            }

            int index = _attributes.IndexOf(existingAttribute);
            _attributes[index] = updatedAttribute;
        }

        public ProductAttr GetAttribute(string attributeName)
        {
            return _attributes.FirstOrDefault(a =>
                a.Name.Equals(attributeName, StringComparison.OrdinalIgnoreCase));
        }

        // Variant relationship management (1-n with Variant)
        public void AddVariant(Variant variant)
        {
            // Business rule: Cannot have duplicate SKUs
            if (_variants.Any(v => v.SKU.Equals(variant.SKU, StringComparison.OrdinalIgnoreCase)))
            {
                throw new DomainException($"Variant with SKU '{variant.SKU}' already exists for this product");
            }

            _variants.Add(variant);
        }

        public void RemoveVariant(Guid variantId)
        {
            Variant variantToRemove = _variants.FirstOrDefault(v => v.Id == variantId);
            if (variantToRemove != null)
            {
                _variants.Remove(variantToRemove);
            }
        }

        public Variant GetVariant(Guid variantId)
        {
            return _variants.FirstOrDefault(v => v.Id == variantId);
        }

        public Variant GetVariantBySKU(string sku)
        {
            return _variants.FirstOrDefault(v =>
                v.SKU.Equals(sku, StringComparison.OrdinalIgnoreCase));
        }

        // Business logic methods
        public decimal GetEffectivePrice()
        {
            // If product has variants, return the lowest variant price
            // Otherwise return base price
            if (_variants.Any(v => v.IsActive && v.StockQuantity > 0))
            {
                return _variants.Where(v => v.IsActive && v.StockQuantity > 0)
                    .Min(v => v.Price);
            }

            return BasePrice ?? 0;
        }

        public int GetTotalStockQuantity()
        {
            return _variants.Where(v => v.IsActive).Sum(v => v.StockQuantity);
        }

        public bool IsInStock()
        {
            return GetTotalStockQuantity() > 0;
        }

        public IEnumerable<Variant> GetAvailableVariants()
        {
            return _variants.Where(v => v.IsActive && v.StockQuantity > 0);
        }

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("Product name cannot be empty or whitespace");
            }

            if (name.Length > 200)
            {
                throw new DomainException("Product name cannot exceed 200 characters");
            }
        }

        private static void ValidateDescription(string description)
        {
            if (description?.Length > 1000)
            {
                throw new DomainException("Product description cannot exceed 1000 characters");
            }
        }

        private static void ValidateImageUrl(string imageUrl)
        {
            if (imageUrl?.Length > 500)
            {
                throw new DomainException("Image URL cannot exceed 500 characters");
            }
        }

        private static void ValidatePrice(decimal price)
        {
            if (price < 0)
            {
                throw new DomainException("Product price cannot be negative");
            }

            if (price > 999999.99m)
            {
                throw new DomainException("Product price cannot exceed 999,999.99");
            }
        }

        private static void ValidateCategoryId(Guid categoryId)
        {
            if (categoryId == Guid.Empty)
            {
                throw new DomainException("Category ID cannot be empty");
            }
        }

        private static void ValidateCategoryName(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                throw new DomainException("Category name cannot be empty or whitespace");
            }

            if (categoryName.Length > 100)
            {
                throw new DomainException("Category name cannot exceed 100 characters");
            }
        }
    }
}
