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
        var userId = Guid.NewGuid();
        var start = DateTime.UtcNow.AddDays(1);
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            userId,
            start,
            start.AddHours(1),
            ReservationSource.Web);
        reservation.Confirm();

        var originalUpdatedAt = reservation.UpdatedAt;

        reservation.Cancel(userId, false, "No longer needed");

        Assert.Equal(ReservationStatus.CancelledByUser, reservation.Status);
        Assert.Equal("No longer needed", reservation.CancellationReason);
        Assert.Equal(userId, reservation.CancelledByUserId);
        Assert.NotNull(reservation.CancelledAt);
        Assert.NotNull(reservation.UpdatedAt);
        Assert.True(reservation.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void Cancel_ByAdmin_SetsStatusToCancelledByAdmin()
    {
        var adminUserId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            ReservationSource.Web);

        reservation.Cancel(adminUserId, true, "No show");

        Assert.Equal(ReservationStatus.CancelledByAdmin, reservation.Status);
        Assert.Equal(adminUserId, reservation.CancelledByUserId);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_IsIdempotent()
    {
        var userId = Guid.NewGuid();
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            ReservationSource.Web);

        reservation.Cancel(userId, false, "Changed plans");
        var firstCancelledAt = reservation.CancelledAt;
        var firstUpdatedAt = reservation.UpdatedAt;

        reservation.Cancel(Guid.NewGuid(), false, "Other reason");

        Assert.Equal(ReservationStatus.CancelledByUser, reservation.Status);
        Assert.Equal("Changed plans", reservation.CancellationReason);
        Assert.Equal(firstCancelledAt, reservation.CancelledAt);
        Assert.Equal(firstUpdatedAt, reservation.UpdatedAt);
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
        var userId = Guid.NewGuid();
        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            userId,
            new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 10, 15, 0, 0, DateTimeKind.Utc),
            ReservationSource.Web);
        reservation.Confirm();
        reservation.Cancel(userId);

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
