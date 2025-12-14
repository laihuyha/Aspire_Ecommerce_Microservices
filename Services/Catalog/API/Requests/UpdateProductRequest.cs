namespace Catalog.Api.Requests
{
    /// <summary>
    ///     Request DTO for updating an existing product.
    /// </summary>
    public record UpdateProductRequest(
        string Name,
        string Description,
        string ImageUrl,
        decimal? BasePrice);
}
