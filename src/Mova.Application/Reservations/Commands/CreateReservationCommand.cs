using Mova.Domain.Enums;

namespace Mova.Application.Reservations.Commands;

public sealed class CreateReservationCommand
{
    public CreateReservationCommand(
        Guid sportsComplexId,
        Guid courtId,
        Guid userId,
        Guid createdByUserId,
        DateTime startAt,
        DateTime endAt,
        string? notes)
        : this(sportsComplexId, courtId, userId, createdByUserId, startAt, endAt, notes, ReservationSource.Admin)
    {
    }

    public CreateReservationCommand(
        Guid sportsComplexId,
        Guid courtId,
        Guid userId,
        Guid createdByUserId,
        DateTime startAt,
        DateTime endAt,
        string? notes,
        ReservationSource source)
    {
        SportsComplexId = sportsComplexId;
        CourtId = courtId;
        UserId = userId;
        CreatedByUserId = createdByUserId;
        StartAt = startAt;
        EndAt = endAt;
        Notes = notes;
        Source = source;
    }

    public Guid SportsComplexId { get; }
    public Guid CourtId { get; }
    public Guid UserId { get; }
    public Guid CreatedByUserId { get; }
    public DateTime StartAt { get; }
    public DateTime EndAt { get; }
    public string? Notes { get; }
    public ReservationSource Source { get; }
}
