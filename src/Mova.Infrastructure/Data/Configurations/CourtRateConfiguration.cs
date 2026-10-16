using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mova.Domain.Entities;

namespace Mova.Infrastructure.Data.Configurations;

public sealed class CourtRateConfiguration : IEntityTypeConfiguration<CourtRate>
{
    public void Configure(EntityTypeBuilder<CourtRate> builder)
    {
        builder.ToTable("CourtRates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CourtId).IsRequired();
        builder.Property(x => x.DaysOfWeek).IsRequired();
        builder.Property(x => x.StartTime).IsRequired();
        builder.Property(x => x.EndTime).IsRequired();
        builder.Property(x => x.AmountPerHour).IsRequired().HasPrecision(18, 2);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);
        builder.HasOne(x => x.Court).WithMany().HasForeignKey(x => x.CourtId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.CourtId);
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("chk_court_rate_days_of_week", "\"DaysOfWeek\" BETWEEN 1 AND 127");
            t.HasCheckConstraint("chk_court_rate_time", "\"StartTime\" >= interval '0 hours' AND \"StartTime\" < \"EndTime\" AND \"EndTime\" <= interval '24 hours'");
            t.HasCheckConstraint("chk_court_rate_amount", "\"AmountPerHour\" > 0");
        });
    }
}
