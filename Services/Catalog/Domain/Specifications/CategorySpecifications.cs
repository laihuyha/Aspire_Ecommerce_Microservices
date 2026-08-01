using BuildingBlocks.Specifications;
using Catalog.Domain.Aggregates.Category;

namespace Catalog.Domain.Specifications
{
    public class CategoryFilterSpecification : BaseSpecification<Category>
    {
        public CategoryFilterSpecification(bool? rootCategoriesOnly = null, bool? activeOnly = null)
        {
            if (rootCategoriesOnly == true && activeOnly == true)
                ApplyCriteria(c => c.ParentCategoryId == null && c.IsActive);
            else if (rootCategoriesOnly == true)
                ApplyCriteria(c => c.ParentCategoryId == null);
            else if (activeOnly == true)
                ApplyCriteria(c => c.IsActive);
            AddOrderBy(c => c.Name);
        }
    }
}
