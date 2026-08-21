using Mova.Application.Complexes.Commands;
using Mova.Application.Complexes.Handlers;
using Mova.Application.Common.Exceptions;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.UnitTests.Application.Authentication;
using Xunit;

namespace Mova.UnitTests.Application.Complexes;

public class UpdateComplexHandlerTests
{
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private UpdateComplexHandler CreateHandler() =>
        new(_sportsComplexRepository, _unitOfWork);

    [Fact]
    public async Task HandleAsync_WithExistingActiveComplex_UpdatesPublicDetailsAndPreservesStatus()
    {
        var sportsComplex = SportsComplex.Create(
            "Old Name",
            "Old description",
            "Old address",
            "Old city",
            null,
            null,
            "+54 11 1111 1111",
            "old@test.com");
        await _sportsComplexRepository.AddAsync(sportsComplex);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new UpdateComplexCommand(
            sportsComplex.Id,
            Guid.NewGuid(),
            "New Name",
            "New description",
            "New address",
            "New city",
            -34.6m,
            -58.3m,
            "+54 11 9999 9999",
            "new@test.com"));

        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
        Assert.Equal("New description", result.Description);
        Assert.Equal("New address", result.Address);
        Assert.Equal("New city", result.City);
        Assert.Equal(-34.6m, result.Latitude);
        Assert.Equal(-58.3m, result.Longitude);
        Assert.Equal("+54 11 9999 9999", result.PhoneNumber);
        Assert.Equal("new@test.com", result.Email);
        Assert.Equal("Active", result.Status);
        Assert.NotNull(result.UpdatedAt);
    }

    [Theory]
    [InlineData(ComplexStatus.Pending)]
    [InlineData(ComplexStatus.Inactive)]
    public async Task HandleAsync_WithNonActiveComplex_UpdatesPublicDetailsAndPreservesStatus(ComplexStatus status)
    {
        var sportsComplex = SportsComplex.Create(
            "Old Name",
            "Old description",
            "Old address",
            "Old city",
            null,
            null,
            "+54 11 1111 1111",
            "old@test.com",
            status);
        await _sportsComplexRepository.AddAsync(sportsComplex);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new UpdateComplexCommand(
            sportsComplex.Id,
            Guid.NewGuid(),
            "New Name",
            "New description",
            "New address",
            "New city",
            -34.6m,
            -58.3m,
            "+54 11 9999 9999",
            "new@test.com"));

        Assert.NotNull(result);
        Assert.Equal(status.ToString(), result.Status);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentComplex_ThrowsNotFoundException()
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new UpdateComplexCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Name",
            "Description",
            "Address",
            "City",
            null,
            null,
            "+54 11 1234 5678",
            "email@test.com")));
    }
}
