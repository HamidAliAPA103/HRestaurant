using HRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRestaurant.Data.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> entity)
    {
        entity.ToTable("OrderItems");
        entity.Property(x => x.MenuItemName).HasMaxLength(150).IsRequired();
        entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
        entity.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        entity.Property(x => x.TotalPrice).HasPrecision(18, 2);
        entity.Property(x => x.KitchenNote).HasMaxLength(300);
        entity.HasIndex(x => new { x.OrderId, x.IsDeleted });
        entity.HasIndex(x => x.MenuItemId);
        entity.HasOne(x => x.Order).WithMany(x => x.Items)
            .HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(x => x.MenuItem).WithMany(x => x.OrderItems)
            .HasForeignKey(x => x.MenuItemId).OnDelete(DeleteBehavior.Restrict);
    }
}
