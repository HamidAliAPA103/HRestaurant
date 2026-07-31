using HRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRestaurant.Data.Configurations;

public sealed class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> entity)
    {
        entity.ToTable("Shifts");
        entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
        entity.Property(x => x.StartTime).HasColumnType("time");
        entity.Property(x => x.EndTime).HasColumnType("time");
        entity.HasIndex(x => new { x.RestaurantId, x.BranchId, x.IsDeleted, x.IsActive });
        entity.HasOne(x => x.Restaurant).WithMany(x => x.Shifts)
            .HasForeignKey(x => x.RestaurantId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Branch).WithMany(x => x.Shifts)
            .HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
    }
}
