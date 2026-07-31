using HRestaurant.Infrastructure.Identity;
using HRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRestaurant.Data.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("Users");
        entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Email).HasMaxLength(254).IsRequired();
        entity.Property(x => x.NormalizedEmail).HasMaxLength(254).IsRequired();
        entity.Property(x => x.Phone).HasMaxLength(20);
        entity.Property(x => x.NormalizedPhone).HasMaxLength(20);
        entity.Property(x => x.Role).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Salary).HasPrecision(18, 2);
        entity.Property(x => x.HireDate).HasColumnType("date");
        entity.Property(x => x.AvatarUrl).HasMaxLength(500);
        entity.Property(x => x.EmergencyContact).HasMaxLength(150);
        entity.Property(x => x.Birthday).HasColumnType("date");
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.Property(x => x.TotalSpent).HasPrecision(18, 2);

        entity.HasIndex(x => new { x.RestaurantId, x.NormalizedEmail }).IsUnique()
            .HasFilter("[RestaurantId] IS NOT NULL AND [NormalizedEmail] <> ''");
        entity.HasIndex(x => new { x.RestaurantId, x.NormalizedPhone }).IsUnique()
            .HasFilter("[RestaurantId] IS NOT NULL AND [NormalizedPhone] IS NOT NULL");
        entity.HasIndex(x => x.AppUserId).IsUnique()
            .HasFilter("[AppUserId] IS NOT NULL");
        entity.HasIndex(x => new { x.RestaurantId, x.BranchId, x.IsDeleted, x.IsActive });
        entity.HasIndex(x => new { x.RestaurantId, x.Role, x.IsDeleted });

        entity.HasOne(x => x.Restaurant).WithMany(x => x.Employees)
            .HasForeignKey(x => x.RestaurantId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Branch).WithMany(x => x.Employees)
            .HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<AppUser>().WithOne()
            .HasForeignKey<User>(x => x.AppUserId).OnDelete(DeleteBehavior.SetNull);
    }
}
