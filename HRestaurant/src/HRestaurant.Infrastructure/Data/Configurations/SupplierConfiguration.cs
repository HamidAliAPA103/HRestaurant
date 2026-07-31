using HRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRestaurant.Data.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> entity)
    {
        entity.ToTable("Suppliers");
        entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
        entity.Property(x => x.NormalizedName).HasMaxLength(150).IsRequired();
        entity.Property(x => x.ContactPerson).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Phone).HasMaxLength(20).IsRequired();
        entity.Property(x => x.Email).HasMaxLength(254).IsRequired();
        entity.Property(x => x.Address).HasMaxLength(300).IsRequired();
        entity.HasIndex(x => new { x.RestaurantId, x.NormalizedName })
            .IsUnique().HasFilter("[IsDeleted] = 0");
        entity.HasIndex(x => new { x.RestaurantId, x.IsDeleted, x.IsActive });
        entity.HasOne(x => x.Restaurant).WithMany(x => x.Suppliers)
            .HasForeignKey(x => x.RestaurantId).OnDelete(DeleteBehavior.Restrict);
    }
}
