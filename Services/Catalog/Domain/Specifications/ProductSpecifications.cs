using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BuildingBlocks.Specifications;
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
            ApplyOrderBy(p => p.Name);
        }
    }

    public class ProductByCategorySpecification : BaseSpecification<Product>
    {
        public ProductByCategorySpecification(Guid categoryId) : base(p =>
            p.Categories.Any(c => c.CategoryId == categoryId))
        {
            ApplyOrderBy(p => p.Name);
        }
    }

    public class ProductInStockSpecification : BaseSpecification<Product>
    {
        public ProductInStockSpecification() : base(p => p.IsInStock())
        {
            ApplyOrderBy(p => p.Name);
        }
    }

    public class ProductByPriceRangeSpecification : BaseSpecification<Product>
    {
        public ProductByPriceRangeSpecification(decimal? minPrice, decimal? maxPrice) : base(BuildCriteria(minPrice, maxPrice))
        {
            ApplyOrderBy(p => p.GetEffectivePrice());
        }

        private static Expression<Func<Product, bool>> BuildCriteria(decimal? minPrice, decimal? maxPrice)
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
    }

    public class ProductSearchSpecification : BaseSpecification<Product>
    {
        public ProductSearchSpecification(string searchTerm, Guid? categoryId, decimal? minPrice, decimal? maxPrice,
            bool inStockOnly) : base(BuildCriteria(searchTerm, categoryId, minPrice, maxPrice, inStockOnly))
        {
            ApplyOrderBy(p => p.Name);
        }

        private static Expression<Func<Product, bool>> BuildCriteria(string searchTerm, Guid? categoryId, decimal? minPrice, decimal? maxPrice, bool inStockOnly)
        {
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

            return criteria;
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
            return Expression.Lambda<Func<Product, bool>>(
                Expression.AndAlso(left.Body, right.Body),
                left.Parameters[0]);
        }
    }

    public class ProductWithVariantsSpecification : BaseSpecification<Product>
    {
        public ProductWithVariantsSpecification() : base(p => p.Variants.Any())
        {
            ApplyOrderBy(p => p.Name);
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
