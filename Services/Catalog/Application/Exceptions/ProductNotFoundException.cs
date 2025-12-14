using System;
using BuildingBlocks.Errors;

namespace Catalog.Application.Exceptions
{
    /// <summary>
    ///     Exception thrown when a product is not found.
    /// </summary>
    public class ProductNotFoundException : NotFoundException
    {
        public ProductNotFoundException(Guid productId)
            : base("Product", productId)
        {
        }
    }
}
