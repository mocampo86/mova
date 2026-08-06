using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mova.Domain.Entities;

namespace Mova.Infrastructure.Data.Configurations;

public sealed class BusinessHoursConfiguration : IEntityTypeConfiguration<BusinessHours>
{
    public void Configure(EntityTypeBuilder<BusinessHours> builder)
    {
        builder.ToTable("BusinessHours");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SportsComplexId).IsRequired();
        builder.Property(x => x.DayOfWeek).IsRequired();
        builder.Property(x => x.OpeningTime).IsRequired();
        builder.Property(x => x.ClosingTime).IsRequired();
        builder.Property(x => x.IsClosed).IsRequired();
        builder.HasIndex(x => new { x.SportsComplexId, x.DayOfWeek }).IsUnique();
        builder.HasOne(x => x.SportsComplex).WithMany().HasForeignKey(x => x.SportsComplexId).OnDelete(DeleteBehavior.Cascade);
    }
}
