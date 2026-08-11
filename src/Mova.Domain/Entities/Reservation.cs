using Mova.Domain.Enums;

namespace Mova.Domain.Entities;

public sealed class Reservation
{
    public Guid Id { get; private set; }
    public Guid SportsComplexId { get; private set; }
    public Guid CourtId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public ReservationStatus Status { get; private set; }
    public ReservationSource Source { get; private set; }
    public Guid? RecurringReservationId { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    public Court? Court { get; private set; }

    public User? User { get; private set; }

    private Reservation()
    {
    }

    public static Reservation Create(
        Guid sportsComplexId,
        Guid courtId,
        Guid userId,
        DateTime startAt,
        DateTime endAt,
        ReservationSource source,
        string? notes = null,
        Guid? recurringReservationId = null)
    {
        if (sportsComplexId == Guid.Empty) throw new ArgumentException("SportsComplexId cannot be empty.", nameof(sportsComplexId));
        if (courtId == Guid.Empty) throw new ArgumentException("CourtId cannot be empty.", nameof(courtId));
        if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty.", nameof(userId));
        if (startAt >= endAt) throw new ArgumentException("StartAt must be earlier than EndAt.", nameof(endAt));

        return new Reservation
        {
            Id = Guid.NewGuid(),
            SportsComplexId = sportsComplexId,
            CourtId = courtId,
            UserId = userId,
            StartAt = startAt,
            EndAt = endAt,
            Status = ReservationStatus.Pending,
            Source = source,
            RecurringReservationId = recurringReservationId,
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Confirm()
    {
        if (Status is ReservationStatus.CancelledByUser or ReservationStatus.CancelledByAdmin)
        {
            throw new InvalidOperationException("Cannot confirm a cancelled reservation.");
        }

        Status = ReservationStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string? reason = null, bool cancelledByAdmin = false)
    {
        if (Status is ReservationStatus.CancelledByUser or ReservationStatus.CancelledByAdmin)
        {
            return;
        }

        Status = cancelledByAdmin || DateTime.UtcNow > StartAt
            ? ReservationStatus.CancelledByAdmin
            : ReservationStatus.CancelledByUser;
        CancellationReason = reason;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkCompleted()
    {
        if (Status is ReservationStatus.CancelledByUser or ReservationStatus.CancelledByAdmin)
        {
            throw new InvalidOperationException("Cannot mark a cancelled reservation as completed.");
        }

        Status = ReservationStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkNoShow()
    {
        if (Status is ReservationStatus.CancelledByUser or ReservationStatus.CancelledByAdmin)
        {
            throw new InvalidOperationException("Cannot mark a cancelled reservation as no-show.");
        }

        Status = ReservationStatus.NoShow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsActiveForAvailability()
    {
        return Status is not (ReservationStatus.CancelledByUser or ReservationStatus.CancelledByAdmin);
    }
}
