using HRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRestaurant.Data.Configurations;

public sealed class RestaurantWorkingHourConfiguration
    : IEntityTypeConfiguration<RestaurantWorkingHour>
{
    public void Configure(
        EntityTypeBuilder<RestaurantWorkingHour> entity)
    {
        entity.ToTable("RestaurantWorkingHours");

        entity.HasIndex(workingHour => new
        {
            workingHour.RestaurantId,
            workingHour.DayOfWeek
        })
            .IsUnique();

        entity.Property(workingHour => workingHour.OpensAt)
            .HasColumnType("time");

        entity.Property(workingHour => workingHour.ClosesAt)
            .HasColumnType("time");

        entity.HasOne(workingHour => workingHour.Restaurant)
            .WithMany(restaurant => restaurant.WorkingHours)
            .HasForeignKey(workingHour => workingHour.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
