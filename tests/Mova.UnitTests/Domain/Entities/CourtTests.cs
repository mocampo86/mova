using System.Reflection;
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

    [Fact]
    public void Activate_WithInactiveCourt_SetsStatusActiveAndUpdatesTimestamp()
    {
        var court = Court.Create(Guid.NewGuid(), "Court 1", "Description", "Surface", false);
        court.Deactivate();
        court.Activate();

        Assert.Equal(CourtStatus.Active, court.Status);
        Assert.NotNull(court.UpdatedAt);
    }

    [Fact]
    public void Activate_WithAlreadyActiveCourt_IsIdempotent()
    {
        var court = Court.Create(Guid.NewGuid(), "Court 1", "Description", "Surface", false);
        var updatedAt = court.UpdatedAt;

        court.Activate();

        Assert.Equal(CourtStatus.Active, court.Status);
        Assert.Equal(updatedAt, court.UpdatedAt);
    }

    [Fact]
    public void Deactivate_WithActiveCourt_SetsStatusInactiveAndUpdatesTimestamp()
    {
        var court = Court.Create(Guid.NewGuid(), "Court 1", "Description", "Surface", false);

        court.Deactivate();

        Assert.Equal(CourtStatus.Inactive, court.Status);
        Assert.NotNull(court.UpdatedAt);
    }

    [Fact]
    public void Deactivate_WithAlreadyInactiveCourt_IsIdempotent()
    {
        var court = Court.Create(Guid.NewGuid(), "Court 1", "Description", "Surface", false);
        court.Deactivate();
        var updatedAt = court.UpdatedAt;

        court.Deactivate();

        Assert.Equal(CourtStatus.Inactive, court.Status);
        Assert.Equal(updatedAt, court.UpdatedAt);
    }

    [Fact]
    public void CanAcceptReservations_WhenActiveWithActiveSport_ReturnsTrue()
    {
        var court = Court.Create(Guid.NewGuid(), "Court 1", "Description", "Surface", false);
        var sport = Sport.Create("Football");
        court.AssignSports([sport.Id]);
        SetSportOnCourtSports(court, sport);
        court.Activate();

        Assert.True(court.CanAcceptReservations());
    }

    [Fact]
    public void CanAcceptReservations_WhenInactive_ReturnsFalse()
    {
        var court = Court.Create(Guid.NewGuid(), "Court 1", "Description", "Surface", false);
        var sport = Sport.Create("Football");
        court.AssignSports([sport.Id]);
        SetSportOnCourtSports(court, sport);
        court.Deactivate();

        Assert.False(court.CanAcceptReservations());
    }

    [Fact]
    public void CanAcceptReservations_WhenActiveWithInactiveSport_ReturnsFalse()
    {
        var court = Court.Create(Guid.NewGuid(), "Court 1", "Description", "Surface", false);
        var sport = Sport.Create("Football");
        typeof(Sport).GetProperty("Status", BindingFlags.Instance | BindingFlags.Public)!.SetValue(sport, SportStatus.Inactive);
        court.AssignSports([sport.Id]);
        SetSportOnCourtSports(court, sport);

        Assert.False(court.CanAcceptReservations());
    }

    [Fact]
    public void CanAcceptReservations_WhenActiveWithoutSports_ReturnsFalse()
    {
        var court = Court.Create(Guid.NewGuid(), "Court 1", "Description", "Surface", false);

        Assert.False(court.CanAcceptReservations());
    }

    [Fact]
    public void Update_WithValidData_UpdatesFieldsAndTimestamp()
    {
        var court = Court.Create(Guid.NewGuid(), "Court 1", "Description", "Surface", false);

        court.Update("Updated Court", "Updated description", "Grass", true);

        Assert.Equal("Updated Court", court.Name);
        Assert.Equal("Updated description", court.Description);
        Assert.Equal("Grass", court.SurfaceType);
        Assert.True(court.Indoor);
        Assert.NotNull(court.UpdatedAt);
    }

    [Fact]
    public void Update_WithSports_ReplacesAssociationsAndUpdatesTimestamp()
    {
        var court = Court.Create(Guid.NewGuid(), "Court 1", "Description", "Surface", false);
        var sportIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        court.Update("Court 1", "Description", "Surface", false, sportIds);

        Assert.Equal(sportIds, court.CourtSports.Select(x => x.SportId));
        Assert.NotNull(court.UpdatedAt);
    }

    [Fact]
    public void Update_WithEmptySports_ClearsAssociations()
    {
        var football = Sport.Create("Football");
        var court = Court.Create(Guid.NewGuid(), "Court 1", "Description", "Surface", false, [football.Id]);

        court.Update("Court 1", "Description", "Surface", false, []);

        Assert.Empty(court.CourtSports);
    }

    [Fact]
    public void Update_WithNullSportIds_LeavesAssociationsUnchanged()
    {
        var football = Sport.Create("Football");
        var court = Court.Create(Guid.NewGuid(), "Court 1", "Description", "Surface", false, [football.Id]);

        court.Update("Court 1", "Description", "Surface", false, null);

        Assert.Single(court.CourtSports);
        Assert.Equal(football.Id, court.CourtSports.Single().SportId);
    }

    [Fact]
    public void Update_WithMissingRequiredField_Throws()
    {
        var court = Court.Create(Guid.NewGuid(), "Court 1", "Description", "Surface", false);

        Assert.Throws<ArgumentException>(() => court.Update("", "Description", "Surface", false));
        Assert.Throws<ArgumentException>(() => court.Update("Name", "", "Surface", false));
        Assert.Throws<ArgumentException>(() => court.Update("Name", "Description", "", false));
    }

    private static void SetSportOnCourtSports(Court court, Sport sport)
    {
        var property = typeof(CourtSport).GetProperty("Sport", BindingFlags.Instance | BindingFlags.Public)!;
        property.SetValue(court.CourtSports.Single(), sport);
    }
}
