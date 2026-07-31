using HRestaurant.Infrastructure.Identity;
using HRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRestaurant.Data.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> entity)
    {
        entity.ToTable("Payments");
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.TransactionReference).HasMaxLength(150);
        entity.Property(x => x.FailureReason).HasMaxLength(300);
        entity.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
        entity.HasIndex(x => new { x.OrderId, x.PaymentStatus, x.CreatAt });
        entity.HasIndex(x => new { x.RestaurantId, x.BranchId, x.PaidAt });
        entity.HasIndex(x => x.TransactionReference).IsUnique()
            .HasFilter("[TransactionReference] IS NOT NULL");
        entity.HasOne(x => x.Order).WithMany(x => x.Payments)
            .HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Restaurant).WithMany(x => x.Payments)
            .HasForeignKey(x => x.RestaurantId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Branch).WithMany(x => x.Payments)
            .HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<AppUser>().WithMany().HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
