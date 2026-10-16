using Mova.Application.Pricing;
using Mova.Domain.Entities;
using Mova.Domain.Helpers;

namespace Mova.UnitTests.Application.Pricing;

public sealed class PriceCalculatorTests
{
    private const int AllDays = 0b1111111;
    private const int Monday = 1 << (int)DayOfWeek.Monday;

    private static readonly TimeZoneInfo Tz = TimeZoneConverter.GetTimeZone("America/Montevideo");

    private static DateTime LocalToUtc(int year, int month, int day, int hour, int minute)
    {
        var local = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(local, Tz);
    }

    private static CourtRate Rate(int days, int startHour, int endHour, decimal amount, bool isActive = true) =>
        CourtRate.Create(Guid.NewGuid(), days, TimeSpan.FromHours(startHour), TimeSpan.FromHours(endHour), amount, isActive);

    [Fact]
    public void Calculate_WithSingleCoveringRate_ReturnsProportionalAmount()
    {
        var rates = new[] { Rate(AllDays, 17, 19, 1200m) };
        var start = LocalToUtc(2026, 8, 10, 17, 0); // Monday 17:00 local
        var end = LocalToUtc(2026, 8, 10, 19, 0);

        var result = PriceCalculator.Calculate(rates, Tz, start, end);

        Assert.True(result.IsFullyCovered);
        Assert.Equal(2400m, result.Total);
        var line = Assert.Single(result.Lines);
        Assert.Equal(2400m, line.Amount);
        Assert.Equal(1200m, line.HourlyRate);
    }

    [Fact]
    public void Calculate_SpanningTwoRates_SumsProportionalSegments()
    {
        // Spec example: 17:00-18:00 @1200 + 18:00-19:00 @1800 = 3000.
        var rates = new[]
        {
            Rate(AllDays, 17, 18, 1200m),
            Rate(AllDays, 18, 19, 1800m)
        };
        var start = LocalToUtc(2026, 8, 10, 17, 0);
        var end = LocalToUtc(2026, 8, 10, 19, 0);

        var result = PriceCalculator.Calculate(rates, Tz, start, end);

        Assert.True(result.IsFullyCovered);
        Assert.Equal(3000m, result.Total);
        Assert.Equal(2, result.Lines.Count);
        Assert.Equal(1200m, result.Lines[0].Amount);
        Assert.Equal(1800m, result.Lines[1].Amount);
    }

    [Fact]
    public void Calculate_WithPartialSegments_UsesMinuteProportions()
    {
        // 17:30-19:00 => 30 min @1200 (600) + 60 min @1800 (1800) = 2400.
        var rates = new[]
        {
            Rate(AllDays, 17, 18, 1200m),
            Rate(AllDays, 18, 19, 1800m)
        };
        var start = LocalToUtc(2026, 8, 10, 17, 30);
        var end = LocalToUtc(2026, 8, 10, 19, 0);

        var result = PriceCalculator.Calculate(rates, Tz, start, end);

        Assert.True(result.IsFullyCovered);
        Assert.Equal(2400m, result.Total);
        Assert.Equal(600m, result.Lines[0].Amount);
        Assert.Equal(1800m, result.Lines[1].Amount);
    }

    [Fact]
    public void Calculate_WithNoRate_ReturnsFullRangeAsGap()
    {
        var start = LocalToUtc(2026, 8, 10, 17, 0);
        var end = LocalToUtc(2026, 8, 10, 18, 0);

        var result = PriceCalculator.Calculate(Array.Empty<CourtRate>(), Tz, start, end);

        Assert.False(result.IsFullyCovered);
        Assert.Equal(0m, result.Total);
        var gap = Assert.Single(result.Gaps);
        Assert.Equal(start, gap.StartAt);
        Assert.Equal(end, gap.EndAt);
    }

