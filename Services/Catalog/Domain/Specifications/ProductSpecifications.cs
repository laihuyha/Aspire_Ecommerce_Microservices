/*
 * SPECIFICATION PATTERN - Product Specifications
 *
 * CURRENT STATE ANALYSIS:
 * ✅ Tốt: Specification pattern separated business logic from queries
 * ✅ Tốt: Reusable criteria definitions
 * ⚠️ NGUYÊN: ProductSearchSpecification quá phức tạp với CombineCriteria
 * ⚠️ NGUYÊN: Expression tree manipulation khó debug và maintain
 *
 * PROBLEMS WITH CURRENT COMBINE CRITERIA:
 * - Expression.AndAlso phức tạp không cần thiết
 * - Hard to debug runtime errors
 * - Difficult to test individual conditions
 * - Performance overhead khi build expressions
 *
 * BETTER APPROACH - SIMPLIFY TO LINQ PREDICATES:
 *
 * RECOMMENDED REFACTOR:
 * public class ProductSearchSpecification : BaseSpecification<Product>
 * {
 *     public ProductSearchSpecification(...) : base()
 *     {
 *         var query = _session.Query<Product>();
 *
 *         if (!string.IsNullOrEmpty(searchTerm))
 *             query = query.Where(p =>
 *                 p.Name.Contains(searchTerm) ||
 *                 p.Description.Contains(searchTerm));
 *
 *         if (categoryId.HasValue)
 *             query = query.Where(p =>
 *                 p.Categories.Any(c => c.CategoryId == categoryId));
 *
 *         // ... more filters
 *
 *         // Apply to specification
 *         ApplyCriteria(BuildCriteria(...));
 *     }
 * }
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Catalog.Domain.Aggregates.Product;

namespace Catalog.Domain.Specifications
{
    /// <summary>
    ///     Specifications for Product queries.
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
        public ProductByCategorySpecification(Guid categoryId) : base(p =>
            p.Categories.Any(c => c.CategoryId == categoryId))
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
        public ProductSearchSpecification(string searchTerm, Guid? categoryId, decimal? minPrice, decimal? maxPrice,
            bool inStockOnly)
        {
            // Build complex search criteria
            Expression<Func<Product, bool>> criteria = p => true;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Expression<Func<Product, bool>> searchCriteria = BuildSearchCriteria(searchTerm);
                criteria = CombineCriteria(criteria, searchCriteria);
            }

            if (categoryId.HasValue)
            {
                Expression<Func<Product, bool>> categoryCriteria = BuildCategoryCriteria(categoryId.Value);
                criteria = CombineCriteria(criteria, categoryCriteria);
            }

            if (minPrice.HasValue || maxPrice.HasValue)
            {
                Expression<Func<Product, bool>> priceCriteria = BuildPriceCriteria(minPrice, maxPrice);
                criteria = CombineCriteria(criteria, priceCriteria);
            }

            if (inStockOnly)
            {
                Expression<Func<Product, bool>> stockCriteria = BuildStockCriteria();
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

            if (minPrice.HasValue)
            {
                return p => p.GetEffectivePrice() >= minPrice.Value;
            }

            if (maxPrice.HasValue)
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
            // FIXME: Replace with LINQ predicates for maintainability
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

    public class ProductWithSkusSpecification: BaseSpecification<Product>
    {
        public ProductWithSkusSpecification(IEnumerable<string> skus) : base(p =>
            p.Variants.Any(v => skus.Contains(v.SKU)))
        {
        }
    }
}
