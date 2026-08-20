# EPIC-07 — Recurring Reservations

## Status

Ready

## Objective

Allow complex administrators and users to create weekly recurring reservations, generate individual occurrences, and manage cancellations at the occurrence or series level.

## Scope

- Create a weekly recurring reservation rule.
- Validate availability across the recurrence period.
- Generate individual `Reservation` occurrences.
- Cancel a single occurrence.
- Cancel the entire series.
- Modify future occurrences without affecting history.
- Detect conflicts during generation.

## User stories

| ID | Story |
|----|-------|
| [US-037](US-037.md) | As a user, I want to book a fixed weekly slot for a period I define. |
| [US-038](US-038.md) | As an administrator, I want to create recurring reservations for customers. |
| [US-039](US-039.md) | As a user or administrator, I want to cancel one occurrence without affecting the rest. |
| [US-040](US-040.md) | As a user or administrator, I want to cancel the entire recurring series. |
| [US-041](US-041.md) | As a system, I want to detect conflicts when generating recurring reservations. |
| [US-076](US-076.md) | As a complex administrator, I want to enable or disable recurring reservations for users. |
| [US-077](US-077.md) | As a complex administrator, I want to discover and manage recurring reservations, including cancelling an entire series. |

## Acceptance criteria

- [ ] A recurring reservation has `DayOfWeek`, `StartTime`, `DurationMinutes`, `StartDate`, and `EndDate`.
- [ ] The recurrence period has an end date or a maximum number of weeks.
- [ ] Individual reservations are generated for each occurrence.
- [ ] Generated occurrences respect existing reservations and blocks.
- [ ] Cancelling a series sets all future occurrences to `CancelledByUser` or `CancelledByAdmin`.
- [ ] Cancelling an occurrence sets only that reservation.
- [ ] Modifying the rule affects future occurrences only.

## Dependencies

- EPIC-06 — Reservations.

## Technical notes

- `RecurringReservation` entity: `Id`, `SportsComplexId`, `CourtId`, `UserId`, `DayOfWeek`, `StartTime`, `DurationMinutes`, `StartDate`, `EndDate`, `Status`, `CreatedAt`.
- Each occurrence creates a `Reservation` with `Source = Recurring` and `RecurringReservationId` populated.
- Generation must be transactional and conflict-aware.
- Consider a background or scheduled task if generation becomes expensive; for the MVP, synchronous generation is acceptable for reasonable ranges.
