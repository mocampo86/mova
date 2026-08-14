using Mova.Application.Reservations.Queries;
using Mova.Application.Reservations.Validators;
using Xunit;

namespace Mova.UnitTests.Application.Reservations;

public sealed class GetReservationsByComplexQueryValidatorTests
{
    [Fact]
    public async Task Validate_WithValidValues_IsValid()
    {
        var query = new GetReservationsByComplexQuery(Guid.NewGuid(), 1, 20);
        var result = await new GetReservationsByComplexQueryValidator().ValidateAsync(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithMissingComplexId_IsInvalid()
    {
        var query = new GetReservationsByComplexQuery(Guid.Empty, 1, 20);
        var result = await new GetReservationsByComplexQueryValidator().ValidateAsync(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(GetReservationsByComplexQuery.SportsComplexId));
    }

    [Fact]
    public async Task Validate_WithZeroPage_IsInvalid()
    {
        var query = new GetReservationsByComplexQuery(Guid.NewGuid(), 0, 20);
        var result = await new GetReservationsByComplexQueryValidator().ValidateAsync(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(GetReservationsByComplexQuery.Page));
    }

    [Fact]
    public async Task Validate_WithZeroPageSize_IsInvalid()
    {
        var query = new GetReservationsByComplexQuery(Guid.NewGuid(), 1, 0);
        var result = await new GetReservationsByComplexQueryValidator().ValidateAsync(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(GetReservationsByComplexQuery.PageSize));
    }

    [Fact]
    public async Task Validate_WithPageSizeAboveMaximum_IsInvalid()
    {
        var query = new GetReservationsByComplexQuery(Guid.NewGuid(), 1, 101);
        var result = await new GetReservationsByComplexQueryValidator().ValidateAsync(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(GetReservationsByComplexQuery.PageSize));
    }
}
