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
        entity.Property(x => x.Model3DUrl).HasMaxLength(500);
        entity.Property(x => x.ImageUrl).HasMaxLength(500);
        entity.Property(x => x.Description).HasMaxLength(1000);
        entity.Property(x => x.Origin).HasMaxLength(120);
        entity.Property(x => x.AllergenInformation).HasMaxLength(500);
        entity.Property(x => x.Calories).HasPrecision(10, 2);
        entity.Property(x => x.Protein).HasPrecision(10, 2);
        entity.Property(x => x.Carbohydrates).HasPrecision(10, 2);
        entity.Property(x => x.Fat).HasPrecision(10, 2);
        entity.HasIndex(x => new { x.RestaurantId, x.NormalizedName })
            .IsUnique().HasFilter("[IsDeleted] = 0");
        entity.HasIndex(x => new { x.RestaurantId, x.IsDeleted, x.IsActive });
        entity.HasOne(x => x.Restaurant).WithMany(x => x.Ingredients)
            .HasForeignKey(x => x.RestaurantId).OnDelete(DeleteBehavior.Restrict);
    }
}
