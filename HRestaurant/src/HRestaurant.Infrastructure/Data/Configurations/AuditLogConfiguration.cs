using HRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRestaurant.Data.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> entity)
    {
        entity.ToTable("AuditLogs");
        entity.Property(x => x.Action).HasMaxLength(50).IsRequired();
        entity.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
        entity.Property(x => x.OldValues).HasColumnType("nvarchar(max)");
        entity.Property(x => x.NewValues).HasColumnType("nvarchar(max)");
        entity.Property(x => x.IpAddress).HasMaxLength(64);
        entity.Property(x => x.UserAgent).HasMaxLength(500);
        entity.HasIndex(x => new { x.EntityName, x.EntityId, x.CreatAt });
        entity.HasIndex(x => new { x.UserId, x.CreatAt });
    }
}
