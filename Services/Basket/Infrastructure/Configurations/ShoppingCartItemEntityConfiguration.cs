using System;
using Basket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Basket.Infrastructure.Configurations;

public class ShoppingCartItemEntityConfiguration : IEntityTypeConfiguration<ShoppingCartItem>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ShoppingCartItem> builder)
    {
        builder.ToTable("ShoppingCartItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2).IsRequired();
        builder.Ignore(x => x.SubTotal);
    }
}
