using Mova.Application.Common.Exceptions;
using Mova.Application.Reservations.Commands;
using Mova.Application.Reservations.Handlers;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Domain.Helpers;
using Mova.UnitTests.Application.Audit;
using Mova.UnitTests.Application.Authentication;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Reservations;

public sealed class RecurringReservationMutationHandlerTests
{
    private readonly FakeRecurringReservationRepository _recurringReservationRepository = new();
    private readonly FakeReservationRepository _reservationRepository = new();
    private readonly FakeCourtBlockRepository _courtBlockRepository = new();
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();
    private readonly FakeAuditLogRepository _auditLogs = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CancelRecurringReservationHandler CreateCancelHandler(int minimumHours = 0, bool allowUserCancellation = true) =>
        new(
            _recurringReservationRepository,
            _reservationRepository,
            new FakeCancellationPolicy(minimumHours, allowUserCancellation),
            _auditLogs,
            _unitOfWork);

    private ModifyRecurringReservationFutureHandler CreateModifyHandler() =>
        new(
            _recurringReservationRepository,
            _reservationRepository,
            _courtBlockRepository,
            _sportsComplexRepository,
            _auditLogs,
            _unitOfWork);

    [Fact]
    public async Task CancelAsync_AsUser_CancelsFutureOccurrences()
    {
        var series = await CreateSeriesWithOccurrencesAsync(
            new DateOnly(2030, 1, 7),
            new DateOnly(2030, 1, 21));

        var handler = CreateCancelHandler();

        var result = await handler.HandleAsync(new CancelRecurringReservationCommand(
            series.SportsComplexId,
            series.Id,
            series.UserId,
            "No longer needed"));

        Assert.Equal("Cancelled", result.Status);
        Assert.Equal(3, result.Occurrences.Count);
        Assert.All(result.Occurrences, occurrence =>
        {
            Assert.Equal("CancelledByUser", occurrence.Status);
            Assert.Equal("No longer needed", occurrence.CancellationReason);
            Assert.Equal(series.UserId, occurrence.CancelledByUserId);
        });

        Assert.Empty(_auditLogs.AuditLogs);
    }

    [Fact]
    public async Task CancelAsync_AsAdmin_CancelsFutureOccurrencesAsCancelledByAdminAndCreatesAuditLog()
    {
        var series = await CreateSeriesWithOccurrencesAsync(
            new DateOnly(2030, 1, 7),
            new DateOnly(2030, 1, 21));

        var adminUserId = Guid.NewGuid();
        var handler = CreateCancelHandler();

        var result = await handler.HandleAsync(new CancelRecurringReservationCommand(
            series.SportsComplexId,
            series.Id,
            adminUserId,
            "Admin cancel",
            true));

        Assert.Equal("Cancelled", result.Status);
        Assert.Equal(3, result.Occurrences.Count);
        Assert.All(result.Occurrences, occurrence =>
        {
            Assert.Equal("CancelledByAdmin", occurrence.Status);
            Assert.Equal("Admin cancel", occurrence.CancellationReason);
            Assert.Equal(adminUserId, occurrence.CancelledByUserId);
        });

        var auditLog = Assert.Single(_auditLogs.AuditLogs);
        Assert.Equal("RecurringReservation.Cancel", auditLog.Action);
        Assert.Equal("RecurringReservation", auditLog.EntityType);
        Assert.Equal(series.Id.ToString(), auditLog.EntityId);
        Assert.Equal(series.SportsComplexId, auditLog.SportsComplexId);
        Assert.Equal(adminUserId, auditLog.UserId);
    }

