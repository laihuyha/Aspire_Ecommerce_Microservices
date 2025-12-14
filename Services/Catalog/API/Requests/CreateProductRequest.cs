using System;
using System.Collections.Generic;

namespace Catalog.Api.Requests
{
    /// <summary>
    ///     Request DTO for creating a new product.
    /// </summary>
    public record CreateProductRequest(
        string Name,
        List<Guid> Categories,
        string Description,
        string ImageUrl,
        decimal? BasePrice);
}
