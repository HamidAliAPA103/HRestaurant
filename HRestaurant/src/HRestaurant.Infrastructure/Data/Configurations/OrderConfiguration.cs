using HRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRestaurant.Data.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> entity)
    {
        entity.ToTable("Orders");
        entity.Property(x => x.OrderNumber).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Subtotal).HasPrecision(18, 2);
        entity.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        entity.Property(x => x.OrderDiscountPercentage).HasPrecision(5, 2);
        entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
        entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
        entity.Property(x => x.PaidAmount).HasPrecision(18, 2);
        entity.Property(x => x.Notes).HasMaxLength(500);
        entity.Property(x => x.CancelReason).HasMaxLength(300);
        entity.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
        entity.HasIndex(x => new { x.RestaurantId, x.OrderNumber }).IsUnique();
        entity.HasIndex(x => new { x.BranchId, x.Status, x.CreatAt });
        entity.HasIndex(x => new { x.WaiterId, x.Status, x.CreatAt });
        entity.HasOne(x => x.Restaurant).WithMany(x => x.Orders)
            .HasForeignKey(x => x.RestaurantId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Branch).WithMany(x => x.Orders)
            .HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Table).WithMany(x => x.Orders)
            .HasForeignKey(x => x.TableId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Customer).WithMany(x => x.Orders)
            .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Waiter).WithMany(x => x.WaiterOrders)
            .HasForeignKey(x => x.WaiterId).OnDelete(DeleteBehavior.Restrict);
    }
}
