# Epic Review

## Epic

EPIC-13 — User Dashboard

**Review date:** 2026-08-29

## Executive Summary

This audit reviewed EPIC-13 and its two stories, US-072 (dashboard overview) and US-074 (cancel an upcoming reservation), across the React application, .NET API/application layers, contracts, persistence access, tests, documentation, and relevant Git history.

The core capability is present: `/user` is guarded by role, rendered in a responsive user shell, redirects users without a phone number, shows the authenticated user's reservations/history/block status, and links to the existing discovery and profile flows. User-side cancellation correctly verifies ownership, eligible state, and the effective cancellation policy before releasing the reservation. The dashboard API is thin at the HTTP boundary and uses the expected Clean Architecture layers.

However, the Epic is not ready for approval. A dashboard card directs users to discovery instead of the recurring-reservation flow; cancellation deadline/disabled-policy errors are rendered from the English API message instead of the selected locale; and the required page/layout/browser coverage is absent. The shared dashboard query also creates cross-story consistency and reliability risks: it is not invalidated after a user cancellation, and a transient summary request failure removes navigation to every user child route. The backend aggregate has an N+1 complex-name lookup for active blocks.

**Production readiness:** Not ready until the MEDIUM findings are addressed and the targeted user journeys are browser-tested. The full frontend and integration suites also need to be returned to green; their observed failures were outside EPIC-13's source paths.

## Overall Verdict

**CHANGES REQUESTED** — six MEDIUM findings should be resolved before approving EPIC-13.

## Epic Completeness

**Implementation completeness: approximately 82%.**

The major happy paths and backend integration are implemented. The remaining gap is not feature scaffolding; it is a broken recurring entry point, localized error handling, resilience/cache consistency, performance hardening, and the unit/component/E2E evidence explicitly required by the stories.

## Scope Reconstruction

### Intended user flow

1. An authenticated `User`, `ComplexAdmin`, or `SuperAdmin` opens `/user`; an unauthenticated/unauthorized visitor is redirected by `RequireRole`.
2. The user layout obtains the current user's dashboard summary. A user with no phone number is sent to `/complete-profile`.
3. The dashboard presents the user's identity, upcoming and historical reservation summaries, active per-complex block notifications, and links to discovery, new single/recurring reservations, profile, and the user sections.
4. The user opens upcoming reservations, chooses an eligible `Pending` or `Confirmed` reservation, optionally supplies a reason, and confirms cancellation.
5. The API authenticates the caller, prevents cross-user access, applies the complex policy/global fallback and cancellation window, records the cancellation, and makes the slot available.
6. The SPA refreshes reservation/history/availability state and the user returns to an accurate dashboard.

### Dependency map

```text
EPIC-13 User Dashboard
├── US-072 Dashboard overview
│   ├── React Router -> RequireRole -> UserLayout -> UserDashboardPage
│   ├── useUserDashboard -> GET /api/v1/users/me/dashboard
│   ├── UsersController -> GetUserDashboardHandler
│   ├── User / Reservation / BlockedUser repositories
│   └── user/dashboard i18n resources and README route documentation
└── US-074 User cancellation
    ├── UserReservationsPage confirmation dialog
    ├── useCancelMyReservation -> PATCH /api/v1/users/me/reservations/{id}/cancel
    ├── ReservationsController -> CancelMyReservationHandler
    ├── ownership, reservation state, and cancellation-policy checks
    └── TanStack Query invalidation of reservations/history/availability/dashboard
```

## Requirements Coverage

