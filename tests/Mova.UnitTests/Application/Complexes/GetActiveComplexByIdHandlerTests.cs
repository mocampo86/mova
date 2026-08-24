using Mova.Application.Complexes.Handlers;
using Mova.Application.Complexes.Queries;
using Mova.Domain.Entities;
using Xunit;

namespace Mova.UnitTests.Application.Complexes;

public class GetActiveComplexByIdHandlerTests
{
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();

    private GetActiveComplexByIdHandler CreateHandler() =>
        new(_sportsComplexRepository);

    [Fact]
    public async Task HandleAsync_WithActiveComplex_ReturnsInfo()
    {
        var complex = SportsComplex.Create(
            "Active Club",
            "Description",
            "Address",
            "City",
            null,
            null,
            "+54 11 1234 5678",
            "active@test.com");
        await _sportsComplexRepository.AddAsync(complex);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetActiveComplexByIdQuery(complex.Id));

        Assert.NotNull(result);
        Assert.Equal(complex.Id, result.Id);
        Assert.Null(result.Status);
    }

    [Fact]
    public async Task HandleAsync_WithInactiveComplex_ReturnsNull()
    {
        var complex = SportsComplex.Create(
            "Inactive Club",
            "Description",
            "Address",
            "City",
            null,
            null,
            "+54 11 1234 5678",
            "inactive@test.com");
        complex.Deactivate();
        await _sportsComplexRepository.AddAsync(complex);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetActiveComplexByIdQuery(complex.Id));

        Assert.Null(result);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentComplex_ReturnsNull()
    {
        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetActiveComplexByIdQuery(Guid.NewGuid()));

        Assert.Null(result);
    }
}
