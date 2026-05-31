using System;
using BuildingBlocks.Entity;
using BuildingBlocks.Errors;

namespace Basket.Domain.Entities
{
    public record CartItemVariant(Guid VariantId, string VariantName, string SKU);

    public class ShoppingCartItem : BaseEntity<Guid>
    {
        private ShoppingCartItem() { }

        private ShoppingCartItem(
            Guid productId,
            string productName,
            string imageUrl,
            decimal unitPrice,
            int quantity,
            CartItemVariant variant)
        {
            Id = Guid.NewGuid();
            ProductId = productId;
            ProductName = productName;
            ImageUrl = imageUrl ?? string.Empty;
            UnitPrice = unitPrice;
            Quantity = quantity;
            VariantId = variant?.VariantId;
            VariantName = variant?.VariantName ?? string.Empty;
            SKU = variant?.SKU ?? string.Empty;
        }

        public Guid ProductId { get; private set; }
        public string ProductName { get; private set; }
        public string ImageUrl { get; private set; }

        // Price snapshot at the time of adding — not affected by later price changes
        public decimal UnitPrice { get; private set; }
        public int Quantity { get; private set; }
        public decimal SubTotal => UnitPrice * Quantity;

        // Variant is optional — null means buyer chose no specific variant
        public Guid? VariantId { get; private set; }
        public string VariantName { get; private set; }
        public string SKU { get; private set; }

        public static ShoppingCartItem Create(
            Guid productId,
            string productName,
            string imageUrl,
            decimal unitPrice,
            int quantity,
            CartItemVariant variant = null)
        {
            ValidateProductId(productId);
            ValidateProductName(productName);
            ValidateUnitPrice(unitPrice);
            ValidateQuantity(quantity);

            return new ShoppingCartItem(productId, productName, imageUrl, unitPrice, quantity, variant);
        }

        public void UpdateQuantity(int newQuantity)
        {
            ValidateQuantity(newQuantity);
            Quantity = newQuantity;
        }

        public void ChangeVariant(CartItemVariant variant, decimal unitPrice)
        {
            if (variant is null)
                throw new DomainException("Variant cannot be null");

            if (variant.VariantId == Guid.Empty)
                throw new DomainException("Variant ID cannot be empty");

            ValidateUnitPrice(unitPrice);

            VariantId = variant.VariantId;
            VariantName = variant.VariantName ?? string.Empty;
            SKU = variant.SKU ?? string.Empty;
            UnitPrice = unitPrice;
        }

        public void UpdatePrice(decimal newUnitPrice)
        {
            ValidateUnitPrice(newUnitPrice);
            UnitPrice = newUnitPrice;
        }

        private static void ValidateProductId(Guid productId)
        {
            if (productId == Guid.Empty)
                throw new DomainException("Product ID cannot be empty");
        }

        private static void ValidateProductName(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new DomainException("Product name cannot be empty");

            if (productName.Length > 200)
                throw new DomainException("Product name cannot exceed 200 characters");
        }

        private static void ValidateUnitPrice(decimal unitPrice)
        {
            if (unitPrice < 0)
                throw new DomainException("Unit price cannot be negative");

            if (unitPrice > 999999.99m)
                throw new DomainException("Unit price cannot exceed 999,999.99");
        }

        private static void ValidateQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new DomainException("Quantity must be greater than zero");

            if (quantity > 9999)
                throw new DomainException("Quantity cannot exceed 9,999 per item");
        }
    }
}
