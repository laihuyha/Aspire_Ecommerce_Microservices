using System;
using BuildingBlocks.Entity;

namespace Catalog.Domain.Aggregates.Category.Events
{
    /// <summary>
    ///     Domain event raised when a category is created.
    /// </summary>
    public class CategoryCreatedDomainEvent : DomainEvent
    {
        public CategoryCreatedDomainEvent(Guid categoryId, string categoryName, bool isRootCategory)
        {
            CategoryId = categoryId;
            CategoryName = categoryName;
            IsRootCategory = isRootCategory;
        }

        public Guid CategoryId { get; }
        public string CategoryName { get; }
        public bool IsRootCategory { get; }
    }
}
