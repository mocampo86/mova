using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mova.Domain.Entities;

namespace Mova.Infrastructure.Data.Configurations;

public sealed class CourtSportConfiguration : IEntityTypeConfiguration<CourtSport>
{
    public void Configure(EntityTypeBuilder<CourtSport> builder)
    {
        builder.ToTable("CourtSports");
        builder.HasKey(x => new { x.CourtId, x.SportId });
        builder.HasOne(x => x.Court).WithMany(x => x.CourtSports).HasForeignKey(x => x.CourtId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Sport).WithMany(x => x.CourtSports).HasForeignKey(x => x.SportId).OnDelete(DeleteBehavior.Cascade);
    }
}
