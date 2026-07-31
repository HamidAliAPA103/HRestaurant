using HRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRestaurant.Data.Configurations;

public sealed class MenuCategoryConfiguration
    : IEntityTypeConfiguration<MenuCategory>
{
    public void Configure(EntityTypeBuilder<MenuCategory> entity)
    {
        entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
        entity.Property(x => x.NormalizedName).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Description).HasMaxLength(1000);
        entity.Property(x => x.ImageUrl).HasMaxLength(500);
        entity.HasIndex(x => new { x.ResdaranId, x.NormalizedName })
            .IsUnique().HasFilter("[IsDeleted] = 0");
        entity.HasIndex(x => new { x.ResdaranId, x.IsDeleted, x.IsActive, x.DisplayOrder });
        entity.HasOne(x => x.Restaurant).WithMany(x => x.Categories)
            .HasForeignKey(x => x.ResdaranId).OnDelete(DeleteBehavior.Restrict);
    }
}
