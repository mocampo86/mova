using Mova.Domain.Entities;

namespace Mova.UnitTests.Domain.Entities;

public sealed class CourtRateTests
{
    private const int AllDays = 0b1111111;
    private const int Monday = 1 << (int)DayOfWeek.Monday;
    private const int Tuesday = 1 << (int)DayOfWeek.Tuesday;

    [Fact]
    public void Create_WithValidData_TracksFields()
    {
        var courtId = Guid.NewGuid();
        var rate = CourtRate.Create(courtId, Monday | Tuesday, TimeSpan.FromHours(17), TimeSpan.FromHours(22), 1200m);

        Assert.NotEqual(Guid.Empty, rate.Id);
        Assert.Equal(courtId, rate.CourtId);
        Assert.Equal(Monday | Tuesday, rate.DaysOfWeek);
        Assert.Equal(TimeSpan.FromHours(17), rate.StartTime);
        Assert.Equal(TimeSpan.FromHours(22), rate.EndTime);
        Assert.Equal(1200m, rate.AmountPerHour);
        Assert.True(rate.IsActive);
    }

    [Fact]
    public void Create_WithEndOfDay_AllowsEndTimeOf24Hours()
    {
        var rate = CourtRate.Create(Guid.NewGuid(), AllDays, TimeSpan.FromHours(20), TimeSpan.FromHours(24), 500m);
        Assert.Equal(TimeSpan.FromHours(24), rate.EndTime);
    }

    [Fact]
    public void Create_WithEmptyDaysMask_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CourtRate.Create(Guid.NewGuid(), 0, TimeSpan.FromHours(8), TimeSpan.FromHours(10), 100m));
    }

    [Fact]
    public void Create_WithDaysMaskOutOfRange_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CourtRate.Create(Guid.NewGuid(), 0b10000000, TimeSpan.FromHours(8), TimeSpan.FromHours(10), 100m));
    }

    [Fact]
    public void Create_WithEndTimeBeforeStartTime_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CourtRate.Create(Guid.NewGuid(), AllDays, TimeSpan.FromHours(18), TimeSpan.FromHours(10), 100m));
    }

    [Fact]
    public void Create_WithOvernightRange_Throws()
    {
        // Same-day-only policy: 22:00 -> 02:00 must be rejected.
        Assert.Throws<ArgumentException>(() =>
            CourtRate.Create(Guid.NewGuid(), AllDays, TimeSpan.FromHours(22), TimeSpan.FromHours(2), 100m));
    }

    [Fact]
    public void Create_WithZeroAmount_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CourtRate.Create(Guid.NewGuid(), AllDays, TimeSpan.FromHours(8), TimeSpan.FromHours(10), 0m));
    }

    [Fact]
    public void Create_WithMoreThanTwoDecimals_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CourtRate.Create(Guid.NewGuid(), AllDays, TimeSpan.FromHours(8), TimeSpan.FromHours(10), 100.555m));
    }

    [Fact]
    public void AppliesOn_ReturnsTrueOnlyForSelectedDays()
    {
        var rate = CourtRate.Create(Guid.NewGuid(), Monday | Tuesday, TimeSpan.FromHours(8), TimeSpan.FromHours(10), 100m);

        Assert.True(rate.AppliesOn(DayOfWeek.Monday));
        Assert.True(rate.AppliesOn(DayOfWeek.Tuesday));
        Assert.False(rate.AppliesOn(DayOfWeek.Wednesday));
    }

    [Fact]
    public void OverlapsWith_WithDisjointDays_ReturnsFalse()
    {
        var a = CourtRate.Create(Guid.NewGuid(), Monday, TimeSpan.FromHours(17), TimeSpan.FromHours(19), 100m);
        var b = CourtRate.Create(Guid.NewGuid(), Tuesday, TimeSpan.FromHours(17), TimeSpan.FromHours(19), 100m);

        Assert.False(a.OverlapsWith(b));
    }

    [Fact]
    public void OverlapsWith_WithAdjacentRanges_ReturnsFalse()
    {
        var a = CourtRate.Create(Guid.NewGuid(), Monday, TimeSpan.FromHours(17), TimeSpan.FromHours(18), 100m);
        var b = CourtRate.Create(Guid.NewGuid(), Monday, TimeSpan.FromHours(18), TimeSpan.FromHours(19), 100m);

        Assert.False(a.OverlapsWith(b));
        Assert.False(b.OverlapsWith(a));
    }

    [Fact]
    public void OverlapsWith_WithPartialOverlap_ReturnsTrue()
    {
        var a = CourtRate.Create(Guid.NewGuid(), Monday, TimeSpan.FromHours(17), TimeSpan.FromHours(19), 100m);
        var b = CourtRate.Create(Guid.NewGuid(), Monday, TimeSpan.FromHours(18), TimeSpan.FromHours(20), 100m);

        Assert.True(a.OverlapsWith(b));
    }

    [Fact]
    public void HasOverlap_IgnoresInactiveRates()
    {
        var a = CourtRate.Create(Guid.NewGuid(), Monday, TimeSpan.FromHours(17), TimeSpan.FromHours(19), 100m);
        var b = CourtRate.Create(Guid.NewGuid(), Monday, TimeSpan.FromHours(18), TimeSpan.FromHours(20), 100m);
        b.Deactivate();

        Assert.False(CourtRate.HasOverlap([a, b]));
    }

    [Fact]
    public void Deactivate_TogglesActiveFlag()
    {
        var rate = CourtRate.Create(Guid.NewGuid(), AllDays, TimeSpan.FromHours(8), TimeSpan.FromHours(10), 100m);
        rate.Deactivate();
        Assert.False(rate.IsActive);
        rate.Activate();
        Assert.True(rate.IsActive);
    }
}
