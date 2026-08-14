using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mova.Domain.Entities;

namespace Mova.Infrastructure.Data.Configurations;

public sealed class RecurringReservationConfiguration : IEntityTypeConfiguration<RecurringReservation>
{
    public void Configure(EntityTypeBuilder<RecurringReservation> builder)
    {
        builder.ToTable("RecurringReservations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SportsComplexId).IsRequired();
        builder.Property(x => x.CourtId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.DayOfWeek).IsRequired().HasConversion<int>();
        builder.Property(x => x.StartTime).IsRequired();
        builder.Property(x => x.DurationMinutes).IsRequired();
        builder.Property(x => x.StartDate).IsRequired();
        builder.Property(x => x.EndDate).IsRequired();
        builder.Property(x => x.Status).IsRequired().HasConversion<string>();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => x.SportsComplexId);
        builder.HasIndex(x => new { x.CourtId, x.Status });
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Court).WithMany().HasForeignKey(x => x.CourtId).OnDelete(DeleteBehavior.Cascade);
        builder.ToTable(t => t.HasCheckConstraint("chk_recurring_dates", "\"StartDate\" <= \"EndDate\""));
    }
}
