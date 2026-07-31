using HRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRestaurant.Data.Configurations;

public sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> entity)
    {
        entity.ToTable("InventoryItems");
        entity.Property(x => x.CurrentQuantity).HasPrecision(18, 3);
        entity.Property(x => x.MinimumQuantity).HasPrecision(18, 3);
        entity.Property(x => x.PurchasePrice).HasPrecision(18, 2);
        entity.Property(x => x.ExpirationDate).HasColumnType("date");
        entity.Property(x => x.BatchNumber).HasMaxLength(100);
        entity.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
        entity.HasIndex(x => new { x.RestaurantId, x.IsDeleted, x.IsActive });
        entity.HasIndex(x => new { x.BranchId, x.IsDeleted, x.IsActive });
        entity.HasIndex(x => new { x.IngredientId, x.BranchId, x.BatchNumber });
        entity.HasIndex(x => new { x.ExpirationDate, x.IsDeleted, x.IsActive });
        entity.HasIndex(x => new { x.BranchId, x.CurrentQuantity, x.MinimumQuantity });
        entity.HasOne(x => x.Restaurant).WithMany(x => x.InventoryItems)
            .HasForeignKey(x => x.RestaurantId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Branch).WithMany(x => x.InventoryItems)
            .HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Ingredient).WithMany(x => x.InventoryItems)
            .HasForeignKey(x => x.IngredientId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Supplier).WithMany(x => x.InventoryItems)
            .HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.SetNull);
    }
}
