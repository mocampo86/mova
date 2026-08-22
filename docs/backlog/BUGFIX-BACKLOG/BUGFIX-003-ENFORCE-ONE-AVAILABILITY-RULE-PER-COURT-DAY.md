# BUGFIX-003 — Enforce one availability rule per court and day of week

## Status

Ready

## Priority

Medium

## Source

EPIC-04 review finding: **MEDIUM — Duplicate `CourtAvailabilityRule` per `(CourtId, DayOfWeek)` is not prevented.**

## Problem

`CourtAvailabilityRuleConfiguration` defines a non-unique index on `(CourtId, DayOfWeek)`. `UpdateCourtAvailabilityRulesHandler` removes all existing rules for a court and then adds the new set without validating that each day of week appears only once. The public availability query uses `FirstOrDefault` for the requested day, so if duplicates exist, one rule is silently ignored and the effective availability becomes non-deterministic.

## Objective

Ensure at most one `CourtAvailabilityRule` exists for each `(CourtId, DayOfWeek)` combination and that the update API rejects payloads containing duplicate days.

## Scope

- `CourtAvailabilityRuleConfiguration` unique index.
- `UpdateCourtAvailabilityRulesCommandValidator` and/or handler duplicate-day validation.
- Migration to make the index unique and resolve any existing duplicate data.
- Tests for the new behavior.

## Out of scope

- Changing the shape of `CourtAvailabilityRule` or its properties.
- Altering the slot-generation algorithm.
- Business-hours changes.

## Acceptance criteria

- [ ] The database has a unique index or constraint on `(CourtId, DayOfWeek)`.
- [ ] `PUT /api/v1/complexes/{complexId}/courts/{courtId}/availability` rejects a payload with duplicate `DayOfWeek` values.
- [ ] The update handler returns a clear client error when duplicates are supplied.
- [ ] Existing duplicates, if any, are handled by the migration strategy (deduplication or error with runbook).
- [ ] Public and admin availability endpoints continue to return the correct rule for each day.
- [ ] Unit and integration tests cover duplicate-day rejection.

## Business rules

1. A court may have at most one availability rule per day of week.
2. The update operation is atomic: replace the full set, but only if the new set is valid.
3. `DayOfWeek` values must be in the `0–6` range.

## Validations

- Validate the court exists and belongs to the requested complex.
- Validate each rule has `StartTime != EndTime` and the slot duration fits the range (existing).
- Validate no two rules in the same request share the same `DayOfWeek`.
- Validate `DayOfWeek` is within `0–6`.

## Error cases

| Scenario | Expected behavior |
|---|---|
| Update with two rules for Monday | `400 Bad Request` with duplicate-day error. |
| Update with `DayOfWeek` outside `0–6` | `400 Bad Request` with validation error. |
| Update with one rule per day | `200 OK` with the new rule set. |
| Existing duplicate data prevents unique index | Migration fails or includes a deduplication step; runbook documented. |

## Security considerations

- Only authorized complex administrators may modify court availability.
- Do not reveal internal database constraints in user-facing errors.

## Technical notes

### Backend

- In `CourtAvailabilityRuleConfiguration`, change:
  ```csharp
  builder.HasIndex(x => new { x.CourtId, x.DayOfWeek }).IsUnique();
  ```
- In `UpdateCourtAvailabilityRulesCommandValidator`, add a `Must` rule or custom validation to ensure all `DayOfWeek` values in the `Rules` collection are distinct.
- Alternatively, validate in the handler before deleting existing rules.
- Create an EF Core migration to apply the unique index. If existing duplicates are present, decide on a deduplication strategy (e.g., keep the first active rule, or fail with a manual runbook).

### Frontend

- The edit form already uses one row per day, so no frontend change is required unless duplicate-day validation needs to be reflected.

## Tests required

### Unit

- `UpdateCourtAvailabilityRulesCommandValidator` fails on duplicate days.
- `UpdateCourtAvailabilityRulesHandler` fails or prevents duplicate-day persistence.

### Integration

- `PUT .../availability` with duplicate days returns the documented error.
- Successful update with one rule per day persists correctly.
- Unique index is enforced at the database level.

## Dependencies

- EPIC-04 — Court Administration.
- US-020 — Configure the days and hours a court is available.

## Definition of Done

- [ ] All acceptance criteria are implemented and verifiable.
- [ ] EF Core migration is created and tested.
- [ ] Backend build and relevant tests pass.
- [ ] No secrets or sensitive data are committed.

## Project affected

- `src/Mova.Domain`
- `src/Mova.Infrastructure`
- `src/Mova.Application`
- `tests/Mova.UnitTests`
- `tests/Mova.IntegrationTests`

## Suggested branch

`fix/BUGFIX-003-enforce-one-availability-rule-per-court-day`

## Instructions for Devin

Implement this bugfix with minimal, focused changes. Verify that existing data will not block the migration; if duplicates exist, include a safe deduplication strategy in the migration.
