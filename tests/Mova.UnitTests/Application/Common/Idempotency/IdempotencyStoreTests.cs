using Mova.Application.Common.Idempotency;
using Mova.UnitTests.Application.Authentication;

namespace Mova.UnitTests.Application.Common.Idempotency;

public sealed class IdempotencyStoreTests
{
    private readonly FakeIdempotencyRecordRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly IdempotencyStore _store;

    public IdempotencyStoreTests()
    {
        _store = new IdempotencyStore(_repository, _unitOfWork);
    }

    [Fact]
    public async Task GetAsync_WhenNoRecordExists_ReturnsNull()
    {
        var result = await _store.GetAsync("actor", "scope", "key");

        Assert.Null(result);
    }

    [Fact]
    public async Task StoreAsync_ThenGetAsync_ReturnsStoredResponse()
    {
        await _store.StoreAsync("actor-1", "POST:/recurring", "key-1", 201, "{\"id\":\"abc\"}");

        var result = await _store.GetAsync("actor-1", "POST:/recurring", "key-1");

        Assert.NotNull(result);
        Assert.Equal(201, result.StatusCode);
        Assert.Equal("{\"id\":\"abc\"}", result.ResponseBody);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task GetAsync_ExpiredRecord_ReturnsNull()
    {
        var expiredRecord = Mova.Domain.Entities.IdempotencyRecord.Create(
            "actor-1",
            "POST:/recurring",
            "expired-key",
            201,
            "{}",
            DateTime.UtcNow.AddDays(-2),
            TimeSpan.FromHours(1));

        await _repository.AddAsync(expiredRecord);

        var result = await _store.GetAsync("actor-1", "POST:/recurring", "expired-key");

        Assert.Null(result);
    }

    [Fact]
    public async Task StoreAsync_DifferentScope_IsIsolated()
    {
        await _store.StoreAsync("actor-1", "POST:/recurring", "shared-key", 201, "recurring");
        await _store.StoreAsync("actor-1", "POST:/reservations", "shared-key", 201, "reservation");

        var recurring = await _store.GetAsync("actor-1", "POST:/recurring", "shared-key");
        var reservation = await _store.GetAsync("actor-1", "POST:/reservations", "shared-key");

        Assert.Equal("recurring", recurring!.ResponseBody);
        Assert.Equal("reservation", reservation!.ResponseBody);
    }
}
