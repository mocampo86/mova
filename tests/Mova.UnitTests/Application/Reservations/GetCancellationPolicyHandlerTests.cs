using Mova.Application.Common.Exceptions;
using Mova.Application.Reservations.Handlers;
using Mova.Application.Reservations.Queries;
using Mova.Domain.Entities;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Reservations;

public sealed class GetCancellationPolicyHandlerTests
{
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();

    private GetCancellationPolicyHandler CreateHandler(int minimumHours = 24, bool allowUserCancellation = true) =>
        new(_sportsComplexRepository, new FakeCancellationPolicy(minimumHours, allowUserCancellation));

    [Fact]
    public async Task HandleAsync_WithExistingComplex_ReturnsPolicyValues()
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

        var handler = CreateHandler(12, false);
        var result = await handler.HandleAsync(new GetCancellationPolicyQuery(complex.Id));

        Assert.Equal(complex.Id, result.SportsComplexId);
        Assert.Equal(12, result.MinimumHours);
        Assert.False(result.AllowUserCancellation);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentComplex_ThrowsNotFoundException()
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new GetCancellationPolicyQuery(Guid.NewGuid())));
    }
}
