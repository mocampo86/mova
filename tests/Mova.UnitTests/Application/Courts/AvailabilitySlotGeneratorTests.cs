using Mova.Application.Courts;
using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.UnitTests.Application.Courts;

public sealed class AvailabilitySlotGeneratorTests
{
    [Fact]
    public void GenerateSlots_WithRuleAndBusinessHours_ReturnsSlots()
    {
        var courtId = Guid.NewGuid();
        var date = new DateOnly(2026, 8, 10);
        var rule = CourtAvailabilityRule.Create(courtId, DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12), 60, true);
        var businessHours = BusinessHours.Create(Guid.NewGuid(), DayOfWeek.Monday, TimeSpan.FromHours(9), TimeSpan.FromHours(22), false);

        var slots = AvailabilitySlotGenerator.GenerateSlots(courtId, date, rule, businessHours, [], []).ToList();

        Assert.Equal(3, slots.Count);
        Assert.All(slots, s => Assert.Equal(courtId, s.CourtId));
        Assert.Equal(new DateTime(2026, 8, 10, 9, 0, 0, DateTimeKind.Utc), slots[0].StartAt);
        Assert.Equal(new DateTime(2026, 8, 10, 10, 0, 0, DateTimeKind.Utc), slots[0].EndAt);
        Assert.Equal(new DateTime(2026, 8, 10, 12, 0, 0, DateTimeKind.Utc), slots[2].EndAt);
    }

    [Fact]
    public void GenerateSlots_WhenBusinessHoursAreClosed_ReturnsEmpty()
    {
        var courtId = Guid.NewGuid();
        var date = new DateOnly(2026, 8, 10);
        var rule = CourtAvailabilityRule.Create(courtId, DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12), 60, true);
        var businessHours = BusinessHours.Create(Guid.NewGuid(), DayOfWeek.Monday, TimeSpan.FromHours(0), TimeSpan.FromHours(0), true);

        var slots = AvailabilitySlotGenerator.GenerateSlots(courtId, date, rule, businessHours, [], []);

        Assert.Empty(slots);
    }

    [Fact]
    public void GenerateSlots_WhenRuleOutsideBusinessHours_ReturnsIntersectedSlots()
    {
        var courtId = Guid.NewGuid();
        var date = new DateOnly(2026, 8, 10);
        var rule = CourtAvailabilityRule.Create(courtId, DayOfWeek.Monday, TimeSpan.FromHours(7), TimeSpan.FromHours(10), 60, true);
        var businessHours = BusinessHours.Create(Guid.NewGuid(), DayOfWeek.Monday, TimeSpan.FromHours(9), TimeSpan.FromHours(22), false);

        var slots = AvailabilitySlotGenerator.GenerateSlots(courtId, date, rule, businessHours, [], []).ToList();

        Assert.Single(slots);
        Assert.Equal(new DateTime(2026, 8, 10, 9, 0, 0, DateTimeKind.Utc), slots[0].StartAt);
        Assert.Equal(new DateTime(2026, 8, 10, 10, 0, 0, DateTimeKind.Utc), slots[0].EndAt);
    }

    [Fact]
    public void GenerateSlots_WithOverlappingReservation_RemovesSlot()
    {
        var courtId = Guid.NewGuid();
        var complexId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var date = new DateOnly(2026, 8, 10);
        var rule = CourtAvailabilityRule.Create(courtId, DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12), 60, true);
        var businessHours = BusinessHours.Create(complexId, DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(22), false);
        var reservation = Reservation.Create(complexId, courtId, userId, new DateTime(2026, 8, 10, 9, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 10, 10, 0, 0, DateTimeKind.Utc), ReservationSource.Web);

        var slots = AvailabilitySlotGenerator.GenerateSlots(courtId, date, rule, businessHours, [reservation], []);

        Assert.Equal(3, slots.Count);
        Assert.DoesNotContain(slots, s => s.StartAt == new DateTime(2026, 8, 10, 9, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void GenerateSlots_WithOverlappingCourtBlock_RemovesSlot()
    {
        var courtId = Guid.NewGuid();
        var complexId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var date = new DateOnly(2026, 8, 10);
        var rule = CourtAvailabilityRule.Create(courtId, DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12), 60, true);
        var businessHours = BusinessHours.Create(complexId, DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(22), false);
        var block = CourtBlock.Create(complexId, courtId, new DateTime(2026, 8, 10, 10, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 10, 11, 0, 0, DateTimeKind.Utc), userId);

        var slots = AvailabilitySlotGenerator.GenerateSlots(courtId, date, rule, businessHours, [], [block]);

        Assert.Equal(3, slots.Count);
        Assert.DoesNotContain(slots, s => s.StartAt == new DateTime(2026, 8, 10, 10, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void GenerateSlots_WithCancelledReservation_DoesNotRemoveSlot()
    {
        var courtId = Guid.NewGuid();
        var complexId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var date = new DateOnly(2026, 8, 10);
        var rule = CourtAvailabilityRule.Create(courtId, DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12), 60, true);
        var businessHours = BusinessHours.Create(complexId, DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(22), false);
        var reservation = Reservation.Create(complexId, courtId, userId, new DateTime(2026, 8, 10, 9, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 10, 10, 0, 0, DateTimeKind.Utc), ReservationSource.Web);
        reservation.Cancel(userId);

        var slots = AvailabilitySlotGenerator.GenerateSlots(courtId, date, rule, businessHours, [reservation], []);

        Assert.Equal(4, slots.Count);
    }

    [Fact]
    public void GenerateSlots_WhenNoRuleForDay_ReturnsEmpty()
    {
        var courtId = Guid.NewGuid();
        var date = new DateOnly(2026, 8, 11); // Tuesday
        var rule = CourtAvailabilityRule.Create(courtId, DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12), 60, true);
        var businessHours = BusinessHours.Create(Guid.NewGuid(), DayOfWeek.Tuesday, TimeSpan.FromHours(8), TimeSpan.FromHours(22), false);

        var slots = AvailabilitySlotGenerator.GenerateSlots(courtId, date, rule, businessHours, [], []);

        Assert.Empty(slots);
    }

    [Fact]
    public void GenerateSlots_WithCrossMidnightRule_GeneratesSlotsAcrossMidnight()
    {
        var courtId = Guid.NewGuid();
        var date = new DateOnly(2026, 8, 10); // Monday
        var rule = CourtAvailabilityRule.Create(courtId, DayOfWeek.Monday, TimeSpan.FromHours(22), TimeSpan.FromHours(2), 60, true);
        var businessHours = BusinessHours.Create(Guid.NewGuid(), DayOfWeek.Monday, TimeSpan.FromHours(20), TimeSpan.FromHours(4), false);

        var slots = AvailabilitySlotGenerator.GenerateSlots(courtId, date, rule, businessHours, [], []).ToList();

        Assert.Equal(4, slots.Count);
        Assert.Equal(new DateTime(2026, 8, 10, 22, 0, 0, DateTimeKind.Utc), slots[0].StartAt);
        Assert.Equal(new DateTime(2026, 8, 11, 0, 0, 0, DateTimeKind.Utc), slots[2].StartAt);
        Assert.Equal(new DateTime(2026, 8, 11, 2, 0, 0, DateTimeKind.Utc), slots[3].EndAt);
    }

    [Fact]
    public void GenerateSlots_WithUtcOffset_AdjustsSlotTimes()
    {
        var courtId = Guid.NewGuid();
        var date = new DateOnly(2026, 8, 10); // Monday
        var rule = CourtAvailabilityRule.Create(courtId, DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12), 60, true);
        var businessHours = BusinessHours.Create(Guid.NewGuid(), DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(22), false);

        var slots = AvailabilitySlotGenerator.GenerateSlots(courtId, date, rule, businessHours, [], [], 180).ToList();

        Assert.Equal(4, slots.Count);
        Assert.Equal(new DateTime(2026, 8, 10, 11, 0, 0, DateTimeKind.Utc), slots[0].StartAt);
        Assert.Equal(new DateTime(2026, 8, 10, 12, 0, 0, DateTimeKind.Utc), slots[0].EndAt);
        Assert.Equal(new DateTime(2026, 8, 10, 15, 0, 0, DateTimeKind.Utc), slots[3].EndAt);
    }
}
