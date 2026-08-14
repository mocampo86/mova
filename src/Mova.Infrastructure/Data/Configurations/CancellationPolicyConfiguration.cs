using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mova.Domain.Entities;

namespace Mova.Infrastructure.Data.Configurations;

public sealed class CancellationPolicyConfiguration : IEntityTypeConfiguration<CancellationPolicy>
{
    public void Configure(EntityTypeBuilder<CancellationPolicy> builder)
    {
        builder.ToTable("CancellationPolicies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SportsComplexId).IsRequired();
        builder.Property(x => x.MinimumHours).IsRequired();
        builder.Property(x => x.AllowUserCancellation).IsRequired();
        builder.HasIndex(x => x.SportsComplexId).IsUnique();
        builder.HasOne(x => x.SportsComplex).WithMany().HasForeignKey(x => x.SportsComplexId).OnDelete(DeleteBehavior.Cascade);
    }
}
