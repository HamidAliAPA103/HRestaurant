using HRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRestaurant.Data.Configurations;

public sealed class LoyaltyAccountConfiguration : IEntityTypeConfiguration<LoyaltyAccount>
{
    public void Configure(EntityTypeBuilder<LoyaltyAccount> entity)
    {
        entity.ToTable("LoyaltyAccounts");
        entity.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
        entity.HasIndex(x => x.CustomerId).IsUnique();
        entity.HasOne(x => x.Customer).WithOne(x => x.LoyaltyAccount)
            .HasForeignKey<LoyaltyAccount>(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class LoyaltyTransactionConfiguration
    : IEntityTypeConfiguration<LoyaltyTransaction>
{
    public void Configure(EntityTypeBuilder<LoyaltyTransaction> entity)
    {
        entity.ToTable("LoyaltyTransactions");
        entity.Property(x => x.Description).HasMaxLength(300).IsRequired();
        entity.HasIndex(x => new { x.LoyaltyAccountId, x.CreatAt });
        entity.HasIndex(x => new { x.OrderId, x.Type }).IsUnique()
            .HasFilter("[OrderId] IS NOT NULL AND [Type] IN (0, 1)");
        entity.HasOne(x => x.LoyaltyAccount).WithMany(x => x.Transactions)
            .HasForeignKey(x => x.LoyaltyAccountId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Order).WithMany(x => x.LoyaltyTransactions)
            .HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);
    }
}
