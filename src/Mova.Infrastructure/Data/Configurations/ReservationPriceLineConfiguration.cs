using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mova.Domain.Entities;

namespace Mova.Infrastructure.Data.Configurations;

public sealed class ReservationPriceLineConfiguration : IEntityTypeConfiguration<ReservationPriceLine>
{
    public void Configure(EntityTypeBuilder<ReservationPriceLine> builder)
    {
        builder.ToTable("ReservationPriceLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReservationId).IsRequired();
        builder.Property(x => x.CourtRateId);
        builder.Property(x => x.StartAt).IsRequired();
        builder.Property(x => x.EndAt).IsRequired();
        builder.Property(x => x.HourlyRate).IsRequired().HasPrecision(18, 2);
        builder.Property(x => x.Amount).IsRequired().HasPrecision(18, 2);
        builder.HasOne(x => x.Reservation).WithMany(r => r.PriceLines).HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<CourtRate>().WithMany().HasForeignKey(x => x.CourtRateId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => x.ReservationId);
        builder.ToTable(t => t.HasCheckConstraint("chk_reservation_price_line_time", "\"StartAt\" < \"EndAt\""));
    }
}
