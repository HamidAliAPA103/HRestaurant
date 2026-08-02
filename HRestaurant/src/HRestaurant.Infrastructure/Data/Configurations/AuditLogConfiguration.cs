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
        // Let the active EF provider select its native large-text type.
        // Hard-coding nvarchar(max) makes the model impossible to create with
        // SQLite, which is also used by the integration test suite.
        entity.Property(x => x.OldValues).IsUnicode();
        entity.Property(x => x.NewValues).IsUnicode();
        entity.Property(x => x.IpAddress).HasMaxLength(64);
        entity.Property(x => x.UserAgent).HasMaxLength(500);
        entity.HasIndex(x => new { x.EntityName, x.EntityId, x.CreatAt });
        entity.HasIndex(x => new { x.UserId, x.CreatAt });
    }
}
