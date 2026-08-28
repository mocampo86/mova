using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mova.Domain.Entities;

namespace Mova.Infrastructure.Data.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId);
        builder.Property(x => x.SportsComplexId);
        builder.Property(x => x.Action).IsRequired().HasMaxLength(255);
        builder.Property(x => x.EntityType).IsRequired().HasMaxLength(255);
        builder.Property(x => x.EntityId).IsRequired().HasMaxLength(255);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.Metadata).HasColumnType("text");
        builder.HasIndex(x => new { x.SportsComplexId, x.CreatedAt });
    }
}
