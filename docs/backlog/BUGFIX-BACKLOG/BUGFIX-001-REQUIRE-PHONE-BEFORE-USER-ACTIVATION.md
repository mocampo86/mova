# BUGFIX-001 — Require phone completion before user activation

## Status

Ready

## Priority

High

## Source

EPIC-02 review finding: **HIGH — CROSS-STORY: Mandatory profile completion can be bypassed through reservation APIs.**

## Problem

A user created through Google login is currently marked `Active` immediately, even when no phone number exists. The React application redirects incomplete users to `/complete-profile`, but reservation handlers only check `UserStatus.Active`; an authenticated user can bypass the UI and call the single or recurring reservation APIs directly.

This violates the EPIC-02 requirement that a phone number is mandatory before a user can make reservations.

## Objective

Ensure a user is not active until they provide and persist a valid phone number. The backend must be the authoritative enforcement point for activation and booking eligibility.

## Scope

- Introduce a distinct user status for an authenticated user whose profile is incomplete, such as `PendingProfile`.
- Create Google-authenticated users in the pending-profile status when no phone number is available.
- Permit a pending-profile user to authenticate and call the self-profile completion endpoint only.
- Transition the user to `Active` atomically after `PATCH /api/v1/users/me` validates and stores a valid phone number.
- Reject single and recurring reservation creation for users who are not `Active`.
- Preserve the existing `Blocked` behavior and ensure it cannot be bypassed.
- Keep the frontend redirect to `/complete-profile` as a usability guard; do not rely on it as the security boundary.

## Out of scope

- Phone ownership verification, SMS, WhatsApp, or OTP workflows.
- Refresh-token implementation.
- Changing complex-admin or super-admin authorization rules.
- Retrospective cleanup of historical reservations created before this fix, unless separately requested.

## Acceptance criteria

- [ ] A newly created Google user with no phone number has `UserStatus.PendingProfile`, not `Active`.
- [ ] A pending-profile user can successfully receive a JWT from Google login and access `PATCH /api/v1/users/me` to submit their phone number.
- [ ] A pending-profile user cannot create a single reservation through `POST /api/v1/complexes/{complexId}/reservations/me`; the API returns a consistent client error that identifies incomplete profile completion as the required next step.
- [ ] A pending-profile user cannot create a recurring reservation through `POST /api/v1/complexes/{complexId}/recurring-reservations/me`; the API returns the same semantic error.
- [ ] Supplying a valid phone number through `PATCH /api/v1/users/me` stores the number and transitions the authenticated user from `PendingProfile` to `Active` in the same persistence operation.
- [ ] After the transition to `Active`, the same user can create single and recurring reservations when all other reservation rules are satisfied.
- [ ] A blocked user remains unable to log in or create reservations.
- [ ] Existing active users who already have a phone number remain active after migration.
- [ ] The SPA continues to redirect pending-profile users to `/complete-profile` and renders the backend rejection safely if a direct request is attempted.

## Business rules

1. `Active` means the user has completed the mandatory phone-number profile requirement and is eligible for normal user operations.
2. `PendingProfile` means the identity has been authenticated but the profile is incomplete; it is not a blocked or deleted account.
3. `Blocked` takes precedence over all other account states.
4. Pending-profile users may access only the minimum authenticated functionality needed to complete their profile. They must not be able to create reservations, recurring reservations, or use other operations that require an active user.
5. The server, not the React route guard, determines whether a user is active and eligible to reserve.
6. Phone numbers must continue to satisfy the existing international format: `+` followed by 7–15 digits, with optional spaces, and no more than 50 characters.

## Technical notes

### Domain and persistence

- Add `PendingProfile` to `UserStatus`.
- Update `User.CreateFromGoogle` to create users with `PendingProfile` when no phone number is provided.
- Add a domain method or extend `CompleteProfile` so it persists the phone number, sets `PhoneVerified` to `false`, and transitions `PendingProfile` to `Active`.
- Create and validate an EF Core migration for the persisted user-status representation. Ensure existing values remain valid and existing active users are not inadvertently downgraded.

### Authentication and authorization

- Keep Google login available to pending-profile users so they can obtain the JWT required by `PATCH /api/v1/users/me`.
- Continue rejecting `Blocked` users at login.
- Do not use a generic "active user" authorization policy for the profile-completion endpoint, because pending-profile users must be able to complete their own profile.
- Define a consistent application/API error for operations that require an active profile. Prefer a conflict or forbidden response with a stable, documented error code such as `PROFILE_COMPLETION_REQUIRED`; use the project’s existing Problem Details / error-response convention.

