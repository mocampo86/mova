using Mova.Application.Audit.Handlers;
using Mova.Application.Audit.Queries;
using Mova.Contracts.Audit;
using Mova.Contracts.Common;
using Mova.Domain.Entities;
using Mova.UnitTests.Application.Authentication;
using Xunit;

namespace Mova.UnitTests.Application.Audit;

public sealed class GetAuditLogsHandlerTests
{
    private readonly FakeAuditLogRepository _auditLogRepository = new();

    private GetAuditLogsHandler CreateHandler() =>
        new(_auditLogRepository);

    [Fact]
    public async Task HandleAsync_WithNoFilters_ReturnsAllItemsPaged()
    {
        _auditLogRepository.AuditLogs.Add(AuditLog.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Court.Create",
            "Court",
            Guid.NewGuid().ToString(),
            new { name = "Court 1" }));

        _auditLogRepository.AuditLogs.Add(AuditLog.Create(
            Guid.NewGuid(),
            null,
            "SportsComplex.Update",
            "SportsComplex",
            Guid.NewGuid().ToString(),
            new { name = "Complex 1" }));

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetAuditLogsQuery(), 1, 10);

        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task HandleAsync_WithActionFilter_ReturnsMatchingItems()
    {
        var complexId = Guid.NewGuid();
        _auditLogRepository.AuditLogs.Add(AuditLog.Create(
            Guid.NewGuid(),
            complexId,
            "Court.Create",
            "Court",
            Guid.NewGuid().ToString(),
            new { name = "Court 1" }));

        _auditLogRepository.AuditLogs.Add(AuditLog.Create(
            Guid.NewGuid(),
            complexId,
            "SportsComplex.Update",
            "SportsComplex",
            Guid.NewGuid().ToString(),
            new { name = "Complex 1" }));

        var handler = CreateHandler();
        var result = await handler.HandleAsync(
            new GetAuditLogsQuery(Action: "Court.Create"),
            1,
            10);

        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal("Court.Create", result.Items[0].Action);
    }

    [Fact]
    public async Task HandleAsync_WithEntityTypeFilter_ReturnsMatchingItems()
    {
        var complexId = Guid.NewGuid();
        _auditLogRepository.AuditLogs.Add(AuditLog.Create(
            Guid.NewGuid(),
            complexId,
            "Court.Create",
            "Court",
            Guid.NewGuid().ToString(),
            new { name = "Court 1" }));

        _auditLogRepository.AuditLogs.Add(AuditLog.Create(
            Guid.NewGuid(),
            complexId,
            "BlockedUser.Block",
            "BlockedUser",
            Guid.NewGuid().ToString(),
            new { }));

        var handler = CreateHandler();
        var result = await handler.HandleAsync(
            new GetAuditLogsQuery(EntityType: "BlockedUser"),
            1,
            10);

        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal("BlockedUser", result.Items[0].EntityType);
    }

    [Fact]
    public async Task HandleAsync_WithSportsComplexIdFilter_ReturnsMatchingItems()
    {
        var firstComplexId = Guid.NewGuid();
        var secondComplexId = Guid.NewGuid();

        _auditLogRepository.AuditLogs.Add(AuditLog.Create(
            Guid.NewGuid(),
            firstComplexId,
            "Court.Create",
            "Court",
            Guid.NewGuid().ToString(),
            new { name = "Court 1" }));

        _auditLogRepository.AuditLogs.Add(AuditLog.Create(
            Guid.NewGuid(),
            secondComplexId,
            "Court.Create",
            "Court",
            Guid.NewGuid().ToString(),
            new { name = "Court 2" }));

        var handler = CreateHandler();
        var result = await handler.HandleAsync(
            new GetAuditLogsQuery(SportsComplexId: secondComplexId),
            1,
            10);

        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal(secondComplexId, result.Items[0].SportsComplexId);
    }

    [Fact]
    public async Task HandleAsync_WithDateRangeFilter_ReturnsMatchingItems()
    {
        var complexId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        _auditLogRepository.AuditLogs.Add(AuditLog.Create(
            Guid.NewGuid(),
            complexId,
            "Court.Create",
            "Court",
            Guid.NewGuid().ToString(),
            new { name = "Court 1" }));

        var handler = CreateHandler();
        var result = await handler.HandleAsync(
            new GetAuditLogsQuery(
                From: now.AddHours(-1),
                To: now.AddHours(1)),
            1,
            10);

        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task HandleAsync_WithPagination_ReturnsCorrectPage()
    {
        for (var i = 0; i < 5; i++)
        {
            _auditLogRepository.AuditLogs.Add(AuditLog.Create(
                Guid.NewGuid(),
                null,
                "Reservation.Cancel",
                "Reservation",
                Guid.NewGuid().ToString(),
                new { }));
        }

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetAuditLogsQuery(), 2, 2);

        Assert.Equal(5, result.TotalItems);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void PagedResult_CalculatesTotalPages()
    {
        var result = PagedResult<AuditLogInfo>.Create([], 1, 10, 25);

        Assert.Equal(3, result.TotalPages);
    }
}
