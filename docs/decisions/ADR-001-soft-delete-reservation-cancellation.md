# ADR-001 — Use soft-delete for reservation cancellations

## Status

Accepted

## Context

When a reservation is cancelled (by a user or by an administrator) the platform must:

1. Keep a record of the cancelled reservation for history, audit, and reporting.
2. Make the court time slot available again so it can be booked by another user.
3. Distinguish between a cancellation performed by the user and one performed by an administrator.
4. Record *who* cancelled the reservation and optionally *why*, because a full `AuditLog` table is not yet implemented.

The two main alternatives were:

- **Option A**: Add a separate `CancelledReservations` table and move/delete the record there.
- **Option B**: Keep cancelled reservations in the existing `Reservations` table and use the `Status` column to mark them as cancelled.

## Decision

We will use **Option B**: a soft-delete/soft-cancel approach within the existing `Reservations` table.

The `Reservation` entity already contains the fields needed for this approach:

- `Status` with `CancelledByUser` and `CancelledByAdmin` values.
- `CancelledAt` timestamp.
- `CancellationReason` free-text field.

To satisfy the additional requirement of tracking the actor, we added a new nullable field:

- `CancelledByUserId` — the `User.Id` of the person who performed the cancellation.

A new `CancelledByUser` navigation property and foreign key relationship to `Users` were also added.

User cancellations are additionally governed by a configurable `ICancellationPolicy`. The default implementation reads from the `CancellationPolicy` configuration section, which contains:

- `MinimumHours` — how far in advance of the slot start a user can cancel.
- `AllowUserCancellation` — a global switch to disable user self-cancellation entirely.

Cancellation logic is implemented in the domain entity (`Reservation.Cancel`) and is invoked by dedicated handlers:

- `CancelMyReservationHandler` for users, which enforces the configured cancellation policy.
- `CancelReservationHandler` for administrators, which does not enforce the user deadline.

## Consequences

### Positive

- No new table or migration is required beyond the new `CancelledByUserId` column.
- All existing reservation queries can continue to use the `Reservations` table.
- Availability/conflict detection naturally excludes cancelled rows by filtering on `Status`.
- History, dashboard, and reporting can include cancelled reservations without joins to a separate table.
- The actor (`CancelledByUserId`) and reason (`CancellationReason`) are stored close to the reservation data.

### Negative

- The `Reservations` table will grow over time because cancelled rows are never deleted.
- Indexes and queries that do not filter by `Status` may scan more rows.
- The `Reservation` entity gains an additional relationship to `Users` (`CancelledByUser`).

### Risks

- Long-term table growth may require archival or partitioning in the future.
- Any new query that forgets to exclude `CancelledByUser`/`CancelledByAdmin` could incorrectly treat cancelled reservations as active. This is mitigated by repository methods such as `GetActiveForCourtAsync` and `HasOverlappingActiveReservationAsync` that explicitly filter by status.

## Alternatives considered

- **Option A — Separate `CancelledReservations` table**: Rejected because it would require duplicating reservation schema, moving data, and complicating queries for history and availability. It would also force the frontend and backend to read from two tables for a complete view.
- **Option C — Hard delete with an `AuditLog` entry**: Rejected because `AuditLog` is not yet implemented and the product requires cancelled reservations to remain visible in user and admin history.

## Related decisions

- `DOMAIN-MODEL.md`: reservation statuses and business rules.
- `DATABASE-DESIGN.md`: `Reservations` table schema.

## References

- `US-032 — Cancel a reservation according to the cancellation policy`
- `US-035 — Cancel a reservation if necessary`
- `US-039 — Cancel one occurrence without affecting the rest`
- `US-040 — Cancel the entire recurring series`
