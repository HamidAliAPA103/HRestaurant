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
        entity.Property(x => x.Model3DUrl).HasMaxLength(500);
        entity.Property(x => x.ModelPosterUrl).HasMaxLength(500);
        entity.Property(x => x.EnableIngredientAnimation).HasDefaultValue(false);
        entity.Property(x => x.VideoUrl).HasMaxLength(500);
        entity.Property(x => x.VideoPosterUrl).HasMaxLength(500);
        entity.Property(x => x.IsVideoEnabled).HasDefaultValue(false);
        entity.Property(x => x.VideoDisplayOrder).HasDefaultValue(0);
        entity.Property(x => x.Price).HasPrecision(18, 2);
        entity.Property(x => x.DiscountPercentage).HasPrecision(5, 2);
        entity.Property(x => x.FinalPrice).HasPrecision(18, 2);
        entity.Property(x => x.ModelScale).HasPrecision(8, 4).HasDefaultValue(1m);
        entity.Property(x => x.ModelRotationX).HasPrecision(9, 4);
        entity.Property(x => x.ModelRotationY).HasPrecision(9, 4);
        entity.Property(x => x.ModelRotationZ).HasPrecision(9, 4);
        entity.HasIndex(x => new { x.CategoryId, x.NormalizedName })
            .IsUnique().HasFilter("[IsDeleted] = 0");
        entity.HasIndex(x => new { x.RestaurantId, x.IsDeleted, x.IsAvailable, x.IsPopular });
        entity.HasIndex(x => new { x.RestaurantId, x.IsVideoEnabled, x.VideoDisplayOrder });
        entity.HasOne(x => x.Restaurant).WithMany()
            .HasForeignKey(x => x.RestaurantId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Category).WithMany(x => x.Menus)
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}
