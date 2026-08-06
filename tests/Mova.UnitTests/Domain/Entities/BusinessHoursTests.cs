using Mova.Domain.Entities;

namespace Mova.UnitTests.Domain.Entities;

public sealed class BusinessHoursTests
{
    [Fact]
    public void Create_WithValidData_TracksFields()
    {
        var complexId = Guid.NewGuid();
        var hours = BusinessHours.Create(complexId, DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(22), false);

        Assert.NotEqual(Guid.Empty, hours.Id);
        Assert.Equal(complexId, hours.SportsComplexId);
        Assert.Equal(DayOfWeek.Monday, hours.DayOfWeek);
        Assert.Equal(TimeSpan.FromHours(8), hours.OpeningTime);
        Assert.Equal(TimeSpan.FromHours(22), hours.ClosingTime);
        Assert.False(hours.IsClosed);
    }

    [Fact]
    public void Create_WithClosedDay_DoesNotValidateTimeOrder()
    {
        var complexId = Guid.NewGuid();
        var hours = BusinessHours.Create(complexId, DayOfWeek.Sunday, TimeSpan.FromHours(22), TimeSpan.FromHours(8), true);

        Assert.True(hours.IsClosed);
    }

    [Fact]
    public void Create_WithOpeningTimeAfterClosingTime_Throws()
    {
        var complexId = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => BusinessHours.Create(complexId, DayOfWeek.Monday, TimeSpan.FromHours(22), TimeSpan.FromHours(8), false));
    }

    [Fact]
    public void Update_ModifiesFields()
    {
        var complexId = Guid.NewGuid();
        var hours = BusinessHours.Create(complexId, DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(22), false);

        hours.Update(TimeSpan.FromHours(9), TimeSpan.FromHours(21), false);

        Assert.Equal(TimeSpan.FromHours(9), hours.OpeningTime);
        Assert.Equal(TimeSpan.FromHours(21), hours.ClosingTime);
    }

    [Fact]
    public void Update_WithSameOpenAndCloseTime_Throws()
    {
        var complexId = Guid.NewGuid();
        var hours = BusinessHours.Create(complexId, DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(22), false);

        Assert.Throws<ArgumentException>(() => hours.Update(TimeSpan.FromHours(10), TimeSpan.FromHours(10), false));
    }
}
