using Mova.Application.Complexes.Handlers;
using Mova.Application.Complexes.Queries;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Xunit;

namespace Mova.UnitTests.Application.Complexes;

public class GetComplexByIdForAdminHandlerTests
{
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();

    private GetComplexByIdForAdminHandler CreateHandler() =>
        new(_sportsComplexRepository);

    [Theory]
    [InlineData(ComplexStatus.Active)]
    [InlineData(ComplexStatus.Pending)]
    [InlineData(ComplexStatus.Inactive)]
    public async Task HandleAsync_WithComplexInAnyStatus_ReturnsInfo(ComplexStatus status)
    {
        var complex = SportsComplex.Create(
            "Club",
            "Description",
            "Address",
            "City",
            null,
            null,
            "+54 11 1234 5678",
            "test@test.com",
            utcOffsetMinutes: 0,
            status: status);
        await _sportsComplexRepository.AddAsync(complex);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetComplexByIdForAdminQuery(complex.Id));

        Assert.NotNull(result);
        Assert.Equal(complex.Id, result.Id);
        Assert.Equal(status.ToString(), result.Status);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentComplex_ReturnsNull()
    {
        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetComplexByIdForAdminQuery(Guid.NewGuid()));

        Assert.Null(result);
    }
}
