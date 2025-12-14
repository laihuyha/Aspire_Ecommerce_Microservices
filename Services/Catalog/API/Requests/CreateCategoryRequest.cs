using System;

namespace Catalog.Api.Requests
{
    /// <summary>
    ///     Request DTO for creating a new category.
    /// </summary>
    public record CreateCategoryRequest(
        string Name,
        string Description,
        Guid? ParentCategoryId);
}
