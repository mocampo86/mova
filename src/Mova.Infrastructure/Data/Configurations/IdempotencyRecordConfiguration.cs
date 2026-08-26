using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mova.Domain.Entities;

namespace Mova.Infrastructure.Data.Configurations;

public sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("IdempotencyRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ActorKey).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Scope).IsRequired().HasMaxLength(512);
        builder.Property(x => x.IdempotencyKey).IsRequired().HasMaxLength(256);
        builder.Property(x => x.StatusCode).IsRequired();
        builder.Property(x => x.ResponseBody).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.HasIndex(x => new { x.ActorKey, x.Scope, x.IdempotencyKey }).IsUnique();
        builder.HasIndex(x => x.ExpiresAt);
    }
}
