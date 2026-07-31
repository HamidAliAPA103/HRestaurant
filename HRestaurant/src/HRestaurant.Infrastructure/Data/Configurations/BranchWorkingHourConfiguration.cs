using HRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRestaurant.Data.Configurations;

public sealed class BranchWorkingHourConfiguration
    : IEntityTypeConfiguration<BranchWorkingHour>
{
    public void Configure(EntityTypeBuilder<BranchWorkingHour> entity)
    {
        entity.ToTable("BranchWorkingHours");

        entity.HasIndex(workingHour => new
        {
            workingHour.BranchId,
            workingHour.DayOfWeek
        })
            .IsUnique();

        entity.Property(workingHour => workingHour.OpensAt)
            .HasColumnType("time");

        entity.Property(workingHour => workingHour.ClosesAt)
            .HasColumnType("time");

        entity.HasOne(workingHour => workingHour.Branch)
            .WithMany(branch => branch.WorkingHours)
            .HasForeignKey(workingHour => workingHour.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
