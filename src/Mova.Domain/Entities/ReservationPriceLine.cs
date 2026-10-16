namespace Mova.Domain.Entities;

public sealed class ReservationPriceLine
{
    public Guid Id { get; private set; }
    public Guid ReservationId { get; private set; }
    public Guid? CourtRateId { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public decimal HourlyRate { get; private set; }
    public decimal Amount { get; private set; }

    public Reservation? Reservation { get; private set; }

    private ReservationPriceLine()
    {
    }

    public static ReservationPriceLine Create(
        Guid reservationId,
        Guid? courtRateId,
        DateTime startAt,
        DateTime endAt,
        decimal hourlyRate,
        decimal amount)
    {
        if (reservationId == Guid.Empty) throw new ArgumentException("ReservationId cannot be empty.", nameof(reservationId));
        if (startAt >= endAt) throw new ArgumentException("StartAt must be earlier than EndAt.", nameof(endAt));
        if (hourlyRate < 0) throw new ArgumentException("HourlyRate cannot be negative.", nameof(hourlyRate));
        if (amount < 0) throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        return new ReservationPriceLine
        {
            Id = Guid.NewGuid(),
            ReservationId = reservationId,
            CourtRateId = courtRateId,
            StartAt = startAt,
            EndAt = endAt,
            HourlyRate = hourlyRate,
            Amount = amount
        };
    }
}
