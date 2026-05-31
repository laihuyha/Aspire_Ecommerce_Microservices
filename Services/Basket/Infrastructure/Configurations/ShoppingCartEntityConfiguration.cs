using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Basket.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Configurations
{
    public class ShoppingCartEntityConfiguration : IEntityTypeConfiguration<ShoppingCart>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ShoppingCart> builder)
        {
            builder.ToTable("ShoppingCarts");
            builder.HasKey(x => x.Id);
            builder.OwnsMany(x => x.Items, a =>
            {
                a.WithOwner().HasForeignKey("ShoppingCartId");
                a.Property<int>("Id");
                a.HasKey("Id");
            });
            builder.Property(x => x.UserId).IsRequired();
        }
    }
}