    [Fact]
    public void Calculate_WithPartialCoverage_ReportsUncoveredGap()
    {
        // Rate covers 17:00-18:00; reservation runs 17:00-19:00 => gap 18:00-19:00.
        var rates = new[] { Rate(AllDays, 17, 18, 1200m) };
        var start = LocalToUtc(2026, 8, 10, 17, 0);
        var end = LocalToUtc(2026, 8, 10, 19, 0);
        var expectedGapStart = LocalToUtc(2026, 8, 10, 18, 0);

        var result = PriceCalculator.Calculate(rates, Tz, start, end);

        Assert.False(result.IsFullyCovered);
        var gap = Assert.Single(result.Gaps);
        Assert.Equal(expectedGapStart, gap.StartAt);
        Assert.Equal(end, gap.EndAt);
    }

    [Fact]
    public void Calculate_OnWrongWeekday_ReturnsGap()
    {
        // Monday-only rate; reservation is on Tuesday (2026-08-11).
        var rates = new[] { Rate(Monday, 17, 19, 1200m) };
        var start = LocalToUtc(2026, 8, 11, 17, 0);
        var end = LocalToUtc(2026, 8, 11, 18, 0);

        var result = PriceCalculator.Calculate(rates, Tz, start, end);

        Assert.False(result.IsFullyCovered);
        Assert.Single(result.Gaps);
    }

    [Fact]
    public void Calculate_CrossingMidnight_UsesRatesOfEachLocalDay()
    {
        // Monday 23:00-24:00 and Tuesday 00:00-01:00; reservation Monday 23:00 -> Tuesday 01:00 local.
        var rates = new[]
        {
            Rate(AllDays, 23, 24, 1000m),
            Rate(AllDays, 0, 1, 1500m)
        };
        var start = LocalToUtc(2026, 8, 10, 23, 0);
        var end = LocalToUtc(2026, 8, 11, 1, 0);

        var result = PriceCalculator.Calculate(rates, Tz, start, end);

        Assert.True(result.IsFullyCovered);
        Assert.Equal(2500m, result.Total);
        Assert.Equal(2, result.Lines.Count);
    }

    [Fact]
    public void Calculate_RateEndingAt24_CoversLastSlotOfDay()
    {
        var rates = new[] { CourtRate.Create(Guid.NewGuid(), AllDays, TimeSpan.FromHours(22), TimeSpan.FromHours(24), 800m) };
        var start = LocalToUtc(2026, 8, 10, 23, 0);
        var end = LocalToUtc(2026, 8, 10, 23, 30);

        var result = PriceCalculator.Calculate(rates, Tz, start, end);

        Assert.True(result.IsFullyCovered);
        Assert.Equal(400m, result.Total);
    }

    [Fact]
    public void Calculate_WithInactiveRate_TreatsRangeAsUncovered()
    {
        var rates = new[] { Rate(AllDays, 17, 19, 1200m, isActive: false) };
        var start = LocalToUtc(2026, 8, 10, 17, 0);
        var end = LocalToUtc(2026, 8, 10, 18, 0);

        var result = PriceCalculator.Calculate(rates, Tz, start, end);

        Assert.False(result.IsFullyCovered);
        Assert.Single(result.Gaps);
    }

    [Fact]
    public void Calculate_RoundsAmountToTwoDecimalsAwayFromZero()
    {
        // 100/h for 1 minute => 1.6666.. => 1.67.
        var rates = new[] { Rate(AllDays, 17, 19, 100m) };
        var start = LocalToUtc(2026, 8, 10, 17, 0);
        var end = start.AddMinutes(1);

        var result = PriceCalculator.Calculate(rates, Tz, start, end);

        Assert.True(result.IsFullyCovered);
        Assert.Equal(1.67m, result.Total);
    }

    [Fact]
    public void Calculate_TotalEqualsSumOfRoundedLineAmounts()
    {
        var rates = new[] { Rate(AllDays, 17, 19, 100m) };
        var start = LocalToUtc(2026, 8, 10, 17, 0);
        var end = start.AddMinutes(31);

        var result = PriceCalculator.Calculate(rates, Tz, start, end);

        Assert.True(result.IsFullyCovered);
        Assert.Equal(result.Lines.Sum(l => l.Amount), result.Total);
    }

    [Fact]
    public void Calculate_WithInvalidRange_Throws()
    {
        var start = LocalToUtc(2026, 8, 10, 17, 0);

        Assert.Throws<ArgumentException>(() =>
            PriceCalculator.Calculate(Array.Empty<CourtRate>(), Tz, start, start));
    }
}
