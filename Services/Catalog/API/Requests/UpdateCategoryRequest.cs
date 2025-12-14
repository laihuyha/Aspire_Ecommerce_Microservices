namespace Catalog.Api.Requests;

/// <summary>
/// Request DTO for updating an existing category.
/// </summary>
public record UpdateCategoryRequest(
    string Name,
    string Description);
