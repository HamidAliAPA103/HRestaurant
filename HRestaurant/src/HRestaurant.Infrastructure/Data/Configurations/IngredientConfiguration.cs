using HRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRestaurant.Data.Configurations;

public sealed class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> entity)
    {
        entity.ToTable("Ingredients");
        entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
        entity.Property(x => x.NormalizedName).HasMaxLength(100).IsRequired();
        entity.HasIndex(x => new { x.RestaurantId, x.NormalizedName })
            .IsUnique().HasFilter("[IsDeleted] = 0");
        entity.HasIndex(x => new { x.RestaurantId, x.IsDeleted, x.IsActive });
        entity.HasOne(x => x.Restaurant).WithMany(x => x.Ingredients)
            .HasForeignKey(x => x.RestaurantId).OnDelete(DeleteBehavior.Restrict);
    }
}
