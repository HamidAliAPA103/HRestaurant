using HRestaurant.Infrastructure.Identity;
using HRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRestaurant.Data.Configurations;

public sealed class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> entity)
    {
        entity.ToTable("Refunds");
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.Reason).HasMaxLength(300).IsRequired();
        entity.HasIndex(x => new { x.PaymentId, x.RefundedAt });
        entity.HasOne(x => x.Payment).WithMany(x => x.Refunds)
            .HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<AppUser>().WithMany().HasForeignKey(x => x.RefundedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
