# EPIC-04 — Court Administration

## Status

Ready

## Objective

Allow complex administrators to create and manage courts, assign sports, set business hours, and configure slot duration.

## Scope

- CRUD for `Court`.
- Association of one or more `Sport` to a court.
- `CourtAvailabilityRule` per day of week.
- `BusinessHours` per complex.
- Activation/deactivation of courts.
- Preparation for court-level time blocks.

## User stories

| ID | Story |
|----|-------|
| [US-018](US-018.md) | As an administrator, I want to create a court with name, description, surface, and indoor/outdoor flag. |
| [US-019](US-019.md) | As an administrator, I want to assign one or more sports to a court. |
| [US-020](US-020.md) | As an administrator, I want to configure the days and hours a court is available. |
| [US-021](US-021.md) | As an administrator, I want to set the duration of each booking slot per court. |
| [US-022](US-022.md) | As an administrator, I want to activate or deactivate a court. |

## Acceptance criteria

- [ ] Courts belong to one complex and display under it.
- [ ] A court must have at least one active sport before it can accept reservations.
- [ ] Availability rules define day of week, start/end time, and slot duration.
- [ ] Business hours can be configured per complex.
- [ ] Inactive courts do not appear in public availability queries.

## Dependencies

- EPIC-03 — Sports Complex Administration.

## Technical notes

- `Court`: `Id`, `SportsComplexId`, `Name`, `Description`, `SurfaceType`, `Indoor`, `Status`, `CreatedAt`, `UpdatedAt`.
- `CourtSport` many-to-many join table.
- `CourtAvailabilityRule`: `CourtId`, `DayOfWeek`, `StartTime`, `EndTime`, `SlotDurationMinutes`, `IsActive`.
- Validate `StartTime < EndTime` and that slots fit within the range.
