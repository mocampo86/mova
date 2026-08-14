using Mova.Application.Common.Exceptions;
using Mova.Application.Reservations.Commands;
using Mova.Application.Reservations.Handlers;
using Mova.Domain.Entities;
using Mova.UnitTests.Application.Authentication;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Reservations;

public sealed class UpdateCancellationPolicyHandlerTests
{
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();
    private readonly FakeCancellationPolicyRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private UpdateCancellationPolicyHandler CreateHandler() =>
        new(_sportsComplexRepository, _repository, _unitOfWork);

    [Fact]
    public async Task HandleAsync_WithNewComplex_CreatesPolicy()
    {
        var complex = SportsComplex.Create(
            "Complex",
            "Description",
            "Address",
            "Montevideo",
            null,
            null,
            "+598 99 123 456",
            "complex@example.com");
        await _sportsComplexRepository.AddAsync(complex);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new UpdateCancellationPolicyCommand(complex.Id, 12, false));

        Assert.Equal(complex.Id, result.SportsComplexId);
        Assert.Equal(12, result.MinimumHours);
        Assert.False(result.AllowUserCancellation);
    }

    [Fact]
    public async Task HandleAsync_WithExistingPolicy_UpdatesPolicy()
    {
        var complex = SportsComplex.Create(
            "Complex",
            "Description",
            "Address",
            "Montevideo",
            null,
            null,
            "+598 99 123 456",
            "complex@example.com");
        await _sportsComplexRepository.AddAsync(complex);
        await _repository.AddAsync(CancellationPolicy.Create(complex.Id, 24, true));

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new UpdateCancellationPolicyCommand(complex.Id, 6, false));

        Assert.Equal(6, result.MinimumHours);
        Assert.False(result.AllowUserCancellation);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentComplex_ThrowsNotFoundException()
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new UpdateCancellationPolicyCommand(Guid.NewGuid(), 12, true)));
    }

    [Fact]
    public async Task HandleAsync_WithNegativeMinimumHours_ThrowsArgumentException()
    {
        var complex = SportsComplex.Create(
            "Complex",
            "Description",
            "Address",
            "Montevideo",
            null,
            null,
            "+598 99 123 456",
            "complex@example.com");
        await _sportsComplexRepository.AddAsync(complex);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(new UpdateCancellationPolicyCommand(complex.Id, -1, true)));
    }
}
