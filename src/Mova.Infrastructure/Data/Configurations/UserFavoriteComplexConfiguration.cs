using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mova.Domain.Entities;

namespace Mova.Infrastructure.Data.Configurations;

public sealed class UserFavoriteComplexConfiguration : IEntityTypeConfiguration<UserFavoriteComplex>
{
    public void Configure(EntityTypeBuilder<UserFavoriteComplex> builder)
    {
        builder.ToTable("UserFavoriteComplexes");
        builder.HasKey(x => new { x.UserId, x.SportsComplexId });

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SportsComplex)
            .WithMany()
            .HasForeignKey(x => x.SportsComplexId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.CreatedAt });
    }
}
