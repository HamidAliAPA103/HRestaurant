using HRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRestaurant.Data.Configurations;

public sealed class RestaurantConfiguration
    : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> entity)
    {
        entity.Property(restaurant => restaurant.Name)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(restaurant => restaurant.Adres)
            .HasMaxLength(250)
            .IsRequired();

        entity.Property(restaurant => restaurant.Slug)
            .HasMaxLength(120)
            .IsUnicode(false)
            .IsRequired();

        entity.HasIndex(restaurant => restaurant.Slug)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        entity.HasIndex(restaurant => new
        {
            restaurant.IsDeleted,
            restaurant.IsActive,
            restaurant.Name
        });

        entity.HasIndex(restaurant => new
        {
            restaurant.IsDeleted,
            restaurant.CreatAt
        });

        entity.Property(restaurant => restaurant.Number)
            .HasMaxLength(15)
            .IsRequired();

        entity.Property(restaurant => restaurant.Email)
            .HasMaxLength(254);

        entity.Property(restaurant => restaurant.Description)
            .HasMaxLength(2000);

        entity.Property(restaurant => restaurant.LogoUrl)
            .HasMaxLength(500);

        entity.Property(restaurant => restaurant.CoverImageUrl)
            .HasMaxLength(500);

        entity.Property(restaurant => restaurant.Currency)
            .HasMaxLength(3)
            .IsUnicode(false)
            .IsRequired();

        entity.Property(restaurant => restaurant.TaxRate)
            .HasPrecision(5, 2);
    }
}
