# EPIC-06 — Reservations

## Status

Ready

## Objective

Allow users to make, view, and cancel reservations, while administrators can manage reservations manually, mark attendance, and handle conflicts.

## Scope

- Create a single reservation.
- Query reservation details.
- List user's reservations.
- List complex reservations for admins.
- Cancel reservations.
- Manual reservation creation by admins.
- Conflict prevention (no overlapping active reservations).
- Mark reservation as completed or no-show.
- Basic reservation history.
- Daily calendar view of reservations with color-coded slots and legend.

## User stories

| ID | Story |
|----|-------|
| [US-029](US-029.md) | As a user, I want to create a reservation for an available slot. |
| [US-030](US-030.md) | As a user, I want to view my upcoming reservations. |
| [US-031](US-031.md) | As a user, I want to view my reservation history. |
| [US-032](US-032.md) | As a user, I want to cancel a reservation according to the cancellation policy. |
| [US-033](US-033.md) | As an administrator, I want to see all reservations of my complex. |
| [US-034](US-034.md) | As an administrator, I want to create a reservation manually for a user. |
| [US-035](US-035.md) | As an administrator, I want to cancel a reservation if necessary. |
| [US-036](US-036.md) | As an administrator, I want to mark a reservation as completed or no-show. |
| [US-073](US-073.md) | As a complex administrator, I want to view reservations in a daily calendar with color-coded slots. |

## Acceptance criteria

- [ ] A reservation is linked to a court, a user, a complex, and a time range.
- [ ] Two active reservations cannot overlap on the same court.
- [ ] Conflict validation runs inside a transaction with concurrency protection.
- [ ] Users can cancel their own reservations before the configured deadline.
- [ ] Admins can cancel any reservation in their complex and record a reason.
- [ ] Manual reservations created by admins bypass public availability but still check conflicts.
- [ ] Status transitions are valid (e.g. Confirmed → CancelledByUser).

## Dependencies

- EPIC-02 — Identity and Access.
- EPIC-04 — Court Administration.
- EPIC-05 — Public Availability and Discovery.

## Technical notes

- `Reservation` entity: `Id`, `SportsComplexId`, `CourtId`, `UserId`, `StartAt`, `EndAt`, `Status`, `Source`, `RecurringReservationId`, `Notes`, `CreatedAt`, `CancelledAt`, `CancellationReason`.
- Status values: `Pending`, `Confirmed`, `CancelledByUser`, `CancelledByAdmin`, `Completed`, `NoShow`.
- For the MVP, reservations made by users are auto-confirmed.
- Cancellation policy is global or per-complex; default is a minimum number of hours before the slot.
- Concurrency protection: serializable transaction or advisory lock per `(CourtId, StartAt)`.
