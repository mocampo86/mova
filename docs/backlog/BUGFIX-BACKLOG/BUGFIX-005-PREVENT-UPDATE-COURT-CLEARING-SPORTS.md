# BUGFIX-005 — Prevent UpdateCourt from clearing all sports on an active court

## Status

Ready

## Priority

Medium

## Source

EPIC-04 review finding: **MEDIUM — `UpdateCourt` can clear all sports from an active court.**

## Problem

`Court.Update` accepts an empty `sportIds` enumerable and clears `CourtSports`, leaving an active court with no sports. This is inconsistent with `Court.AssignSports`, which requires at least one sport, and with the EPIC-04 business rule that an active court must have at least one active sport to be reservable.

## Objective

Ensure that updating a court cannot remove all sports while the court remains active.

## Scope

- `Court.Update` domain method or `UpdateCourtHandler`.
- `UpdateCourtCommandValidator`.
- Frontend `EditCourtPage` sport validation (already requires at least one).
- Unit and integration tests.

## Out of scope

- Requiring a minimum number of sports at creation (tracked in BUGFIX-004).
- Requiring sports to be active before update.
- Changing the `AssignSports` endpoint.

## Acceptance criteria

- [ ] `PUT /api/v1/complexes/{complexId}/courts/{courtId}` with an empty `SportIds` array returns `400 Bad Request`.
- [ ] `UpdateCourt` with an empty `SportIds` does not persist the change.
- [ ] `UpdateCourt` with one or more valid sports replaces the existing sports correctly.
- [ ] `UpdateCourt` without providing `SportIds` (null) leaves sports unchanged (non-sport-field update still works).
- [ ] Unit and integration tests cover the empty-sport rejection.

## Business rules

1. An active court must retain at least one assigned sport.
2. A request that would clear all sports from an active court must be rejected.
3. A request that does not include `SportIds` should not modify the existing sport assignment.

## Validations

- Validate the court exists and belongs to the requested complex.
- If `SportIds` is provided, it must contain at least one non-empty GUID.
- Validate each provided sport ID exists (existing behavior).
- Return clear, structured error messages.

## Error cases

| Scenario | Expected behavior |
|---|---|
| Update with `SportIds: []` | `400 Bad Request` with "At least one sport is required." |
| Update with `SportIds: null` | `200 OK`; only name/description/surface/indoor are updated. |
| Update with one or more valid sports | `200 OK` with updated court. |
| Court not found or from different complex | `404 Not Found`. |

## Security considerations

- Only authorized complex administrators may update a court.
- Do not reveal internal identifiers in error responses.

## Technical notes

### Backend

- Update `UpdateCourtCommandValidator`:
  ```csharp
  RuleFor(x => x.SportIds)
      .Must(x => x is null || x.Count > 0)
      .When(x => x.SportIds is not null)
      .WithMessage("At least one sport must be assigned to the court.");
  ```
- Alternatively, enforce in `Court.Update` by throwing if `sportIds` is non-null and empty. Keep null semantics as "do not change sports".
- Ensure `UpdateCourtHandler` loads `CourtSports` with `Sport` if the handler itself needs to evaluate active-sport status.

### Frontend

- `EditCourtPage` already requires at least one sport; no change needed unless the error message mapping must be updated.

## Tests required

### Unit

- `UpdateCourtCommandValidator` rejects empty `SportIds` but allows `null`.
- `UpdateCourtHandler` does not clear sports when `null` is passed.
- `UpdateCourtHandler` rejects empty `SportIds`.

### Integration

- `PUT .../courts/{courtId}` with `SportIds: []` returns `400`.
- `PUT` with valid `SportIds` returns `200`.
- `PUT` without `SportIds` returns `200` and preserves sports.

## Dependencies

- EPIC-04 — Court Administration.
- US-018 — Create a court.
- US-019 — Assign one or more sports to a court.

## Definition of Done

- [ ] All acceptance criteria are implemented and verifiable.
- [ ] Backend build and relevant tests pass.
- [ ] No secrets or sensitive data are committed.

## Project affected

- `src/Mova.Domain`
- `src/Mova.Application`
- `tests/Mova.UnitTests`
- `tests/Mova.IntegrationTests`

## Suggested branch

`fix/BUGFIX-005-prevent-update-court-clearing-sports`

## Instructions for Devin

Implement this bugfix with minimal, focused changes. Preserve the existing behavior where `SportIds` is optional and, when omitted, sports are not modified.
