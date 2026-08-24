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
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("chk_business_hours_day_of_week", "\"DayOfWeek\" BETWEEN 0 AND 6");
            t.HasCheckConstraint("chk_business_hours_opening_time", "\"OpeningTime\" >= '00:00:00' AND \"OpeningTime\" < '24:00:00'");
            t.HasCheckConstraint("chk_business_hours_closing_time", "\"ClosingTime\" >= '00:00:00' AND \"ClosingTime\" < '24:00:00'");
            t.HasCheckConstraint("chk_business_hours_not_closed", "\"IsClosed\" = TRUE OR \"OpeningTime\" <> \"ClosingTime\"");
        });
    }
}
