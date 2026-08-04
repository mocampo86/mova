using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.Infrastructure.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.GoogleSubjectId)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.GoogleSubjectId)
            .IsUnique();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(50);

        builder.Property(u => u.PhoneVerified)
            .IsRequired();

        builder.Property(u => u.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(u => u.Roles)
            .IsRequired()
            .HasColumnType("text[]")
            .HasDefaultValueSql("ARRAY[]::text[]")
            .HasConversion(
                roles => roles.Select(r => r.ToString()).ToList(),
                values => values.Select(v => Enum.Parse<Role>(v)).ToList())
            .Metadata.SetValueComparer(new ValueComparer<IReadOnlyCollection<Role>>(
                (left, right) => left != null && right != null && left.SequenceEqual(right),
                roles => roles.Aggregate(0, (hash, role) => HashCode.Combine(hash, (int)role)),
                roles => roles.ToList()));

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt);
    }
}