| Requirement | Story | Implementation | Tests | Status | Finding |
|---|---|---|---|---|---|
| Protected `/user` dashboard and role-based redirect | US-072 / Epic | Complete | Partial | PASS | `RequireRole` allows the specified frontend roles. |
| Redirect a user without a phone number | US-072 / Epic | Complete | No direct layout test | PARTIAL | Implemented after the summary request succeeds; no guard test exists. |
| Welcome identity, upcoming summary, history summary, and block notifications | US-072 | Complete | Handler, API, hook | PARTIAL | Page rendering/empty/error-state tests are absent (M-04). |
| Persistent responsive navigation to user sections | US-072 / Epic | Complete | No direct layout test | PARTIAL | Desktop drawer and mobile temporary drawer are implemented; transient dashboard failure blocks all routes (M-05). |
| Browse active complexes and create a single reservation | US-072 | Complete | Existing dependent feature tests | PASS | Dashboard and navigation link to `/complexes`. |
| Entry point to recurring booking | US-072 | Partial | No page test | GAP | Card links to `/complexes`, not `/user/recurring` (M-01). |
| Entry point to profile | US-072 | Complete | Existing profile tests | PASS | Dashboard/nav link to `/user/profile`. |
| Translated dashboard labels and empty states | US-072 | Mostly complete | Existing i18n/hook tests | PARTIAL | Cancellation policy/deadline failures bypass translation (M-02). |
| Cancel only owned, Pending, or Confirmed reservation | US-074 | Complete | Unit and integration | PASS | Application handler checks owner and state. |
| Confirmation dialog accepts optional reason and disables submit while pending | US-074 | Complete | No page test | PARTIAL | Implemented, but no component test proves dialog behavior (M-04). |
| User cancellation request and policy/deadline errors | US-074 | Mostly complete | API hook/unit/integration | PARTIAL | Request/error code path works, but relevant error text is not localized (M-02). |
| Refresh upcoming, history, freed availability, and dashboard state after cancellation | US-074 / US-072 | Partial | No invalidation test | GAP | Dashboard query is not invalidated (M-03). |
| Required unit, integration, and E2E evidence | US-072 / US-074 | Partial | Handler/API/hook only | GAP | Page/layout and dedicated Epic E2E tests are missing (M-04). |
| Dashboard aggregate is appropriate under many blocks | US-072 | Partial | Basic handler/API tests | GAP | Active block complex names are fetched one-by-one (M-06). |
| Route/API documentation and delivery metadata are current | Epic | Partial | Documentation review | GAP | README routes are updated; API design omits the dashboard endpoint and Epic/story status remains `Ready` with unchecked criteria (L-01). |

## Findings

[MEDIUM] Recurring-booking dashboard card bypasses the recurring flow

Category:
Functional / Frontend

Affected stories:
US-072

Location:
`src/mova-web/src/pages/UserDashboardPage.tsx:170-180`

Problem:
The recurring-reservation card's `Book weekly` action targets `/complexes`. The recurring creation form is the nested `/user/recurring` route, which is already exposed by `UserLayout` navigation.

Why it matters:
The dashboard must provide an entry point to create a recurring reservation. Discovery may eventually lead to a recurring flow, but this card does not perform the purpose stated in its own copy and creates an unnecessary, indirect journey.

Scenario:
A user selects the dashboard's recurring-booking card expecting to set up a weekly reservation. They are sent to complex discovery instead of the recurring reservation form.

Recommendation:
Change the card destination to `/user/recurring` and add a page test that asserts the recurring action routes there.

Confidence:
HIGH

---

[MEDIUM] Cancellation policy/deadline errors are not localized

Category:
Functional / Frontend

Affected stories:
US-074, US-072

Location:
`src/mova-web/src/shared/utils/apiError.ts:35-58`; `src/mova-web/src/pages/UserReservationsPage.tsx:166-168`; `src/Mova.Application/Common/Errors/ErrorResponseBuilder.cs:45-55`

Problem:
The API emits stable `CANCELLATION_DEADLINE_PASSED` and `USER_CANCELLATION_DISABLED` error codes, and the reservation dialog correctly renders errors through `ApiErrorMessage`. However, `ERROR_CODE_KEY_MAP` has no mappings for either code. The mapper therefore falls back to the server `message`, which is English.

Why it matters:
US-072 requires translated labels and error states, while US-074 specifically requires deadline errors to be surfaced. A Spanish or Portuguese user receives an English cancellation-policy explanation at the point they need to understand the action.

