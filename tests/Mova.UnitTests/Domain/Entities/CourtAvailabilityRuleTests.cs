using Mova.Domain.Entities;

namespace Mova.UnitTests.Domain.Entities;

public sealed class CourtAvailabilityRuleTests
{
    [Fact]
    public void Create_WithValidData_TracksFields()
    {
        var courtId = Guid.NewGuid();
        var rule = CourtAvailabilityRule.Create(courtId, DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12), 60, true);

        Assert.NotEqual(Guid.Empty, rule.Id);
        Assert.Equal(courtId, rule.CourtId);
        Assert.Equal(DayOfWeek.Monday, rule.DayOfWeek);
        Assert.Equal(TimeSpan.FromHours(8), rule.StartTime);
        Assert.Equal(TimeSpan.FromHours(12), rule.EndTime);
        Assert.Equal(60, rule.SlotDurationMinutes);
        Assert.True(rule.IsActive);
    }

    [Fact]
    public void Create_WithStartTimeEqualToEndTime_Throws()
    {
        var courtId = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => CourtAvailabilityRule.Create(courtId, DayOfWeek.Monday, TimeSpan.FromHours(12), TimeSpan.FromHours(12), 60, true));
    }

    [Fact]
    public void Create_WithSlotDurationNotFittingRange_Throws()
    {
        var courtId = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => CourtAvailabilityRule.Create(courtId, DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12), 50, true));
    }

    [Fact]
    public void Create_WithZeroSlotDuration_Throws()
    {
        var courtId = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => CourtAvailabilityRule.Create(courtId, DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12), 0, true));
    }

    [Fact]
    public void Update_ModifiesFields()
    {
        var courtId = Guid.NewGuid();
        var rule = CourtAvailabilityRule.Create(courtId, DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12), 60, true);

        rule.Update(TimeSpan.FromHours(9), TimeSpan.FromHours(13), 30, false);

        Assert.Equal(TimeSpan.FromHours(9), rule.StartTime);
        Assert.Equal(TimeSpan.FromHours(13), rule.EndTime);
        Assert.Equal(30, rule.SlotDurationMinutes);
        Assert.False(rule.IsActive);
    }
}
