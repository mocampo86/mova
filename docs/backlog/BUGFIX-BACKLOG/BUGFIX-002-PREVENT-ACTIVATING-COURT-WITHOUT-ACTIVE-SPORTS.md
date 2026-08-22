# BUGFIX-002 — Prevent activating a court without active sports

## Status

Ready

## Priority

High

## Source

EPIC-04 review finding: **HIGH — Court can be activated without any active sports.**

## Problem

`UpdateCourtStatusHandler` toggles a court's status to `Active` without verifying that the court has at least one active sport. Because public availability and the reservation creation path only check `CourtStatus.Active`, a court can be activated, listed publicly, and selected for booking even though it has no sports assigned.

This violates the EPIC-04 business rule that a court must have at least one active sport before it can accept reservations.

## Objective

Ensure a court can only be activated when it has at least one active sport. Provide a clear, actionable error when an activation request does not meet this condition.

## Scope

- `UpdateCourtStatusHandler` in `src/Mova.Application/Courts/Handlers`.
- `Court` domain entity in `src/Mova.Domain/Entities/Court.cs` if a new query method is needed.
- `UpdateCourtStatusHandlerTests` and related integration tests.
- Optional: frontend message or guard on the court status toggle.

## Out of scope

- Enforcing the active-sport rule at reservation creation (tracked as a related finding; consider a separate bugfix if not already covered).
- Requiring sports at court creation (tracked in BUGFIX-004).
- Changing the public availability filtering behavior.
- Audit logging of status changes.

## Acceptance criteria

- [ ] A court with no assigned sports cannot be activated through `PATCH /api/v1/complexes/{complexId}/courts/{courtId}/status`.
- [ ] A court with only inactive sports cannot be activated.
- [ ] A court with at least one active sport can be activated normally.
- [ ] Deactivation of a court is unaffected by sport assignment.
- [ ] The API returns a consistent client error with a stable error code when activation is rejected.
- [ ] The React admin UI surfaces the error when the user toggles a court to active.
- [ ] Existing integration and unit tests are updated or extended to cover the new behavior.

## Business rules

1. A court may be created without sports, but it must not be publicly reservable until it has at least one active sport.
2. `Active` status for a court implies it is eligible for reservations, which requires at least one active sport.
3. `Inactive` status may be set regardless of sport assignment.
4. Only assigned sports with `SportStatus.Active` count toward activation eligibility.

## Validations

- Validate the court exists and belongs to the requested complex.
- If the requested status is `Active`, validate the court has at least one assigned sport whose status is `Active`.
- If the requested status is `Inactive`, skip the sport check.
- Return a structured error message and avoid exposing internal identifiers.

## Error cases

| Scenario | Expected behavior |
|---|---|
| Court has no sports and admin requests activation | `409 Conflict` with a clear error code. |
| Court has only inactive sports and admin requests activation | `409 Conflict` with a clear error code. |
| Court has at least one active sport and admin requests activation | `200 OK` with updated court info. |
| Admin requests deactivation | `200 OK` regardless of sports. |
| Court does not belong to the complex | `404 Not Found`. |

## Security considerations

- Only authorized complex administrators or super admins may activate/deactivate a court.
- Do not reveal internal sport or court identifiers in error responses.
- Keep the existing `ComplexAdmin` authorization policy unchanged.

## Technical notes

### Backend

- In `UpdateCourtStatusHandler`, after loading the court and before applying the status change, check:
  ```csharp
  if (command.Status == CourtStatus.Active &&
      !court.CourtSports.Any(cs => cs.Sport?.Status == SportStatus.Active))
      throw new ConflictException("The court must have at least one active sport before it can be activated.");
  ```
- Ensure `GetByIdAsync` includes `CourtSports` and `Sport` so the status can be evaluated without a separate query.
- Reuse `Court.CanAcceptReservations()` logic where possible, but note that it also checks `Status == Active`, so a helper such as `HasActiveSports()` or an inline check is needed.

### Frontend

- The `ComplexCourtsPage` toggle can continue to call the API; the API error should be displayed.
- Consider adding a disabled state or tooltip when the court has no sports.

## Tests required

### Unit

- `UpdateCourtStatusHandler` rejects activation when no sports are assigned.
- `UpdateCourtStatusHandler` rejects activation when only inactive sports are assigned.
- `UpdateCourtStatusHandler` allows activation when at least one active sport exists.
- Deactivation behavior remains unchanged.

### Integration

- `PATCH /api/v1/complexes/{complexId}/courts/{courtId}/status` returns the documented error for a sport-less court.
- Activation succeeds for a court with active sports.
- Deactivation succeeds regardless of sports.

### Frontend

- Error message is rendered when the user toggles a sport-less court to active.

## Dependencies

- EPIC-04 — Court Administration.
- US-019 — Assign one or more sports to a court.
- US-022 — Activate or deactivate a court.

## Definition of Done

- [ ] All acceptance criteria are implemented and verifiable.
- [ ] Backend build and relevant tests pass.
- [ ] Frontend lint, build, and tests pass.
- [ ] No secrets or sensitive data are committed.
- [ ] Documentation is updated if the API contract changes.

## Project affected

- `src/Mova.Domain`
- `src/Mova.Application`
- `src/Mova.Api`
- `tests/Mova.UnitTests`
- `tests/Mova.IntegrationTests`
- `src/mova-web`

## Suggested branch

`fix/BUGFIX-002-prevent-activating-court-without-active-sports`

## Instructions for Devin

Implement this bugfix with minimal, focused changes. Do not change the reservation-creation flow; only the activation handler and related tests should be modified.