Scenario:
A Portuguese-speaking user cancels inside the server-enforced deadline window. The API returns `CANCELLATION_DEADLINE_PASSED`; the dialog displays the raw English message instead of a `pt` translation.

Recommendation:
Map both stable codes to new `common.error` keys, supply those keys in `en`, `es`, and `pt`, and test the rendered translated error for the deadline response.

Confidence:
HIGH

---

[MEDIUM] CROSS-STORY — Cancellation leaves the dashboard aggregate stale

Category:
Functional / Frontend

Affected stories:
US-072, US-074

Location:
`src/mova-web/src/features/reservations/reservationApi.ts:349-365`; `src/mova-web/src/features/users/useUserDashboard.ts:5-18`

Problem:
`useCancelMyReservation` invalidates `my-reservations`, `my-reservation-history`, and `court-availability`, but not the `user-dashboard` query. The dashboard owns the upcoming/history summaries affected by the cancellation.

Why it matters:
The individual list can refresh while the central dashboard temporarily retains a cancelled reservation and pre-cancellation history count. This inconsistency is only visible when the two Epic stories are used together.

Scenario:
A user cancels a reservation from `/user/reservations` and returns to `/user`. Until a later refetch completes, cached dashboard cards can show the cancelled booking as upcoming or an outdated history summary.

Recommendation:
Invalidate `['user-dashboard']` on success for self-service cancellation and every user reservation mutation that changes dashboard summaries. Extend the hook test to assert all required query invalidations.

Confidence:
HIGH

---

[MEDIUM] CROSS-STORY — Required dashboard/cancellation UI and browser journeys are unproven

Category:
Testing

Affected stories:
US-072, US-074

Location:
`docs/backlog/EPIC-13-USER-DASHBOARD/US-072.md:76-80`; `docs/backlog/EPIC-13-USER-DASHBOARD/US-074.md:60-64`; no `UserDashboardPage.test.tsx`, `UserReservationsPage.test.tsx`, `UserLayout.test.tsx`, or EPIC-13 Playwright specification exists

Problem:
The dashboard hook, cancellation hook, dashboard handler, and backend endpoint have tests. No component test proves dashboard card content/links, responsive user-layout guards/navigation, confirmation dialog behavior, or user-facing cancellation errors. No E2E test covers an authenticated user viewing the dashboard, navigating to discovery/profile/reservations, cancelling, and observing the reservation move to history.

Why it matters:
The required tests explicitly include page-level unit coverage and E2E journeys. Mocked handler/hook tests cannot establish router/layout composition, real dialog behavior, or the cross-story cache/navigation transition.

Scenario:
The recurring card regression in M-01 and the missing dashboard invalidation in M-03 are both compatible with the current focused test coverage.

Recommendation:
Add dashboard-page, user-layout, and user-reservations-page tests. Add deterministic Playwright coverage for: protected dashboard/profile-completion redirect; dashboard navigation; and cancellation through the dialog with the booking removed from upcoming/history updated and availability refreshed.

Confidence:
HIGH

---

[MEDIUM] CROSS-STORY — A dashboard-summary request failure disables the entire user portal

Category:
Reliability / Frontend

Affected stories:
US-072, US-074

Location:
`src/mova-web/src/layouts/UserLayout.tsx:21-34`

Problem:
`UserLayout`, which wraps dashboard, reservations, history, recurring, and profile, queries the optional dashboard aggregate. When that query errors, it renders only a full-page error and does not render its navigation or outlet.

Why it matters:
A transient failure in summary aggregation prevents a user from accessing independent user functions, including profile completion/recovery and reservations cancellation. An aggregate dashboard endpoint should not become a single point of failure for every child route.

Scenario:
The dashboard endpoint times out while `/user/reservations` is loaded. The layout replaces the reservations page with an error screen, so the user cannot view or cancel bookings even if the reservation endpoint remains healthy.

