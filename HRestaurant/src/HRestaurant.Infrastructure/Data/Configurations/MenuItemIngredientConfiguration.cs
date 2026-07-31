using HRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRestaurant.Data.Configurations;

public sealed class MenuItemIngredientConfiguration
    : IEntityTypeConfiguration<MenuItemIngredient>
{
    public void Configure(EntityTypeBuilder<MenuItemIngredient> entity)
    {
        entity.ToTable("MenuItemIngredients");
        entity.HasKey(x => new { x.MenuItemId, x.IngredientId });
        entity.Property(x => x.RequiredQuantity).HasPrecision(18, 3);
        entity.HasOne(x => x.MenuItem).WithMany(x => x.Ingredients)
            .HasForeignKey(x => x.MenuItemId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(x => x.Ingredient).WithMany(x => x.MenuItems)
            .HasForeignKey(x => x.IngredientId).OnDelete(DeleteBehavior.Restrict);
    }
}
