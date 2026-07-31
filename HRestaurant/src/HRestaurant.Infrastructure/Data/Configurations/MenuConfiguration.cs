using HRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRestaurant.Data.Configurations;

public sealed class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> entity)
    {
        entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
        entity.Property(x => x.NormalizedName).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Image).HasMaxLength(255).IsRequired();
        entity.Property(x => x.ImageURL).HasMaxLength(500).IsRequired();
        entity.Property(x => x.Desc).HasMaxLength(1000).IsRequired();
        entity.Property(x => x.Nutrition).HasMaxLength(1000).IsRequired();
        entity.Property(x => x.Price).HasPrecision(18, 2);
        entity.Property(x => x.DiscountPercentage).HasPrecision(5, 2);
        entity.Property(x => x.FinalPrice).HasPrecision(18, 2);
        entity.HasIndex(x => new { x.CategoryId, x.NormalizedName })
            .IsUnique().HasFilter("[IsDeleted] = 0");
        entity.HasIndex(x => new { x.RestaurantId, x.IsDeleted, x.IsAvailable, x.IsPopular });
        entity.HasOne(x => x.Restaurant).WithMany()
            .HasForeignKey(x => x.RestaurantId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Category).WithMany(x => x.Menus)
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}