Recommendation:
Keep the user shell/outlet available when the summary query fails. Restrict dashboard-specific error UI to the dashboard page, and make profile-completion state available from a dedicated current-user/auth source or a resilient guard path.

Confidence:
HIGH

---

[MEDIUM] Dashboard active-block enrichment performs N+1 complex lookups

Category:
Performance / Database

Affected stories:
US-072

Location:
`src/Mova.Application/Users/Handlers/GetUserDashboardHandler.cs:38-46`; `src/Mova.Infrastructure/Persistence/Repositories/BlockedUserRepository.cs:60-68`

Problem:
After fetching active blocks in one query, the handler loops over distinct complex IDs and calls `ISportsComplexRepository.GetByIdAsync` once per ID.

Why it matters:
The endpoint is invoked for the user layout and dashboard. A user blocked in many complexes incurs one initial block query plus one database query per complex, increasing latency and database load precisely on the portal landing request.

Scenario:
A user with active blocks at 20 complexes opens `/user`; the aggregate can issue 21 database queries before rendering.

Recommendation:
Project the complex name in the blocked-user repository query, or add one bulk complex-name lookup keyed by the distinct IDs. Add a repository/handler test covering multiple blocks and the bulk path.

Confidence:
HIGH

---

[LOW] CROSS-STORY — Delivery records and API reference do not reflect the implemented Epic

Category:
Documentation

Affected stories:
EPIC-13, US-072, US-074

Location:
`docs/backlog/EPIC-13-USER-DASHBOARD/EPIC-13-USER-DASHBOARD.md:3-5,34-41`; `US-072.md:3-5,15-30`; `US-074.md:3-5,15-25`; `.ai-kit/docs/architecture/API-DESIGN.md:50-60`

Problem:
The Epic and both stories remain `Ready` with all acceptance boxes unchecked despite an implementation existing. The frontend README documents the user routes, but the API design's user endpoint list does not describe `GET /api/v1/users/me/dashboard`.

Why it matters:
Delivery status cannot distinguish implemented behavior from the remaining audit findings, and API consumers lack the architecture-level reference for the new aggregate contract.

Scenario:
A release reviewer sees a ready/unimplemented Epic while an API integrator only finds the dashboard route by inspecting controller code or Swagger.

Recommendation:
After addressing and verifying the findings, update statuses and only the criteria supported by evidence. Add the dashboard endpoint and its aggregate response to API design documentation.

Confidence:
HIGH

## Security Assessment

- **Authentication:** The user controller and self-service reservation endpoints require the `User` policy. The SPA's `/user` route additionally applies `RequireRole` for `User`, `ComplexAdmin`, and `SuperAdmin` and redirects unauthenticated visitors to `/login`.
- **Authorization and ownership:** `CancelMyReservationHandler` uses the authenticated user ID and returns not-found for missing or non-owned reservations, preventing user-to-user reservation enumeration. Dashboard repository queries are keyed by the authenticated user ID. The configured `User` backend policy means any authenticated principal can invoke these self-scoped endpoints; this aligns with the architecture document's definition of the User policy as "Any authenticated user" and does not expose another user's data.
- **Data exposure:** The dashboard exposes the authenticated user's profile fields, their reservations, and their own active blocks. No cross-user or cross-complex administration data was found in the aggregate contract.
- **Input security:** Dashboard pagination is FluentValidation-constrained to 1–100. Cancellation is handled through typed DTOs and application-layer rules; no unsafe dynamic query or rendering path was identified.
- **Policy/cancellation rules:** Ownership, cancellable status, complex policy/global fallback, and deadline checks are correctly enforced server-side. `CANCELLATION_DEADLINE_PASSED` and `USER_CANCELLATION_DISABLED` have structured server error codes.
- **Security findings:** No HIGH or CRITICAL security defect was verified. M-02 is a user-facing localization/security-consistency gap, not an authorization bypass.

## Architecture Assessment

