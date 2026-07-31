using HRestaurant.Infrastructure.Identity;
using HRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRestaurant.Data.Configurations;

public sealed class StockTransactionConfiguration
    : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> entity)
    {
        entity.ToTable("StockTransactions");
        entity.Property(x => x.Quantity).HasPrecision(18, 3);
        entity.Property(x => x.PreviousQuantity).HasPrecision(18, 3);
        entity.Property(x => x.NewQuantity).HasPrecision(18, 3);
        entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
        entity.Property(x => x.Reason).HasMaxLength(300).IsRequired();
        entity.Property(x => x.ReferenceNumber).HasMaxLength(100);
        entity.HasIndex(x => new { x.InventoryItemId, x.CreatAt });
        entity.HasIndex(x => new { x.CreatedByUserId, x.CreatAt });
        entity.HasIndex(x => new
            { x.InventoryItemId, x.ReferenceNumber, x.TransactionType })
            .IsUnique()
            .HasFilter("[TransactionType] = 4 AND [ReferenceNumber] IS NOT NULL AND [IsDeleted] = 0");
        entity.HasOne(x => x.InventoryItem).WithMany(x => x.Transactions)
            .HasForeignKey(x => x.InventoryItemId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<AppUser>().WithMany()
            .HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
