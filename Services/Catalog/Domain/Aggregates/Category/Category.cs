using System;
using System.Collections.Generic;
using BuildingBlocks.Entity;

namespace Catalog.Domain.Aggregates.Category;

// Domain-specific exceptions for better error handling
public class InvalidCategoryNameException : Exception
{
    public InvalidCategoryNameException(string message) : base(message) { }
}

public class InvalidCategoryDescriptionException : Exception
{
    public InvalidCategoryDescriptionException(string message) : base(message) { }
}

public class Category : BaseEntity<Guid>, IAggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();
    public bool IsActive { get; private set; }

    private readonly List<Category> _subCategories = new();

    private Category() { } // For ORM

    private Category(string name, string description, Guid? parentCategoryId)
    {
        Name = name;
        Description = description;
        ParentCategoryId = parentCategoryId;
        IsActive = true;
    }

    public static Category CreateRootCategory(string name, string description = "")
    {
        ValidateName(name);
        ValidateDescription(description);

        var category = new Category(name.Trim(), description?.Trim() ?? string.Empty, null);

        // Raise domain event for root category creation
        // category.AddDomainEvent(new CategoryCreatedDomainEvent(category.Id, category.Name));

        return category;
    }

    public static Category CreateSubCategory(string name, Guid parentCategoryId, string description = "")
    {
        ValidateName(name);
        ValidateDescription(description);

        var category = new Category(name.Trim(), description?.Trim() ?? string.Empty, parentCategoryId);

        // Raise domain event for sub category creation
        // category.AddDomainEvent(new SubCategoryCreatedDomainEvent(category.Id, category.Name, parentCategoryId));

        return category;
    }

    public void UpdateDetails(string name, string description)
    {
        ValidateName(name);
        ValidateDescription(description);

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
    }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            // Add domain event
            // AddDomainEvent(new CategoryActivatedDomainEvent(Id, Name));
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            // Add domain event
            // AddDomainEvent(new CategoryDeactivatedDomainEvent(Id, Name));
        }
    }

    public Category AddSubCategory(string subCategoryName, string description = "")
    {
        if (string.IsNullOrWhiteSpace(subCategoryName))
        {
            throw new InvalidCategoryNameException("Sub category name cannot be empty");
        }

        var subCategory = CreateSubCategory(subCategoryName, Id, description);
        _subCategories.Add(subCategory);

        return subCategory;
    }

    public void ChangeParent(Guid? newParentId)
    {
        // Business rule: Cannot make a category its own parent
        if (newParentId.HasValue && newParentId.Value == Id)
        {
            throw new InvalidOperationException("Category cannot be its own parent");
        }

        ParentCategoryId = newParentId;
    }

    public bool IsRootCategory()
    {
        return !ParentCategoryId.HasValue;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidCategoryNameException("Category name cannot be empty or whitespace");
        }

        if (name.Length > 100)
        {
            throw new InvalidCategoryNameException("Category name cannot exceed 100 characters");
        }
    }

    private static void ValidateDescription(string description)
    {
        if (description?.Length > 500)
        {
            throw new InvalidCategoryDescriptionException("Category description cannot exceed 500 characters");
        }
    }
}
