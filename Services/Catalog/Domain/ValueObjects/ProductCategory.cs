using System;

namespace Catalog.Domain.ValueObjects
{
    /// <summary>
    ///     Represents the relationship data between Product and Category.
    ///     This is a simple data structure for the many-to-many relationship.
    /// </summary>
    public class ProductCategory
    {
        private ProductCategory() { } // For ORM/deserialization

        public ProductCategory(Guid categoryId, string categoryName)
        {
            CategoryId = categoryId;
            CategoryName = categoryName;
        }

        public Guid CategoryId { get; }
        public string CategoryName { get; }

        public override bool Equals(object obj)
        {
            if (obj is not ProductCategory other)
            {
                return false;
            }

            return CategoryId == other.CategoryId;
        }

        public override int GetHashCode()
        {
            return CategoryId.GetHashCode();
        }

        public override string ToString()
        {
            return CategoryName;
        }
    }
}
