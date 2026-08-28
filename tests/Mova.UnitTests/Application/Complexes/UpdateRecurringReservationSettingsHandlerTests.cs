using Mova.Application.Complexes.Commands;
using Mova.Application.Complexes.Handlers;
using Mova.Application.Common.Exceptions;
using Mova.Domain.Entities;
using Mova.UnitTests.Application.Audit;
using Mova.UnitTests.Application.Authentication;
using Xunit;

namespace Mova.UnitTests.Application.Complexes;

public sealed class UpdateRecurringReservationSettingsHandlerTests
{
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();
    private readonly FakeAuditLogRepository _auditLogs = new();
    private readonly FakeCurrentUserContext _currentUser = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private UpdateRecurringReservationSettingsHandler CreateHandler() =>
        new(_sportsComplexRepository, _auditLogs, _currentUser, _unitOfWork);

    [Fact]
    public async Task HandleAsync_WithExistingComplex_UpdatesSetting()
    {
        var complex = SportsComplex.Create(
            "Test Complex",
            "Description",
            "Address",
            "Montevideo",
            null,
            null,
            "+598 99 123 456",
            $"test-{Guid.NewGuid()}@example.com");
        await _sportsComplexRepository.AddAsync(complex);

        var handler = CreateHandler();

        var result = await handler.HandleAsync(new UpdateRecurringReservationSettingsCommand(complex.Id, false));

        Assert.False(result.AllowUserRecurringReservations);
        Assert.False(complex.AllowUserRecurringReservations);
    }

    [Fact]
    public async Task HandleAsync_WithMissingComplex_ThrowsNotFoundException()
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.HandleAsync(new UpdateRecurringReservationSettingsCommand(Guid.NewGuid(), false)));
    }
}
