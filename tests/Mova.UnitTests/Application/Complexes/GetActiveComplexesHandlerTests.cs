using Mova.Application.Complexes.Handlers;
using Mova.Application.Complexes.Queries;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Xunit;

namespace Mova.UnitTests.Application.Complexes;

public class GetActiveComplexesHandlerTests
{
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();

    private GetActiveComplexesHandler CreateHandler() =>
        new(_sportsComplexRepository);

    [Fact]
    public async Task HandleAsync_WithActiveAndInactiveComplexes_ReturnsOnlyActive()
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
        var result = await handler.HandleAsync(new GetActiveComplexesQuery(1, 20));

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("Active Club", result.Items[0].Name);
        Assert.Equal("Active", result.Items[0].Status);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(1, result.TotalPages);
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
        var result = await handler.HandleAsync(new GetActiveComplexesQuery(2, 2));

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(5, result.TotalItems);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
    }

    [Fact]
    public async Task HandleAsync_WithSearch_ReturnsOnlyMatchingActiveComplexes()
    {
        await _sportsComplexRepository.AddAsync(SportsComplex.Create("North Courts", "Description", "Main Street", "Montevideo", null, null, "+598 0000 0000", "north@test.com"));
        await _sportsComplexRepository.AddAsync(SportsComplex.Create("South Courts", "Description", "Other Street", "Canelones", null, null, "+598 0000 0001", "south@test.com"));
        var inactive = SportsComplex.Create("North Inactive", "Description", "Main Street", "Montevideo", null, null, "+598 0000 0002", "inactive@test.com");
        inactive.Deactivate();
        await _sportsComplexRepository.AddAsync(inactive);

        var result = await CreateHandler().HandleAsync(new GetActiveComplexesQuery(1, 20, "monte"));

        Assert.Single(result.Items);
        Assert.Equal("North Courts", result.Items[0].Name);
        Assert.Equal(1, result.TotalItems);
    }

    [Fact]
    public async Task HandleAsync_WithNoActiveComplexes_ReturnsEmptyResult()
    {
        var inactiveComplex = SportsComplex.Create(
            "Inactive Club",
            "Description",
            "Address",
            "City",
            null,
            null,
            "+54 11 1234 5678",
            "inactive@test.com");
        inactiveComplex.Deactivate();
        await _sportsComplexRepository.AddAsync(inactiveComplex);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetActiveComplexesQuery(1, 20));

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
        Assert.Equal(0, result.TotalPages);
    }
}
