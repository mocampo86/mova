# EPIC-13 — User Dashboard

## Status

Ready

## Objective

Provide authenticated end users with a centralized, self-contained dashboard where they can access every user-facing feature of the platform from a single place: profile, discovery, reservations, recurring bookings, and account status.

## Scope

- Centralized user landing page at `/user` (replacing the current placeholder).
- Persistent navigation layout (sidebar or top navigation) for all user sections.
- Reuse existing backend endpoints from EPIC-02, EPIC-05, EPIC-06, EPIC-07, and EPIC-08.
- Aggregated dashboard overview with cards/summary widgets.
- Entry points to all current user functionalities:
  - Search and browse active complexes
  - View complex details, courts, sport filters and availability
  - Create single and recurring reservations
  - View upcoming reservations and history
  - Cancel a reservation
  - View/edit profile
  - View block status per complex
  - Switch language

## User stories

| ID | Story |
|----|-------|
| [US-072](US-072.md) | As a user, I want a dashboard home page so I can access all my features and reservations at a glance. |
| [US-074](US-074.md) | As a user, I want to cancel an upcoming reservation from the user dashboard. |

## Acceptance criteria

- [ ] A logged-in user can access `/user` and see a dashboard overview.
- [ ] The dashboard has persistent navigation to all user sections.
- [ ] The dashboard surfaces existing user features through summary cards and links.
- [ ] The dashboard is responsive, translated, and protected by authentication.
- [ ] Incomplete profiles are redirected to `/complete-profile` before accessing the dashboard.
- [ ] Unauthorized access redirects to `/login` or `/unauthorized`.

## Dependencies

- EPIC-02 — Identity and Access.
- EPIC-05 — Public Availability and Discovery.
- EPIC-06 — Reservations.
- EPIC-07 — Recurring Reservations.
- EPIC-08 — User Management and Blocking.
- EPIC-11 — Internationalization and Language Support.

## Technical notes

- Create `UserLayout` in `src/mova-web/src/layouts/` wrapping routes with a `RequireRole` guard for `User`, `ComplexAdmin`, or `SuperAdmin`.
- Refactor/replace the `UserHomePage` placeholder with a real `UserDashboardPage`.
- Add nested routes under `/user` in `src/mova-web/src/app/router.tsx`.
- Reuse existing API clients from `features/complexes/complexApi.ts`, `features/reservations/reservationApi.ts`, and `services/usersApi.ts`.
- Consider `GET /api/v1/users/me/dashboard` to aggregate counts (upcoming reservations, history, active blocks) if client-side aggregation becomes inefficient.
- Follow MUI patterns and the mobile-first approach.
- Update `src/mova-web/README.md` with new routes.

## Definition of Done

- All acceptance criteria are implemented and verifiable.
- Relevant unit, integration, and E2E tests pass.
- Code review is approved.
- No secrets or sensitive data are committed.
- Documentation is updated if the change affects setup or API contracts.
