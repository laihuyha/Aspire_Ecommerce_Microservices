using System;

namespace Catalog.Application.Features.Category.Exceptions;

/// <summary>
/// Exception thrown when a category is not found.
/// </summary>
public class CategoryNotFoundException : Exception
{
    public CategoryNotFoundException(Guid categoryId)
        : base($"Category with ID '{categoryId}' was not found.")
    {
        CategoryId = categoryId;
    }

    public Guid CategoryId { get; }
}
