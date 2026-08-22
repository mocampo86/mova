# BUGFIX-004 — Require at least one sport when creating a court

## Status

Ready

## Priority

Medium

## Source

EPIC-04 review finding: **MEDIUM — Frontend create court allows and labels sports as optional.**

## Problem

`CreateCourtCommand` and `CreateCourtCommandValidator` allow `SportIds` to be `null` or empty. The React `CreateCourtPage` uses `sportIds: z.array(z.string()).default([])` and the English locale labels sports as optional. This is inconsistent with `EditCourtPage` (which requires at least one sport) and the EPIC-04 rule that a court must have at least one active sport before it can accept reservations.

## Objective

Make sports required when creating a court, both in the API and in the React admin form, and remove the "optional" label.

## Scope

- `CreateCourtCommandValidator`.
- `CreateCourtPage` Zod schema and default values.
- English, Spanish, and Portuguese i18n locale files.
- Unit and integration tests for court creation.
- Optional: backend `CreateCourtHandler` early validation.

## Out of scope

- Requiring sports to be active at creation.
- Changing the update or assign-sports flows (tracked in BUGFIX-005).
- Public availability or reservation flows.

## Acceptance criteria

- [ ] `POST /api/v1/complexes/{complexId}/courts` with no `SportIds` returns `400 Bad Request`.
- [ ] `CreateCourtCommandValidator` requires at least one sport.
- [ ] The `CreateCourtPage` form requires at least one selected sport and shows a validation error if none.
- [ ] The English, Spanish, and Portuguese labels no longer describe sports as optional.
- [ ] A court created with at least one sport still behaves as before.
- [ ] Existing integration tests for court creation are updated.

## Business rules

1. A court must be associated with at least one sport from the moment it is created.
2. The assigned sport may be active or inactive at creation; only an active sport makes the court reservation-ready.
3. The backend is the authoritative enforcement point; the frontend validation is a usability guard.

## Validations

- Validate `SportIds` is not null and contains at least one non-empty GUID.
- Validate each `SportId` corresponds to an existing sport (existing behavior).
- Return clear, structured error messages.

## Error cases

| Scenario | Expected behavior |
|---|---|
| Create court with no sports | `400 Bad Request` with "At least one sport is required." |
| Create court with an empty sport ID | `400 Bad Request` with sport validation details. |
| Create court with a non-existent sport ID | `404 Not Found` (existing behavior). |
| Create court with one or more valid sports | `201 Created` with persisted court. |

## Security considerations

- Only authorized complex administrators may create courts.
- Do not expose sport or court internal identifiers in error responses.
- Keep the existing `ComplexAdmin` authorization policy unchanged.

## Technical notes

### Backend

- Update `CreateCourtCommandValidator`:
  ```csharp
  RuleFor(x => x.SportIds).NotNull().Must(x => x.Count > 0)
      .WithMessage("At least one sport must be assigned to the court.");
  RuleForEach(x => x.SportIds).NotEmpty();
  ```
- `CreateCourtHandler` already rejects unknown sport IDs; ensure it still works.

### Frontend

- Update `CreateCourtPage` schema:
  ```typescript
  sportIds: z.array(z.string()).min(1, 'At least one sport must be assigned to the court.')
  ```
- Update i18n keys such as `admin.createCourt.sports` to remove the word "optional".
- Update `CreateCourtPage.test.tsx` to include at least one sport in successful-submission tests and to test the new validation.

## Tests required

### Unit

- `CreateCourtCommandValidator` fails with no sports.
- `CreateCourtCommandValidator` fails with empty sport ID.
- `CreateCourtCommandValidator` passes with valid sports.

### Integration

- `POST /api/v1/complexes/{complexId}/courts` with no sports returns `400`.
- `POST` with valid sports returns `201`.

### Frontend

- `CreateCourtPage` shows validation error when no sport is selected.
- `CreateCourtPage` submits successfully with at least one sport.

## Dependencies

- EPIC-04 — Court Administration.
- US-018 — Create a court with name, description, surface, and indoor/outdoor flag.
- US-019 — Assign one or more sports to a court.

## Definition of Done

- [ ] All acceptance criteria are implemented and verifiable.
- [ ] Backend and frontend tests pass.
- [ ] Frontend lint, build, and tests pass.
- [ ] No secrets or sensitive data are committed.

## Project affected

- `src/Mova.Application`
- `src/mova-web`
- `tests/Mova.UnitTests`
- `tests/Mova.IntegrationTests`

## Suggested branch

`fix/BUGFIX-004-require-sport-when-creating-court`

## Instructions for Devin

Implement this bugfix with minimal, focused changes. Do not make sports required at the domain level if that would break existing handler tests; use the command validator as the enforcement point.