### Reservation flows

- Enforce active-user eligibility in both `CreateReservationHandler` and `CreateRecurringReservationHandler` before any reservation is persisted.
- Centralize this eligibility rule where practical so future user operations cannot accidentally omit it.
- Preserve the existing blocked-user and availability checks.

### Frontend

- Continue routing an incomplete user to `/complete-profile` after login and when entering the user portal.
- Map `PROFILE_COMPLETION_REQUIRED` to a clear, non-sensitive UI message and route the user to `/complete-profile` if an API call exposes the state.

## Error cases

| Scenario | Expected behavior |
|---|---|
| Google login creates a user without a phone number | User is `PendingProfile`; login succeeds and response requires profile completion. |
| Pending-profile user calls profile completion with invalid phone | `400 Bad Request` with validation details; user remains `PendingProfile`. |
| Pending-profile user creates a single reservation directly | No reservation is created; API returns the defined profile-completion error. |
| Pending-profile user creates a recurring reservation directly | No recurring reservation or occurrences are created; API returns the same semantic error. |
| Pending-profile user completes profile with valid phone | Phone is persisted; user becomes `Active`; subsequent permitted booking may proceed. |
| Blocked user logs in or books | Request remains rejected according to existing blocked-user behavior. |
| Active user with a phone number books | Existing successful booking behavior remains unchanged. |

## Security considerations

- Treat profile completion as server-side authorization and business-rule enforcement; frontend routing is not a security control.
- Do not reveal phone numbers, tokens, or internal status details in logs or error responses.
- Ensure a caller can only activate their own user record through `PATCH /api/v1/users/me`.
- Test direct API access because a valid JWT alone must not grant booking eligibility before activation.

## Tests required

### Unit

- `User.CreateFromGoogle` creates a pending-profile user.
- Completing a valid profile transitions a pending-profile user to active.
- Completing a profile does not reactivate a blocked user unless an explicit, separately authorized unblock flow does so.
- Single and recurring reservation handlers reject pending-profile users and do not persist reservation data.

### Integration

- Google login for a new user returns `RequiresProfileCompletion=true` and a persisted `PendingProfile` status.
- A pending-profile JWT can complete its own profile.
- A pending-profile JWT receives the documented rejection for direct single-reservation and recurring-reservation API calls.
- After valid completion, the user can create both reservation types when court and availability setup is valid.
- Existing active and blocked-user behaviors remain covered.

### Frontend

- Login response requiring profile completion navigates to `/complete-profile`.
- Protected user routes keep a pending-profile user at `/complete-profile`.
- A booking API response with `PROFILE_COMPLETION_REQUIRED` is displayed safely and directs the user to complete the profile.

### E2E

- Test-authenticated pending-profile user is redirected to `/complete-profile`.
- Completing a valid phone number transitions the user to the portal and permits a booking.
- A direct booking attempt made before completion is rejected and no reservation appears.

## Dependencies

- EPIC-02 — Identity and Access, especially US-008, US-009, US-011, and US-012.
- Reservation creation flows in EPIC-06.

## Definition of Done

- [ ] All acceptance criteria are implemented and verifiable.
- [ ] Domain, application, API, database migration, and frontend changes are covered by focused tests.
- [ ] Backend build and the full backend test suite pass, including integration tests.
- [ ] Frontend lint, build, and test suites pass.
- [ ] Relevant E2E test passes in a deterministic test-auth setup.
- [ ] Existing active users retain their status after migration validation.
- [ ] API error behavior is documented if a new error code is introduced.
- [ ] No phone numbers, Google credentials, or JWT values are logged or committed.

## Project affected

- `src/Mova.Domain`
- `src/Mova.Application`
- `src/Mova.Infrastructure`
- `src/Mova.Api`
- `src/mova-web`
- `tests/Mova.UnitTests`
- `tests/Mova.IntegrationTests`

## Suggested branch

`fix/BUGFIX-001-require-phone-before-user-activation`

## Instructions for Devin

Implement this bugfix with minimal, focused changes. Make the server authoritative for user activation and reservation eligibility. Do not silently treat a pending-profile user as blocked: they must still be able to authenticate and complete their own profile.