The dashboard follows the intended layered path: controller -> query/handler -> persistence abstractions -> contracts. The controller is thin; dashboard composition is located in the application handler, and response contracts are explicitly JSON-named. The cancellation handler similarly owns business-rule orchestration rather than the controller.

The React implementation follows the project layout/page/feature-query pattern and shares the user-dashboard query between the shell and dashboard page. The main architectural concern is that this shared optional aggregate is also treated as a hard dependency for the entire user shell (M-05). The active-block name hydration is also a less-scalable aggregate composition approach (M-06).

No schema or migration is introduced by EPIC-13.

## Functional Assessment

- The dashboard shows welcome identity, upcoming items, history summary/recent history, blocks, loading skeletons, empty states, and generic translated request errors.
- The user shell has desktop sidebar/mobile drawer navigation to home, discovery, reservations, history, recurring reservation, and profile.
- The cancellation dialog offers an optional reason, prevents submit while pending, resets/closes on success, and only enables cancellation for `Pending`/`Confirmed` reservations.
- The backend immediately persists cancellation, and active availability queries exclude cancelled reservations.
- M-01 breaks the intended recurring creation handoff; M-02 breaks selected-language error presentation; M-03 and M-05 affect cross-page correctness/resilience.

## Testing Assessment

- **Unit:** `GetUserDashboardHandlerTests` cover no-data, upcoming/history, active blocks, and unknown user. `CancelMyReservationHandlerTests` cover core ownership/state/policy paths. The frontend dashboard hook and cancellation hook request are tested.
- **Integration:** The focused dashboard aggregate endpoint and cancellation policy paths pass (see Validation Results). Existing integration coverage verifies user cancellation within and beyond the policy deadline.
- **Missing coverage:** No direct dashboard-page, user-reservations-page, or user-layout test was found. The cancellation hook does not test query invalidation. No dedicated EPIC-13 E2E specification exists.
- **Cross-story tests:** There is no test that combines dashboard summary -> user reservations -> cancellation -> updated dashboard/history/availability.

## Data & Database Assessment

EPIC-13 changes no entity, migration, or database schema. Reservation cancellation delegates data integrity/business rules to the existing reservation/cancellation-policy implementation and persists through the unit of work. The aggregate has an N+1 read pattern for block complex names (M-06); no data corruption or concurrency issue specific to the aggregate was verified.

## Frontend Assessment

The user dashboard uses MUI cards, responsive grid breakpoints, a permanent desktop drawer, and a temporary mobile drawer. MUI dialog/table/text field primitives provide baseline accessible semantics; no critical keyboard or labeling issue was verified. Translation keys exist for dashboard content in all supported locales.

The user experience remains incomplete in cancellation failure states due to M-02, and M-01 makes the recurring card's primary action misleading. M-05 makes a nonessential dashboard request a portal-wide availability dependency.

## Performance Assessment

- **Backend/database:** Upcoming/history data are fetched through existing paged repository calls. Active blocks create the N+1 complex-name lookup described in M-06.
- **Frontend:** The dashboard uses one aggregate request rather than independent summary calls, which is positive. Cancellation availability invalidation is broad (`['court-availability']`) but correct; it may refresh more complex availability caches than necessary.
- **End-to-end:** The aggregate is mounted by both `UserLayout` and the dashboard page under one React Query key, so the fetch is deduplicated/cached. It should nevertheless be decoupled from non-dashboard route availability (M-05).

## Observability Assessment

The Epic introduces no specific telemetry, metric, or audit event. Existing error middleware returns structured error envelopes with trace IDs, allowing request failures to be correlated. No high-risk sensitive data logging was observed in the new dashboard/cancellation paths. Product/operational telemetry for dashboard failures is not required for the present scope, but M-05 makes aggregate-endpoint health relevant to portal usability.

## Regression Risks

