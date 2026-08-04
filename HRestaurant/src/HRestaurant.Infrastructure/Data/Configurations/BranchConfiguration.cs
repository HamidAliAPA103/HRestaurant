using HRestaurant.Infrastructure.Identity;
using HRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRestaurant.Data.Configurations;

public sealed class BranchConfiguration
    : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> entity)
    {
        entity.ToTable("Branches");

        entity.Property(branch => branch.Name)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(branch => branch.NormalizedName)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(branch => branch.Slug)
            .HasMaxLength(120)
            .IsUnicode(false)
            .IsRequired();

        entity.Property(branch => branch.Address)
            .HasMaxLength(250)
            .IsRequired();

        entity.Property(branch => branch.Phone)
            .HasMaxLength(20);

        entity.Property(branch => branch.Email)
            .HasMaxLength(254);

        entity.Property(branch => branch.Latitude)
            .HasPrecision(8, 6);

        entity.Property(branch => branch.Longitude)
            .HasPrecision(9, 6);

        entity.Property(branch => branch.FrontImageUrl).HasMaxLength(500);
        entity.Property(branch => branch.CoverImageUrl).HasMaxLength(500);
        entity.Property(branch => branch.ShortDescription).HasMaxLength(500);
        entity.Property(branch => branch.GoogleMapsUrl).HasMaxLength(500);
        entity.Property(branch => branch.VirtualTourUrl).HasMaxLength(500);
        entity.Property(branch => branch.ParkingInfo).HasMaxLength(500);
        entity.Property(branch => branch.Landmark).HasMaxLength(250);
        entity.Property(branch => branch.IsPubliclyVisible).HasDefaultValue(true);

        entity.Property(branch => branch.TimeZoneId)
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired();

        entity.HasIndex(branch => new
        {
            branch.RestaurantId,
            branch.NormalizedName
        })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        entity.HasIndex(branch => new
        {
            branch.RestaurantId,
            branch.Slug
        })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        entity.HasIndex(branch => new
        {
            branch.RestaurantId,
            branch.IsDeleted,
            branch.IsActive
        });

        entity.HasIndex(branch => new
        {
            branch.ManagerId,
            branch.IsDeleted
        });

        entity.HasOne(branch => branch.Restaurant)
            .WithMany(restaurant => restaurant.Branches)
            .HasForeignKey(branch => branch.RestaurantId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(branch => branch.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
