using Mova.Application.Complexes.Commands;
using Mova.Application.Complexes.Handlers;
using Mova.Application.Common.Exceptions;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.UnitTests.Application.Audit;
using Mova.UnitTests.Application.Authentication;
using Xunit;

namespace Mova.UnitTests.Application.Complexes;

public class UpdateComplexStatusHandlerTests
{
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();
    private readonly FakeAuditLogRepository _auditLogs = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private UpdateComplexStatusHandler CreateHandler() =>
        new(_sportsComplexRepository, _auditLogs, _unitOfWork);

    [Fact]
    public async Task HandleAsync_WithActiveComplex_DeactivatesAndReturnsInfo()
    {
        var complex = SportsComplex.Create(
            "Club Padel",
            "Description",
            "Address",
            "City",
            null,
            null,
            "+54 11 1234 5678",
            "club@test.com");
        await _sportsComplexRepository.AddAsync(complex);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new UpdateComplexStatusCommand(complex.Id, Guid.NewGuid(), ComplexStatus.Inactive));

        Assert.NotNull(result);
        Assert.Equal(complex.Id, result.Id);
        Assert.Equal("Inactive", result.Status);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task HandleAsync_WithInactiveComplex_ActivatesAndReturnsInfo()
    {
        var complex = SportsComplex.Create(
            "Club Padel",
            "Description",
            "Address",
            "City",
            null,
            null,
            "+54 11 1234 5678",
            "club@test.com");
        complex.Deactivate();
        await _sportsComplexRepository.AddAsync(complex);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new UpdateComplexStatusCommand(complex.Id, Guid.NewGuid(), ComplexStatus.Active));

        Assert.NotNull(result);
        Assert.Equal("Active", result.Status);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentComplex_ThrowsNotFoundException()
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new UpdateComplexStatusCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            ComplexStatus.Inactive)));
    }
}