1. Shared `UserLayout` behavior can make every user page unavailable when the dashboard aggregate fails (M-05).
2. Reservation mutations can leave dashboard summaries inconsistent unless all user-affecting mutations invalidate the aggregate key (M-03).
3. New cancellation error codes can regress localization unless the centralized mapping and locale-key parity are maintained (M-02).
4. Changes to the recurring booking route can silently break the dashboard handoff without page/E2E coverage (M-01, M-04).
5. The N+1 lookup grows with the number of active blocks and can affect user-portal latency (M-06).

## Documentation Assessment

`src/mova-web/README.md` correctly documents the `/user` routes and their access model. The Epic technical note to update it is therefore satisfied. The user-dashboard API is not in `.ai-kit/docs/architecture/API-DESIGN.md`, and the delivery records still represent the Epic/stories as ready rather than implemented-with-audit-gaps (L-01).

## Positive Findings

- The dashboard endpoint returns a coherent aggregate contract for the user identity, paged upcoming reservations, history summary, and active blocks.
- Application handlers correctly preserve layer boundaries and reuse reservation mapping/pagination conventions.
- Self-service cancellation protects ownership by returning not-found for non-owned reservations, validates allowed states, and enforces the effective complex cancellation policy.
- The UI uses a responsive MUI layout with a persistent desktop navigation and usable mobile drawer.
- Existing backend unit/integration tests and focused frontend hook tests pass for the main aggregate/cancellation paths.
- The frontend README has been updated with the delivered user routes.

## Validation Results

| Validation | Result |
|---|---|
| `dotnet build src/Mova.Api/Mova.Api.csproj` | PASSED — 0 warnings, 0 errors |
| `dotnet test tests/Mova.UnitTests/Mova.UnitTests.csproj` | PASSED — 432/432 |
| Focused dashboard/cancellation backend unit tests | PASSED — 11/11 |
| `dotnet test tests/Mova.IntegrationTests/Mova.IntegrationTests.csproj` | FAILED — 157/158; unrelated `CourtsControllerTests.Update_WithValidData_ReturnsUpdatedCourt` expected 200 and received 400 |
| Focused dashboard/cancellation integration tests | PASSED — 4/4 |
| `npm run lint` | PASSED |
| `npm run build` | PASSED — Vite reported the existing >500 kB chunk warning |
| Focused Vitest (`useUserDashboard`, `reservationApi`) | PASSED — 18/18 |
| Full Vitest (`npx vitest run --pool=threads`) | FAILED — 243/252; nine timeouts in unrelated home/complex-admin/business-hours/recurring page tests; no EPIC-13 page test exists |
| E2E | NOT EXECUTED — no dedicated EPIC-13 Playwright specification exists; required user journey remains unproven |

## Epic Score

| Dimension | Score |
|---|---:|
| Requirements completeness | 80 |
| Functional correctness | 78 |
| Security | 92 |
| Architecture | 84 |
| API consistency | 89 |
| Database/data integrity | 91 |
| Frontend | 78 |
| Testing | 57 |
| Performance | 82 |
| Observability | 84 |
| Documentation | 80 |
| Production readiness | 73 |

**Overall risk indicator: 81/100.** The implementation has sound core behavior, but the medium functional, resilience, performance, and coverage gaps make release approval premature.

## Final Verdict

**VERDICT: CHANGES REQUESTED**

CRITICAL: 0  
HIGH: 0  
MEDIUM: 6  
LOW: 1  
INFO: 0

Main risks:

1. The recurring-booking dashboard action takes the user to the wrong flow.
2. Policy/deadline errors appear in English for users of other supported locales.
3. The portal can show stale dashboard data after cancellation, or become unavailable across child routes if the aggregate fails.

Main missing requirements:

1. Direct dashboard/user-layout/cancellation-dialog tests and the required authenticated E2E journey.
2. A correct dashboard entry point for recurring booking and localized cancellation error states.

Main cross-story issue:

US-074 mutates the reservation data summarized by US-072, but does not invalidate the shared `user-dashboard` query; no cross-story browser test would catch the resulting inconsistency.
