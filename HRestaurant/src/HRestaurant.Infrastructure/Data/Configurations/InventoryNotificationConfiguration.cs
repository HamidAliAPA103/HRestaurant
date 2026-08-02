using HRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRestaurant.Data.Configurations;

public sealed class InventoryNotificationConfiguration
    : IEntityTypeConfiguration<InventoryNotification>
{
    public void Configure(EntityTypeBuilder<InventoryNotification> entity)
    {
        entity.ToTable("InventoryNotifications");
        entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
        entity.Property(x => x.Message).HasMaxLength(500).IsRequired();
        entity.Property(x => x.TargetUrl).HasMaxLength(300);
        entity.HasIndex(x => new { x.InventoryItemId, x.Type })
            .IsUnique()
            .HasFilter("[InventoryItemId] IS NOT NULL AND [IsDeleted] = 0 AND [IsRead] = 0 AND [IsResolved] = 0");
        entity.HasIndex(x => new { x.RestaurantId, x.IsRead, x.IsResolved, x.CreatAt });
        entity.HasIndex(x => new { x.BranchId, x.IsRead, x.IsResolved, x.CreatAt });
        entity.HasOne(x => x.InventoryItem).WithMany(x => x.Notifications)
            .HasForeignKey(x => x.InventoryItemId).IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Restaurant).WithMany(x => x.InventoryNotifications)
            .HasForeignKey(x => x.RestaurantId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Branch).WithMany(x => x.InventoryNotifications)
            .HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
    }
}