    [Fact]
    public async Task CancelAsync_AsUnrelatedUser_ThrowsNotFoundException()
    {
        var series = await CreateSeriesWithOccurrencesAsync(
            new DateOnly(2030, 1, 7),
            new DateOnly(2030, 1, 21));

        var handler = CreateCancelHandler();

        var unrelatedUserId = Guid.NewGuid();
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new CancelRecurringReservationCommand(
            series.SportsComplexId,
            series.Id,
            unrelatedUserId,
            "No longer needed")));

        Assert.Equal("Recurring reservation not found.", exception.Message);
    }

    [Fact]
    public async Task CancelAsync_AsUser_WhenUserCancellationDisabled_ThrowsUserCancellationDisabledException()
    {
        var series = await CreateSeriesWithOccurrencesAsync(
            new DateOnly(2030, 1, 7),
            new DateOnly(2030, 1, 21));

        var handler = CreateCancelHandler(allowUserCancellation: false);

        await Assert.ThrowsAsync<UserCancellationDisabledException>(() => handler.HandleAsync(new CancelRecurringReservationCommand(
            series.SportsComplexId,
            series.Id,
            series.UserId,
            "No longer needed")));
    }

    [Fact]
    public async Task CancelAsync_AsAdmin_WhenUserCancellationDisabled_CancelsFutureOccurrencesAndCreatesAuditLog()
    {
        var series = await CreateSeriesWithOccurrencesAsync(
            new DateOnly(2030, 1, 7),
            new DateOnly(2030, 1, 21));

        var adminUserId = Guid.NewGuid();
        var handler = CreateCancelHandler(allowUserCancellation: false);

        var result = await handler.HandleAsync(new CancelRecurringReservationCommand(
            series.SportsComplexId,
            series.Id,
            adminUserId,
            "Admin cancel",
            true));

        Assert.Equal("Cancelled", result.Status);
        Assert.Equal(3, result.Occurrences.Count);
        Assert.All(result.Occurrences, occurrence => Assert.Equal("CancelledByAdmin", occurrence.Status));

        var auditLog = Assert.Single(_auditLogs.AuditLogs);
        Assert.Equal("RecurringReservation.Cancel", auditLog.Action);
        Assert.Equal("RecurringReservation", auditLog.EntityType);
        Assert.Equal(series.Id.ToString(), auditLog.EntityId);
        Assert.Equal(series.SportsComplexId, auditLog.SportsComplexId);
        Assert.Equal(adminUserId, auditLog.UserId);
    }

    [Fact]
    public async Task ModifyFutureAsync_CancelsOldFutureAndCreatesNewOccurrences()
    {
        var series = await CreateSeriesWithOccurrencesAsync(
            new DateOnly(2026, 8, 10),
            new DateOnly(2026, 8, 24));
        var handler = CreateModifyHandler();

        var result = await handler.HandleAsync(new ModifyRecurringReservationFutureCommand(
            series.SportsComplexId,
            series.Id,
            series.UserId,
            new DateOnly(2026, 8, 17),
            DayOfWeek.Tuesday,
            new TimeOnly(16, 0),
            90,
            new DateOnly(2026, 8, 31),
            "Moved",
            false));

        Assert.Equal(DayOfWeek.Tuesday, (DayOfWeek)result.DayOfWeek);
        Assert.Equal(new TimeOnly(16, 0), result.StartTime);
        Assert.Equal(2, result.Occurrences.Count);

        var timeZone = TimeZoneConverter.GetTimeZone("America/Montevideo");
        Assert.All(result.Occurrences, occurrence =>
        {
            Assert.Equal("Confirmed", occurrence.Status);
            Assert.Equal("Recurring", occurrence.Source);
            var localStart = TimeZoneInfo.ConvertTimeFromUtc(occurrence.StartAt, timeZone);
            Assert.Equal(new TimeOnly(16, 0), TimeOnly.FromDateTime(localStart));
        });

        var oldFuture = await _reservationRepository.GetFutureActiveByRecurringReservationIdAsync(
            series.Id,
            new DateTime(2026, 8, 17, 0, 0, 0, DateTimeKind.Utc));
        Assert.Equal(2, oldFuture.Count);

        Assert.Empty(_auditLogs.AuditLogs);
    }

    [Fact]
    public async Task ModifyFutureAsync_AsAdmin_AllowedForCustomerSeriesAndCreatesAuditLog()
    {
        var series = await CreateSeriesWithOccurrencesAsync(
            new DateOnly(2026, 8, 10),
            new DateOnly(2026, 8, 24));

        var adminUserId = Guid.NewGuid();
        var handler = CreateModifyHandler();

        var result = await handler.HandleAsync(new ModifyRecurringReservationFutureCommand(
            series.SportsComplexId,
            series.Id,
            adminUserId,
            new DateOnly(2026, 8, 17),
            DayOfWeek.Tuesday,
            new TimeOnly(16, 0),
            90,
            new DateOnly(2026, 8, 31),
            "Moved by admin",
            true));

        Assert.Equal(DayOfWeek.Tuesday, (DayOfWeek)result.DayOfWeek);

        var (allReservations, _) = await _reservationRepository.GetByComplexIdAsync(
            series.SportsComplexId,
            1,
            100,
            userId: series.UserId);

        Assert.Equal(5, allReservations.Count);
        Assert.Equal(3, allReservations.Count(r => r.Status == ReservationStatus.Confirmed));

        var cancelled = allReservations.Where(r => r.Status == ReservationStatus.CancelledByAdmin).ToList();
        Assert.Equal(2, cancelled.Count);
        Assert.All(cancelled, occurrence =>
        {
            Assert.Equal(adminUserId, occurrence.CancelledByUserId);
            Assert.Equal("Modified recurring reservation.", occurrence.CancellationReason);
        });

        var auditLog = Assert.Single(_auditLogs.AuditLogs);
        Assert.Equal("RecurringReservation.ModifyFuture", auditLog.Action);
        Assert.Equal("RecurringReservation", auditLog.EntityType);
        Assert.Equal(series.Id.ToString(), auditLog.EntityId);
        Assert.Equal(series.SportsComplexId, auditLog.SportsComplexId);
        Assert.Equal(adminUserId, auditLog.UserId);
    }

    [Fact]
    public async Task ModifyFutureAsync_AsUnrelatedUser_ThrowsNotFoundException()
    {
        var series = await CreateSeriesWithOccurrencesAsync(
            new DateOnly(2026, 8, 10),
            new DateOnly(2026, 8, 24));

        var unrelatedUserId = Guid.NewGuid();
        var handler = CreateModifyHandler();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new ModifyRecurringReservationFutureCommand(
            series.SportsComplexId,
            series.Id,
            unrelatedUserId,
            new DateOnly(2026, 8, 17),
            DayOfWeek.Tuesday,
            new TimeOnly(16, 0),
            90,
            new DateOnly(2026, 8, 31),
            "Moved",
            false)));

        Assert.Equal("Recurring reservation not found.", exception.Message);
    }

    private async Task<RecurringReservation> CreateSeriesWithOccurrencesAsync(DateOnly startDate, DateOnly endDate)
    {
        var courtId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var sportsComplex = SportsComplex.Create(
            "Test Complex",
            "Description",
            "Address",
            "Montevideo",
            null,
            null,
            "+598 99 123 456",
            $"test-{Guid.NewGuid()}@example.com");
        await _sportsComplexRepository.AddAsync(sportsComplex);

        var series = RecurringReservation.Create(
            sportsComplex.Id,
            courtId,
            userId,
            DayOfWeek.Monday,
            new TimeOnly(14, 0),
            60,
            startDate,
            endDate);

        await _recurringReservationRepository.AddAsync(series);

        var occurrences = CreateRecurringReservationHandler.GenerateOccurrences(
            series.Id,
            sportsComplex.Id,
            courtId,
            userId,
            DayOfWeek.Monday,
            new TimeOnly(14, 0),
            60,
            startDate,
            endDate,
            null,
            TimeZoneConverter.GetTimeZone(sportsComplex.TimeZoneId!));

        await _reservationRepository.AddRangeAsync(occurrences);
        return series;
    }
}
