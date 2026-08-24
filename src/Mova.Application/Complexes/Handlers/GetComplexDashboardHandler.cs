using Mova.Application.Abstractions.Persistence;
using Mova.Application.Complexes.Queries;
using Mova.Contracts.Complexes;
using Mova.Domain.Exceptions;
using Mova.Domain.Helpers;

namespace Mova.Application.Complexes.Handlers;

public sealed class GetComplexDashboardHandler : IGetComplexDashboardHandler
{
    private readonly ISportsComplexRepository _sportsComplexRepository;
    private readonly ICourtRepository _courtRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IBlockedUserRepository _blockedUserRepository;

    public GetComplexDashboardHandler(
        ISportsComplexRepository sportsComplexRepository,
        ICourtRepository courtRepository,
        IReservationRepository reservationRepository,
        IBlockedUserRepository blockedUserRepository)
    {
        _sportsComplexRepository = sportsComplexRepository;
        _courtRepository = courtRepository;
        _reservationRepository = reservationRepository;
        _blockedUserRepository = blockedUserRepository;
    }

    public async Task<ComplexDashboardInfo?> HandleAsync(GetComplexDashboardQuery query, CancellationToken cancellationToken = default)
    {
        var sportsComplex = await _sportsComplexRepository.GetByIdAsync(query.ComplexId, cancellationToken);

        if (sportsComplex is null)
        {
            return null;
        }

        var (activeCourts, inactiveCourts) = await _courtRepository.GetCourtStatusCountsByComplexIdAsync(
            query.ComplexId,
            cancellationToken);

        if (!TimeZoneConverter.TryGetTimeZone(sportsComplex.TimeZoneId, out var timeZone))
        {
            throw new UnresolvedTimeZoneException();
        }

        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));
        var start = TimeZoneConverter.GetDayStartUtc(today, timeZone);
        var end = TimeZoneConverter.GetDayStartUtc(today.AddDays(1), timeZone);

        var (confirmed, cancelled, completed) = await _reservationRepository.GetTodayStatusCountsByComplexIdAsync(
            query.ComplexId,
            start,
            end,
            cancellationToken);

        var blockedUsers = await _blockedUserRepository.CountActiveByComplexIdAsync(
            query.ComplexId,
            cancellationToken);

        return new ComplexDashboardInfo
        {
            Complex = new DashboardComplexSummary
            {
                Id = sportsComplex.Id,
                Name = sportsComplex.Name,
                Status = sportsComplex.Status.ToString(),
                LastUpdatedAt = sportsComplex.UpdatedAt
            },
            Courts = new DashboardCourtSummary
            {
                Active = activeCourts,
                Inactive = inactiveCourts
            },
            ReservationsToday = new DashboardReservationsSummary
            {
                Confirmed = confirmed,
                Cancelled = cancelled,
                Completed = completed
            },
            BlockedUsers = blockedUsers
        };
    }
}
