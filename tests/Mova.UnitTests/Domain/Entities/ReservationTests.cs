using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.UnitTests.Domain.Entities;

public sealed class ReservationTests
{
    [Fact]
    public void Create_WithValidData_SetsCreatedAtAndUpdatedAt()
    {
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 10, 15, 0, 0, DateTimeKind.Utc),
            ReservationSource.Web,
            "Notes");

        Assert.NotEqual(Guid.Empty, reservation.Id);
        Assert.Equal(ReservationStatus.Pending, reservation.Status);
        Assert.True(reservation.CreatedAt > DateTime.MinValue);
        Assert.NotNull(reservation.UpdatedAt);
    }

    [Fact]
    public void Confirm_SetsStatusToConfirmedAndUpdatesUpdatedAt()
    {
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 10, 15, 0, 0, DateTimeKind.Utc),
            ReservationSource.Web);

        var originalUpdatedAt = reservation.UpdatedAt;

        reservation.Confirm();

        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
        Assert.NotNull(reservation.UpdatedAt);
        Assert.True(reservation.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void Cancel_SetsStatusAndCancellationDetailsAndUpdatedAt()
    {
        var start = DateTime.UtcNow.AddDays(1);
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            start,
            start.AddHours(1),
            ReservationSource.Web);
        reservation.Confirm();

        var originalUpdatedAt = reservation.UpdatedAt;

        reservation.Cancel("No longer needed");

        Assert.Equal(ReservationStatus.CancelledByUser, reservation.Status);
        Assert.Equal("No longer needed", reservation.CancellationReason);
        Assert.NotNull(reservation.CancelledAt);
        Assert.NotNull(reservation.UpdatedAt);
        Assert.True(reservation.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void MarkCompleted_SetsStatusToCompletedAndUpdatedAt()
    {
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 10, 15, 0, 0, DateTimeKind.Utc),
            ReservationSource.Web);
        reservation.Confirm();

        var originalUpdatedAt = reservation.UpdatedAt;

        reservation.MarkCompleted();

        Assert.Equal(ReservationStatus.Completed, reservation.Status);
        Assert.NotNull(reservation.UpdatedAt);
        Assert.True(reservation.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void MarkNoShow_SetsStatusToNoShowAndUpdatedAt()
    {
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 10, 15, 0, 0, DateTimeKind.Utc),
            ReservationSource.Web);
        reservation.Confirm();

        var originalUpdatedAt = reservation.UpdatedAt;

        reservation.MarkNoShow();

        Assert.Equal(ReservationStatus.NoShow, reservation.Status);
        Assert.NotNull(reservation.UpdatedAt);
        Assert.True(reservation.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void Confirm_WhenCancelled_ThrowsInvalidOperationException()
    {
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 10, 15, 0, 0, DateTimeKind.Utc),
            ReservationSource.Web);
        reservation.Confirm();
        reservation.Cancel();

        Assert.Throws<InvalidOperationException>(() => reservation.Confirm());
    }

    [Fact]
    public void Create_WithEndAtBeforeOrEqualToStartAt_ThrowsArgumentException()
    {
        var start = new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc);

        Assert.Throws<ArgumentException>(() => Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            start,
            start,
            ReservationSource.Web));
    }
}
