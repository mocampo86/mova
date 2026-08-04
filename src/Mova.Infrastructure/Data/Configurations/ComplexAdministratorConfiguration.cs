using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.Infrastructure.Data.Configurations;

public sealed class ComplexAdministratorConfiguration : IEntityTypeConfiguration<ComplexAdministrator>
{
    public void Configure(EntityTypeBuilder<ComplexAdministrator> builder)
    {
        builder.ToTable("ComplexAdministrators");
        builder.HasKey(ca => ca.Id);

        builder.Property(ca => ca.SportsComplexId)
            .IsRequired();

        builder.Property(ca => ca.UserId)
            .IsRequired();

        builder.Property(ca => ca.Role)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(ca => ca.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(ca => ca.CreatedAt)
            .IsRequired();

        builder.HasIndex(ca => new { ca.SportsComplexId, ca.UserId })
            .IsUnique();

        builder.HasIndex(ca => ca.UserId);
    }
}
