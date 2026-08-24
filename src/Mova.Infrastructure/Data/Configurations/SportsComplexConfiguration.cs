using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.Infrastructure.Data.Configurations;

public sealed class SportsComplexConfiguration : IEntityTypeConfiguration<SportsComplex>
{
    public void Configure(EntityTypeBuilder<SportsComplex> builder)
    {
        builder.ToTable("SportsComplexes");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(s => s.Address)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.City)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.Latitude)
            .HasPrecision(10, 8);

        builder.Property(s => s.Longitude)
            .HasPrecision(11, 8);

        builder.Property(s => s.PhoneNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.AllowUserRecurringReservations)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.TimeZoneId)
            .HasMaxLength(100);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt);

        builder.HasMany(s => s.ComplexAdministrators)
            .WithOne(ca => ca.SportsComplex)
            .HasForeignKey(ca => ca.SportsComplexId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.City);
    }
}
