namespace Catalog.Api.Requests;

/// <summary>
/// Request DTO for getting products with pagination and filtering.
/// </summary>
public record GetProductsRequest(
    int PageNumber = 1,
    int PageSize = 10,
    string Category = null);
