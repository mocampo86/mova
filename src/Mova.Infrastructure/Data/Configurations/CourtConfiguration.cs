using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mova.Domain.Entities;

namespace Mova.Infrastructure.Data.Configurations;

public sealed class CourtConfiguration : IEntityTypeConfiguration<Court>
{
    public void Configure(EntityTypeBuilder<Court> builder)
    {
        builder.ToTable("Courts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.SurfaceType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Status).IsRequired().HasConversion<string>();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => x.SportsComplexId);
        builder.HasIndex(x => new { x.SportsComplexId, x.Name }).IsUnique();
        builder.HasOne(x => x.SportsComplex).WithMany().HasForeignKey(x => x.SportsComplexId).OnDelete(DeleteBehavior.Cascade);

    }
}
