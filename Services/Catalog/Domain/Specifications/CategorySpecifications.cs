using System;
using BuildingBlocks.Specifications;
using Catalog.Domain.Aggregates.Category;

namespace Catalog.Domain.Specifications
{
    public class CategoryQuerySpecification : BaseSpecification<Category>
    {
        public CategoryQuerySpecification(bool? rootCategoriesOnly, bool? activeOnly)
            : base(c => (rootCategoriesOnly != true || c.ParentCategoryId == null)
                        && (activeOnly != true || c.IsActive))
        {
            ApplyOrderBy(c => c.Name);
        }
    }
}
