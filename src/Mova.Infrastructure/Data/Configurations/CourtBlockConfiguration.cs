using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mova.Domain.Entities;

namespace Mova.Infrastructure.Data.Configurations;

public sealed class CourtBlockConfiguration : IEntityTypeConfiguration<CourtBlock>
{
    public void Configure(EntityTypeBuilder<CourtBlock> builder)
    {
        builder.ToTable("CourtBlocks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SportsComplexId).IsRequired();
        builder.Property(x => x.CourtId).IsRequired();
        builder.Property(x => x.StartAt).IsRequired();
        builder.Property(x => x.EndAt).IsRequired();
        builder.Property(x => x.CreatedByUserId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => x.SportsComplexId);
        builder.HasIndex(x => new { x.CourtId, x.StartAt, x.EndAt });
        builder.HasOne<Court>().WithMany().HasForeignKey(x => x.CourtId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.ToTable(t => t.HasCheckConstraint("chk_block_time", "\"StartAt\" < \"EndAt\""));
    }
}
