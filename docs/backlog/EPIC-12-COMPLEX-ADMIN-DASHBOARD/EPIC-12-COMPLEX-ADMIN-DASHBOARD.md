# EPIC-12 — Complex Admin Dashboard

## Status

Ready

## Objective

Provide complex administrators with a centralized, self-contained dashboard where they can manage every aspect of their sports complex: profile, courts, availability, reservations, and users.

## Scope

- Centralized admin landing page at `/admin/complex/:complexId`.
- Persistent navigation layout (sidebar or top navigation) for all admin sections.
- Reuse existing backend endpoints from EPIC-03, EPIC-04, EPIC-06, and EPIC-08.
- Add a dashboard overview endpoint if required for aggregated metrics.
- Frontend pages for:
  - Dashboard overview
  - Complex profile editing and activation
  - Court list, creation, editing, status toggle, sports assignment, and availability
  - Reservation list, manual creation, cancellation, and status updates
  - User search and block management

## User stories

| ID | Story |
|----|-------|
| [US-062](US-062.md) | As a complex administrator, I want a dashboard home page so I can see the status of my complex at a glance. |
| [US-063](US-063.md) | As a complex administrator, I want a navigation layout so I can move between sections of my complex admin panel. |
| [US-064](US-064.md) | As a complex administrator, I want to edit my complex profile from the admin panel so it is always up to date. |
| [US-065](US-065.md) | As a complex administrator, I want to activate or deactivate my complex from the admin panel so I can control public visibility. |
| [US-066](US-066.md) | As a complex administrator, I want to list all courts of my complex from the admin panel so I can manage them. |
| [US-067](US-067.md) | As a complex administrator, I want to create a court from the admin panel so I can expand my complex. |
| [US-068](US-068.md) | As a complex administrator, I want to edit a court from the admin panel so its information stays accurate. |
| [US-069](US-069.md) | As a complex administrator, I want to configure a court's sports, availability rules, and slot duration from the admin panel. |
| [US-070](US-070.md) | As a complex administrator, I want to view and manage all reservations of my complex from the admin panel. |
| [US-071](US-071.md) | As a complex administrator, I want to search users of my complex and manage blocks from the admin panel. |

## Acceptance criteria

- [ ] A complex administrator can access `/admin/complex/:complexId` and see a dashboard overview.
- [ ] The admin panel has persistent navigation to all sections.
- [ ] The administrator can edit complex profile information and activation status.
- [ ] The administrator can list, create, edit, activate/deactivate, and configure courts.
- [ ] The administrator can view, create, cancel, and mark reservations as completed/no-show.
- [ ] The administrator can search users, view history, and block/unblock users.
- [ ] All sections respect the `RequireComplexAdmin` guard and multi-tenancy authorization.
- [ ] Mobile-responsive layout is supported.
- [ ] Pages use translations and loading/error states.
- [ ] Invalid or unauthorized access redirects to `/unauthorized`.

## Dependencies

- EPIC-02 — Identity and Access.
- EPIC-03 — Sports Complex Administration.
- EPIC-04 — Court Administration.
- EPIC-06 — Reservations.
- EPIC-08 — User Management and Blocking.
- EPIC-11 — Internationalization and Language Support (for translated UI text).

## Technical notes

- Create `ComplexAdminLayout` in `src/mova-web/src/layouts/` with `RequireComplexAdmin` wrapper.
- Refactor `ComplexAdminPage` from placeholder to dashboard shell with `Outlet` for nested routes.
- Add nested routes under `/admin/complex/:complexId` in `src/mova-web/src/app/router.tsx`.
- Add feature folders under `src/mova-web/src/features/admin/complex/` or extend existing `features/complexes/`:
  - `dashboard/`
  - `profile/`
  - `courts/`
  - `reservations/`
  - `users/`
- Reuse existing API clients where possible; extend `complexApi.ts` or create `adminApi.ts` with mutations.
- Consider a dashboard overview endpoint `GET /api/v1/complexes/{complexId}/dashboard` for aggregated data.
- Follow Material-UI patterns already in use (e.g., `CompleteComplexAdminPage.tsx`).
- Add unit and E2E tests for critical admin flows.

## Definition of Done

- All acceptance criteria are implemented and verifiable.
- Relevant unit, integration, and E2E tests pass.
- Code review is approved.
- No secrets or sensitive data are committed.
- Documentation is updated if the change affects setup or API contracts.
