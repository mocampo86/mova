using Mova.Contracts.Reservations;
using Mova.Domain.Entities;

namespace Mova.Application.Reservations;

public static class ReservationMapper
{
    public static ReservationInfo ToInfo(Reservation reservation) => new()
    {
        Id = reservation.Id,
        SportsComplexId = reservation.SportsComplexId,
        CourtId = reservation.CourtId,
        CourtName = reservation.Court?.Name ?? string.Empty,
        UserId = reservation.UserId,
        UserName = reservation.User?.FullName ?? string.Empty,
        StartAt = reservation.StartAt,
        EndAt = reservation.EndAt,
        Status = reservation.Status.ToString(),
        Source = reservation.Source.ToString(),
        Notes = reservation.Notes,
        CreatedAt = reservation.CreatedAt,
        CancelledAt = reservation.CancelledAt,
        CancellationReason = reservation.CancellationReason
    };
}
