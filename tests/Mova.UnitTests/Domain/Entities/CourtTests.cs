using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.UnitTests.Domain.Entities;

public sealed class CourtTests
{
    [Fact]
    public void Create_WithValidData_TracksFieldsAndTimestamp()
    {
        var complexId = Guid.NewGuid();
        var court = Court.Create(complexId, "Court 1", "Indoor court", "Synthetic", true);

        Assert.NotEqual(Guid.Empty, court.Id);
        Assert.Equal(complexId, court.SportsComplexId);
        Assert.Equal(CourtStatus.Active, court.Status);
        Assert.True(court.CreatedAt > DateTime.MinValue);
        Assert.False(court.CanAcceptReservations());
    }

    [Fact]
    public void Create_WithMissingRequiredField_Throws()
    {
        Assert.Throws<ArgumentException>(() => Court.Create(Guid.NewGuid(), "", "Description", "Surface", false));
        Assert.Throws<ArgumentException>(() => Court.Create(Guid.NewGuid(), "Name", "", "Surface", false));
        Assert.Throws<ArgumentException>(() => Court.Create(Guid.NewGuid(), "Name", "Description", "", false));
    }

    [Fact]
    public void AssignSports_ReplacesAssociationsAndUpdatesTimestamp()
    {
        var court = Court.Create(Guid.NewGuid(), "Court 1", "Description", "Surface", false);
        var sportIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        court.AssignSports([sportIds[0], sportIds[1], sportIds[1], sportIds[2]]);

        Assert.Equal(sportIds, court.CourtSports.Select(x => x.SportId));
        Assert.NotNull(court.UpdatedAt);
        Assert.False(court.CanAcceptReservations());
    }

    [Fact]
    public void AssignSports_WithoutSports_Throws()
    {
        var court = Court.Create(Guid.NewGuid(), "Court 1", "Description", "Surface", false);

        Assert.Throws<ArgumentException>(() => court.AssignSports([]));
    }
}
