using System;
using System.Linq;
using System.Linq.Expressions;
using Catalog.Domain.Aggregates.Product;

namespace Catalog.Domain.Specifications;

/// <summary>
/// Specifications for Product queries.
/// </summary>

public class ProductByIdSpecification : BaseSpecification<Product>
{
    public ProductByIdSpecification(Guid productId) : base(p => p.Id == productId)
    {
    }
}

public class ProductByNameSpecification : BaseSpecification<Product>
{
    public ProductByNameSpecification(string name) : base(p => p.Name.Contains(name))
    {
        AddOrderBy(p => p.Name);
    }
}

public class ProductByCategorySpecification : BaseSpecification<Product>
{
    public ProductByCategorySpecification(Guid categoryId) : base(p => p.Categories.Any(c => c.CategoryId == categoryId))
    {
        AddOrderBy(p => p.Name);
    }
}

public class ProductInStockSpecification : BaseSpecification<Product>
{
    public ProductInStockSpecification() : base(p => p.IsInStock())
    {
        AddOrderBy(p => p.Name);
    }
}

public class ProductByPriceRangeSpecification : BaseSpecification<Product>
{
    public ProductByPriceRangeSpecification(decimal? minPrice, decimal? maxPrice)
    {
        // Complex criteria for price filtering
        Expression<Func<Product, bool>> criteria = p => true; // Default to all

        if (minPrice.HasValue && maxPrice.HasValue)
        {
            criteria = p => p.GetEffectivePrice() >= minPrice.Value && p.GetEffectivePrice() <= maxPrice.Value;
        }
        else if (minPrice.HasValue)
        {
            criteria = p => p.GetEffectivePrice() >= minPrice.Value;
        }
        else if (maxPrice.HasValue)
        {
            criteria = p => p.GetEffectivePrice() <= maxPrice.Value;
        }

        // Apply the criteria
        ApplyCriteria(criteria);
        AddOrderBy(p => p.GetEffectivePrice());
    }
}

public class ProductSearchSpecification : BaseSpecification<Product>
{
    public ProductSearchSpecification(string searchTerm, Guid? categoryId, decimal? minPrice, decimal? maxPrice, bool inStockOnly)
    {
        // Build complex search criteria
        Expression<Func<Product, bool>> criteria = p => true;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchCriteria = BuildSearchCriteria(searchTerm);
            criteria = CombineCriteria(criteria, searchCriteria);
        }

        if (categoryId.HasValue)
        {
            var categoryCriteria = BuildCategoryCriteria(categoryId.Value);
            criteria = CombineCriteria(criteria, categoryCriteria);
        }

        if (minPrice.HasValue || maxPrice.HasValue)
        {
            var priceCriteria = BuildPriceCriteria(minPrice, maxPrice);
            criteria = CombineCriteria(criteria, priceCriteria);
        }

        if (inStockOnly)
        {
            var stockCriteria = BuildStockCriteria();
            criteria = CombineCriteria(criteria, stockCriteria);
        }

        ApplyCriteria(criteria);
        AddOrderBy(p => p.Name);
    }

    private static Expression<Func<Product, bool>> BuildSearchCriteria(string searchTerm)
    {
        return p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm);
    }

    private static Expression<Func<Product, bool>> BuildCategoryCriteria(Guid categoryId)
    {
        return p => p.Categories.Any(c => c.CategoryId == categoryId);
    }

    private static Expression<Func<Product, bool>> BuildPriceCriteria(decimal? minPrice, decimal? maxPrice)
    {
        if (minPrice.HasValue && maxPrice.HasValue)
        {
            return p => p.GetEffectivePrice() >= minPrice.Value && p.GetEffectivePrice() <= maxPrice.Value;
        }
        else if (minPrice.HasValue)
        {
            return p => p.GetEffectivePrice() >= minPrice.Value;
        }
        else if (maxPrice.HasValue)
        {
            return p => p.GetEffectivePrice() <= maxPrice.Value;
        }

        return p => true;
    }

    private static Expression<Func<Product, bool>> BuildStockCriteria()
    {
        return p => p.IsInStock();
    }

    private static Expression<Func<Product, bool>> CombineCriteria(
        Expression<Func<Product, bool>> left,
        Expression<Func<Product, bool>> right)
    {
        // Combine two expressions with AND
        return Expression.Lambda<Func<Product, bool>>(
            Expression.AndAlso(left.Body, right.Body),
            left.Parameters[0]);
    }
}

public class ProductWithVariantsSpecification : BaseSpecification<Product>
{
    public ProductWithVariantsSpecification() : base(p => p.Variants.Any())
    {
        AddOrderBy(p => p.Name);
    }
}
