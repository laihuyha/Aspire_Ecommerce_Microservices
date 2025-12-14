using System;
using BuildingBlocks.Errors;

namespace Catalog.Application.Exceptions
{
    /// <summary>
    ///     Exception thrown when a category is not found.
    /// </summary>
    public class CategoryNotFoundException : NotFoundException
    {
        public CategoryNotFoundException(Guid categoryId)
            : base("Category", categoryId)
        {
        }
    }
}
