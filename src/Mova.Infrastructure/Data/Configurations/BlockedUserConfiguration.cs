using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mova.Domain.Entities;

namespace Mova.Infrastructure.Data.Configurations;

public sealed class BlockedUserConfiguration : IEntityTypeConfiguration<BlockedUser>
{
    public void Configure(EntityTypeBuilder<BlockedUser> builder)
    {
        builder.ToTable("BlockedUsers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SportsComplexId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.BlockedByUserId).IsRequired();
        builder.Property(x => x.BlockedAt).IsRequired();
        builder.Property(x => x.Status).IsRequired().HasConversion<string>();
        builder.HasIndex(x => x.SportsComplexId);
        builder.HasIndex(x => new { x.SportsComplexId, x.UserId, x.Status }).IsUnique();
    }
}
