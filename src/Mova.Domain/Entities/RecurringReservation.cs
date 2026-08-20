using Mova.Domain.Enums;

namespace Mova.Domain.Entities;

public sealed class RecurringReservation
{
    public Guid Id { get; private set; }
    public Guid SportsComplexId { get; private set; }
    public Guid CourtId { get; private set; }
    public Guid UserId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public int DurationMinutes { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public RecurringReservationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public Court? Court { get; private set; }
    public User? User { get; private set; }

    private RecurringReservation()
    {
    }

    public static RecurringReservation Create(
        Guid sportsComplexId,
        Guid courtId,
        Guid userId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        int durationMinutes,
        DateOnly startDate,
        DateOnly endDate)
    {
        if (sportsComplexId == Guid.Empty) throw new ArgumentException("SportsComplexId cannot be empty.", nameof(sportsComplexId));
        if (courtId == Guid.Empty) throw new ArgumentException("CourtId cannot be empty.", nameof(courtId));
        if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty.", nameof(userId));
        if (durationMinutes <= 0) throw new ArgumentException("DurationMinutes must be greater than zero.", nameof(durationMinutes));
        if (startDate > endDate) throw new ArgumentException("StartDate must be on or before EndDate.", nameof(endDate));

        return new RecurringReservation
        {
            Id = Guid.NewGuid(),
            SportsComplexId = sportsComplexId,
            CourtId = courtId,
            UserId = userId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            DurationMinutes = durationMinutes,
            StartDate = startDate,
            EndDate = endDate,
            Status = RecurringReservationStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Cancel()
    {
        Status = RecurringReservationStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ModifyFuture(DayOfWeek dayOfWeek, TimeOnly startTime, int durationMinutes, DateOnly effectiveDate, DateOnly endDate)
    {
        if (Status != RecurringReservationStatus.Active)
        {
            throw new InvalidOperationException("Only active recurring reservations can be modified.");
        }

        if (durationMinutes <= 0)
        {
            throw new ArgumentException("DurationMinutes must be greater than zero.", nameof(durationMinutes));
        }

        if (effectiveDate > endDate)
        {
            throw new ArgumentException("EffectiveDate must be on or before EndDate.", nameof(endDate));
        }

        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        DurationMinutes = durationMinutes;
        EndDate = endDate;
        UpdatedAt = DateTime.UtcNow;
    }
}
