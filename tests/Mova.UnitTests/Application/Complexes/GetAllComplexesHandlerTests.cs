using Mova.Application.Complexes.Handlers;
using Mova.Application.Complexes.Queries;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Xunit;

namespace Mova.UnitTests.Application.Complexes;

public class GetAllComplexesHandlerTests
{
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();

    private GetAllComplexesHandler CreateHandler() =>
        new(_sportsComplexRepository);

    [Fact]
    public async Task HandleAsync_WithActiveAndInactiveComplexes_ReturnsAll()
    {
        var activeComplex = SportsComplex.Create(
            "Active Club",
            "An active complex",
            "Av. Libertador 1234",
            "Buenos Aires",
            null,
            null,
            "+54 11 1234 5678",
            "active@test.com");

        var inactiveComplex = SportsComplex.Create(
            "Inactive Club",
            "An inactive complex",
            "Av. Libertador 5678",
            "Buenos Aires",
            null,
            null,
            "+54 11 8765 4321",
            "inactive@test.com");
        inactiveComplex.Deactivate();

        await _sportsComplexRepository.AddAsync(activeComplex);
        await _sportsComplexRepository.AddAsync(inactiveComplex);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetAllComplexesQuery(1, 20));

        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalItems);
        Assert.Contains(result.Items, c => c.Status == "Active");
        Assert.Contains(result.Items, c => c.Status == "Inactive");
    }

    [Fact]
    public async Task HandleAsync_WithMultiplePages_ReturnsCorrectPage()
    {
        for (var i = 0; i < 5; i++)
        {
            var complex = SportsComplex.Create(
                $"Club {i}",
                "Description",
                "Address",
                "City",
                null,
                null,
                "+54 11 1234 5678",
                $"club{i}@test.com");
            await _sportsComplexRepository.AddAsync(complex);
        }

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetAllComplexesQuery(2, 2));

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(5, result.TotalItems);
        Assert.Equal(3, result.TotalPages);
    }
}
