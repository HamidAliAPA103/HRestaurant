using HRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRestaurant.Data.Configurations;

public sealed class EmployeeShiftConfiguration
    : IEntityTypeConfiguration<EmployeeShift>
{
    public void Configure(EntityTypeBuilder<EmployeeShift> entity)
    {
        entity.ToTable("EmployeeShifts");
        entity.Property(x => x.WorkDate).HasColumnType("date");
        entity.Property(x => x.StartTime).HasColumnType("time");
        entity.Property(x => x.EndTime).HasColumnType("time");
        entity.Property(x => x.Notes).HasMaxLength(500);
        entity.HasIndex(x => new { x.EmployeeId, x.ShiftId, x.WorkDate })
            .IsUnique().HasFilter("[IsDeleted] = 0");
        entity.HasIndex(x => new { x.EmployeeId, x.WorkDate, x.StartTime, x.EndTime });
        entity.HasIndex(x => new { x.ShiftId, x.WorkDate });
        entity.HasOne(x => x.Employee).WithMany(x => x.EmployeeShifts)
            .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Shift).WithMany(x => x.EmployeeShifts)
            .HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
    }
}
