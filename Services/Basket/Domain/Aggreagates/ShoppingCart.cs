using System;
using System.Collections.Generic;
using System.Linq;
using Basket.Domain.Aggregates.Events;
using Basket.Domain.Entities;
using BuildingBlocks.Entity;
using BuildingBlocks.Errors;

namespace Basket.Domain.Aggregates
{
    public class ShoppingCart : BaseEntity<Guid>, IAggregateRoot
    {
        private readonly List<ShoppingCartItem> _items = new();

        public ShoppingCart() { }

        public Guid UserId { get; set; }
        public IReadOnlyCollection<ShoppingCartItem> Items => _items;
        public decimal Discount { get; private set; }
        public decimal Coupon { get; private set; }
        public decimal SubTotal => _items.Sum(i => i.SubTotal);
        public decimal Total => SubTotal - Discount - Coupon;

        public static ShoppingCart Create(Guid userId, IEnumerable<ShoppingCartItem> items)
        {
            ValidateUserId(userId);

            List<ShoppingCartItem> itemList = items?.ToList();
            ValidateItems(itemList);

            var cart = new ShoppingCart { Id = Guid.NewGuid(), UserId = userId };
            foreach (var item in itemList)
                cart.AddItem(item.ProductId, item.ImageUrl, item.ProductName, item.UnitPrice, item.Quantity);

            cart.AddDomainEvent(new ShoppingCartCreatedEvent(cart.Id, userId));
            return cart;
        }

        public void SetDiscount(decimal discount)
        {
            if (discount < 0)
                throw new DomainException("Discount cannot be negative.");
            Discount = discount;
        }

        public void SetCoupon(decimal coupon)
        {
            if (coupon < 0)
                throw new DomainException("Coupon cannot be negative.");
            Coupon = coupon;
        }

        public void AddItem(Guid productId, string imageUrl, string productName, decimal unitPrice, int quantity)
        {
            ShoppingCartItem existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
                existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            else
                _items.Add(ShoppingCartItem.Create(productId, productName, imageUrl, unitPrice, quantity));

            AddDomainEvent(new ShoppingCartItemAddedEvent(Id, productId, quantity));
        }

        public void AddItems(IEnumerable<ShoppingCartItem> items)
        {
            List<ShoppingCartItem> itemList = items?.ToList();
            ValidateItems(itemList);
            foreach (var item in itemList)
                AddItem(item.ProductId, item.ImageUrl, item.ProductName, item.UnitPrice, item.Quantity);
        }

        public void UpdateItem(Guid productId, int quantity)
        {
            ShoppingCartItem existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                if (quantity <= 0)
                {
                    _items.Remove(existingItem);
                    AddDomainEvent(new ShoppingCartItemRemovedEvent(Id, productId));
                }
                else
                {
                    existingItem.UpdateQuantity(quantity);
                    AddDomainEvent(new ShoppingCartItemUpdatedEvent(Id, productId, quantity));
                }
            }
        }

        public void RemoveItem(Guid productId)
        {
            ShoppingCartItem item = _items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                _items.Remove(item);
                AddDomainEvent(new ShoppingCartItemRemovedEvent(Id, productId));
            }
        }

        public void Clear()
        {
            _items.Clear();
            Discount = 0;
            Coupon = 0;
            AddDomainEvent(new ShoppingCartClearedEvent(Id));
        }

        private static void ValidateUserId(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new DomainException("User ID for shopping cart cannot be empty.");
        }

        private static void ValidateItems(List<ShoppingCartItem> items)
        {
            if (items == null || items.Count == 0)
                throw new DomainException("Shopping cart must contain at least one item.");

            ShoppingCartItem invalid = items.FirstOrDefault(i => i.Quantity <= 0);
            if (invalid != null)
                throw new DomainException($"Item {invalid.ProductName} quantity must be greater than zero.");
        }
    }
}
