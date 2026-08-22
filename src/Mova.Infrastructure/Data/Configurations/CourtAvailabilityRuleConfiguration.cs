using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mova.Domain.Entities;

namespace Mova.Infrastructure.Data.Configurations;

public sealed class CourtAvailabilityRuleConfiguration : IEntityTypeConfiguration<CourtAvailabilityRule>
{
    public void Configure(EntityTypeBuilder<CourtAvailabilityRule> builder)
    {
        builder.ToTable("CourtAvailabilityRules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CourtId).IsRequired();
        builder.Property(x => x.DayOfWeek).IsRequired();
        builder.Property(x => x.StartTime).IsRequired();
        builder.Property(x => x.EndTime).IsRequired();
        builder.Property(x => x.SlotDurationMinutes).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.HasOne(x => x.Court).WithMany().HasForeignKey(x => x.CourtId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.CourtId, x.DayOfWeek });
        builder.ToTable(t => t.HasCheckConstraint("chk_court_availability_time", "\"StartTime\" <> \"EndTime\""));
    }
}
