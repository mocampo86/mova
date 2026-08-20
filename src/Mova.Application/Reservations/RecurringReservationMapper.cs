using Mova.Contracts.Reservations;
using Mova.Domain.Entities;

namespace Mova.Application.Reservations;

public static class RecurringReservationMapper
{
    public static RecurringReservationInfo ToInfo(RecurringReservation recurringReservation, IReadOnlyList<Reservation> occurrences) => new()
    {
        Id = recurringReservation.Id,
        SportsComplexId = recurringReservation.SportsComplexId,
        CourtId = recurringReservation.CourtId,
        UserId = recurringReservation.UserId,
        DayOfWeek = (int)recurringReservation.DayOfWeek,
        StartTime = recurringReservation.StartTime,
        DurationMinutes = recurringReservation.DurationMinutes,
        StartDate = recurringReservation.StartDate,
        EndDate = recurringReservation.EndDate,
        Status = recurringReservation.Status.ToString(),
        CreatedAt = recurringReservation.CreatedAt,
        UpdatedAt = recurringReservation.UpdatedAt,
        Occurrences = occurrences.Select(ReservationMapper.ToInfo).ToList()
    };

    public static RecurringReservationListItem ToListItem(RecurringReservation recurringReservation) => new()
    {
        Id = recurringReservation.Id,
        SportsComplexId = recurringReservation.SportsComplexId,
        CourtId = recurringReservation.CourtId,
        CourtName = recurringReservation.Court?.Name ?? string.Empty,
        UserId = recurringReservation.UserId,
        UserName = recurringReservation.User?.FullName ?? string.Empty,
        DayOfWeek = (int)recurringReservation.DayOfWeek,
        StartTime = recurringReservation.StartTime,
        DurationMinutes = recurringReservation.DurationMinutes,
        StartDate = recurringReservation.StartDate,
        EndDate = recurringReservation.EndDate,
        Status = recurringReservation.Status.ToString(),
        CreatedAt = recurringReservation.CreatedAt,
        UpdatedAt = recurringReservation.UpdatedAt
    };
}
