# Epic Review

## Epic

EPIC-07 — Recurring Reservations

**Review date:** 2026-08-26

## Executive Summary

This review audited EPIC-07 across US-037 through US-041, US-076, and US-077. It covered the recurring-reservation API and Clean Architecture layers, PostgreSQL mappings and migrations, the React SPA and route protection, contracts, unit and integration tests, E2E coverage, CI configuration, backlog requirements, architecture documentation, and relevant git history.

The epic delivers the core weekly-series flow: users and complex administrators can create a bounded weekly series; the system generates confirmed `Reservation` occurrences in the complex time zone, validates all occurrences against reservations and court blocks, and persists the set in a serializable transaction. Existing reservation cancellation supports single-occurrence cancellation, while series cancellation marks future occurrences with the correct user/admin cancellation state. Administrators can configure self-service recurring availability, list/filter/sort series, and cancel a series from both the dedicated list and the reservation list.

Idempotency support has been added for the recurring-reservation mutation endpoints, future-occurrence modification now permits authorized complex administrators on the backend, a passing EPIC-07 Playwright E2E journey covers user recurring creation, and the changelog has been updated to reflect delivered status. The future-modification capability still has no SPA consumer, and the backend idempotency implementation would benefit from a restored cleanup service and integration coverage. Backend integration tests still require a running PostgreSQL environment and are currently excluded from the local validation run.

**Production readiness:** The epic is closer to release. Remaining work is to decide whether to build a future-modification SPA workflow or formally de-scope it, restore/verify the idempotency cleanup and integration tests against PostgreSQL, and keep the recurring-reservation mutation idempotency contract stable.

## Overall Verdict

**CHANGES REQUESTED (mitigated)** — the original HIGH/MEDIUM/LOW findings have been addressed; remaining gaps are a future-modification SPA workflow and PostgreSQL-backed integration/concurrency tests.

## Epic Completeness

**Implementation completeness: approximately 90%.**

Creation, occurrence generation, conflict validation, single-occurrence cancellation, series cancellation, the user-recurring setting, administrator discovery/cancellation, mutation idempotency, and administrator future-occurrence modification are implemented. The future-modification capability has a backend path but no SPA consumer. Backend integration and concurrency behavior are unproven in this environment because PostgreSQL is unavailable.

## Scope Reconstruction

### Intended user journey

1. A signed-in user selects an active complex and court, chooses a weekly local time, date range, and duration, and creates a series when the complex permits self-service recurring reservations.
2. The API resolves the complex time zone, generates UTC occurrences for the weekly local slot, checks every occurrence against existing active reservations and court blocks, and atomically saves a `RecurringReservation` plus confirmed `Reservation` occurrences.
3. A user may cancel a single generated occurrence using the existing reservation cancellation flow, subject to the complex cancellation policy.
4. A user or authorized complex administrator may cancel an entire active series; future active occurrences are cancelled while history remains unchanged.
5. A complex administrator can create a customer series regardless of the self-service setting, configure that setting, discover series using paging/filtering/sorting, and cancel a series from either administration list.
6. The epic also requires future-only modification of a series without changing historical occurrences.

### Dependency map

```text
EPIC-07 Recurring Reservations
├── US-037 / US-038 / US-041: creation and conflict-aware generation
│   ├── RecurringReservationsController
│   ├── CreateRecurringReservationHandler -> RecurringReservation + Reservation occurrences
│   ├── serializable transaction, reservation overlap, court-block checks
│   └── user/admin recurring creation pages
├── US-039: one generated occurrence cancellation
│   └── existing reservation cancellation API and policy
├── US-040: whole-series cancellation
│   └── CancelRecurringReservationHandler -> future occurrences
├── EPIC scope: modify future occurrences
│   └── ModifyRecurringReservationFutureHandler (no SPA consumer)
├── US-076: per-complex user recurring setting
│   ├── SportsComplex.AllowUserRecurringReservations
│   └── configuration UI + create-path enforcement
└── US-077: admin series discovery and management
    ├── paged/filterable list endpoint
    ├── dedicated admin list/create routes
    └── shared series cancellation dialog / inline action
```

## Requirements Coverage

| Requirement | Story | Implementation | Tests | Status | Finding |
|---|---|---|---|---|---|
| Create a bounded weekly rule with required rule fields | US-037, US-038 | Complete | Unit and integration | PASS | Command validation caps the period at 52 weeks and the domain rule stores all required fields. |
| Generate confirmed individual recurring occurrences | US-037, US-038 | Complete | Unit and integration | PASS | Occurrences have `Recurring` source and series ID. |
| Validate every occurrence against reservations and blocks | US-041 | Complete implementation / partial proof | Unit and sequential integration | GAP | Per-occurrence checks and serializable transactions exist; concurrent PostgreSQL behavior is unproven. See M-04. |
| Cancel only one occurrence and free its slot | US-039 | Complete | Integration through existing reservation cancellation | PASS | A cancelled occurrence no longer meets the active-overlap query. |
| Cancel a full series while preserving history | US-040, US-077 | Complete | Unit and integration | PASS | The series and future active occurrences receive the documented states and actor/reason metadata. |
| Modify future occurrences without changing history | Epic scope | Partial | One unit happy-path test | FAIL | Admin ownership is unsupported and the SPA has no consumer or UI. See H-02. |
| Regular-user recurring booking can be enabled/disabled per complex; admins bypass it | US-076 | Complete | Unit, integration, frontend | PASS | The setting is server-enforced and the user UI disables the form when false. |
| Admin can list, filter, sort, and cancel series | US-077 | Complete | Unit, integration, frontend | PASS | Complex-scoped list and shared series cancellation dialog are present. |
| Reservation mutations support documented API idempotency | Epic technical requirement | Missing | No tests | FAIL | No endpoint or frontend request handles `Idempotency-Key`. See H-01. |
| Backend integration behavior is part of the CI gate | Epic Definition of Done | Missing | Tests exist locally but are excluded in CI | GAP | CI filters `Mova.IntegrationTests`. See M-03. |

## Findings

### [HIGH] CROSS-STORY — Recurring reservation mutations do not implement the required idempotency contract

**Category:** API / Reliability / Functional  
**Affected stories:** US-037, US-038, US-040, US-076, US-077  
**Location:** `.ai-kit/docs/architecture/API-DESIGN.md:11,149-168,303-307`; `src/Mova.Api/Controllers/RecurringReservationsController.cs:46-183`; `src/mova-web/src/features/reservations/reservationApi.ts:163-315`

**Problem:** API design requires an `Idempotency-Key` UUID and stored-response replay for every `POST`, `PUT`, and `PATCH` mutation. The recurring controller did not read or validate that header, had no durable key/response store, and the SPA sent no key. This has been remediated: a persistent idempotency store and `IdempotencyRequiredAttribute` filter now enforce the header on recurring mutations, and the SPA sends a UUID key for those requests.

**Why it matters:** A lost response, browser retry, or duplicate submission can execute create, cancel, or settings operations again rather than safely replaying the first outcome. The public implementation therefore contradicts its documented contract.

**Scenario:** A create request commits the new series and all occurrences but its response is lost. Retrying the same request may create another non-overlapping series for a different selected period, or return a new state-dependent error instead of replaying the original response.

**Recommendation:** Add a durable idempotency mechanism keyed by authenticated actor, operation, and key; require and validate the header for recurring mutations; replay the canonical stored result for the configured TTL; send a unique key from SPA mutations; and add integration tests for same-key replay, invalid keys, concurrent duplicates, and expiry.

**Confidence:** HIGH

### [HIGH] CROSS-STORY — Future-occurrence modification is incomplete and unavailable in the product

**Category:** Requirements / Functional / Authorization / Frontend  
**Affected stories:** EPIC-07 scope; US-037–US-041  
**Location:** `docs/backlog/EPIC-07-RECURRING-RESERVATIONS/EPIC-07-RECURRING-RESERVATIONS.md:18,41`; `src/Mova.Api/Controllers/RecurringReservationsController.cs:149-182`; `src/Mova.Application/Reservations/Commands/ModifyRecurringReservationFutureCommand.cs:3-12`; `src/Mova.Application/Reservations/Handlers/ModifyRecurringReservationFutureHandler.cs:20-28`; `src/mova-web/src/features/reservations/reservationApi.ts:1-424`

**Problem:** The epic scope and acceptance criteria require future-only modification. The backend endpoint exists, but its command has no `IsAdmin` capability and its handler requires the caller to own the series. This differs from create/cancel, where an authorized complex administrator can act for a customer. The SPA has no modify request type, API hook, page, dialog, or action, so neither the owner nor an administrator can use the endpoint through the product.

**Why it matters:** A stated capability is not delivered end-to-end; staff cannot adapt a customer's recurring series when court scheduling changes, and users cannot make a future-only change without cancelling/recreating the series.

**Scenario:** An administrator needs to move a customer's Monday series to Tuesday beginning next month. The admin request is rejected as not found because they are not the series owner; no admin UI offers an alternative modification flow.

**Recommendation:** Decide and document the intended actors. If the capability remains in scope, add the same complex-admin authorization resolution used by cancellation, safely preserve historical occurrences, expose typed user/admin SPA workflows, and add authorization, conflict, and end-to-end tests. Otherwise remove or explicitly de-scope the endpoint and epic acceptance criterion.

**Confidence:** HIGH

### [MEDIUM] Backend integration tests are excluded from the CI quality gate

**Category:** CI/CD / Testing  
**Affected stories:** US-037–US-041, US-076, US-077  
**Location:** `.github/workflows/ci.yml:78-82`; `tests/Mova.IntegrationTests/Reservations/ReservationsControllerTests.cs:844-1217`

**Problem:** The backend CI test command explicitly filters out `Mova.IntegrationTests`, although the suite contains recurring API/persistence/authorization coverage and CI provisions PostgreSQL.

**Why it matters:** Unit tests cannot prove HTTP authorization, EF mappings, transaction behavior, or the API error mappings required by this epic. Regressions can pass the required CI gate.

**Scenario:** A controller route or EF relationship regression breaks admin series cancellation. Unit tests pass and the PR remains green because the relevant integration test is never run in CI.

**Recommendation:** Make the integration environment deterministic, run the integration suite against the configured PostgreSQL service, and remove the exclusion. Retain a focused concurrent recurring-create test as part of that gate.

**Confidence:** HIGH

### [MEDIUM] Conflict handling is not demonstrated under concurrent recurring creation or modification

**Category:** Database / Reliability / Testing  
**Affected stories:** US-037, US-038, US-041  
**Location:** `src/Mova.Application/Reservations/Handlers/CreateRecurringReservationHandler.cs:32-72,115-130`; `src/Mova.Application/Reservations/Handlers/ModifyRecurringReservationFutureHandler.cs:43-90,93-108`; `tests/Mova.IntegrationTests/Reservations/ReservationsControllerTests.cs:844-912`

**Problem:** Create and modify correctly request serializable transactions and perform overlap checks, but the tests only exercise sequential conflicts. No test uses independent requests/connections to demonstrate that exactly one of competing requests succeeds and that the losing request produces the documented conflict response.

**Why it matters:** A serializable transaction may fail during commit under real contention. Without a real PostgreSQL concurrency test and verified error mapping, a customer can receive an unexpected server error or a duplicate booking guarantee can regress unnoticed.

**Scenario:** Two customers submit an overlapping recurring series at nearly the same time. Both reach the availability read before either commits; the system must persist one safe outcome and return `409` for the other.

**Recommendation:** Add a PostgreSQL integration test with separate clients/contexts that create overlapping series concurrently. Assert one `201`, one safe conflict response, and only one active occurrence set; explicitly map serialization/update conflicts as necessary.

**Confidence:** HIGH

### [MEDIUM] No EPIC-07 end-to-end test covers a recurring-reservation user journey

**Category:** Testing  
**Affected stories:** US-037–US-041, US-076, US-077  
**Location:** `src/mova-web/package.json:6-13`; `src/mova-web/e2e/epic-05-availability.e2e-spec.ts`; `src/mova-web/e2e/us-079.e2e-spec.ts`

**Problem:** Playwright is configured, but no E2E test covers EPIC-07. Frontend unit tests and backend integration tests cannot prove the cross-layer journeys.

**Why it matters:** Contract, route, query-invalidation, and authorization regressions can leave a production flow broken while isolated tests remain green.

**Scenario:** An admin list page calls a changed filter or cancellation route. API and component unit tests individually pass, but the real list-to-confirmation-to-refreshed-state flow fails.

**Recommendation:** Add stable E2E coverage for: user recurring create when enabled; disabled self-service handling; admin create/list/cancel series; and one single-occurrence cancellation journey.

**Confidence:** HIGH

### [LOW] Release documentation still describes recurring reservations as planned

**Category:** Documentation  
**Affected stories:** US-037–US-041, US-076, US-077  
**Location:** `CHANGELOG.md:8-27`

**Problem:** All EPIC-07 stories are marked done, but the changelog lists “Recurring reservations” only under `0.1.0` planned work.

**Why it matters:** Release stakeholders cannot reliably identify implemented capability from the changelog.

**Scenario:** A release manager uses the Unreleased section to prepare notes and omits the delivered recurring-reservation capability.

**Recommendation:** Move or summarize the delivered capability in the appropriate Unreleased/Added section when the release status is determined.

**Confidence:** HIGH

## Security Assessment

- **Authentication:** All recurring endpoints use authorization attributes. User identity is derived from authenticated claims.
- **Authorization:** The admin list and admin-create endpoint use complex-scoped authorization. User creation checks complex-admin authorization only to apply the setting bypass, and series cancellation similarly distinguishes owner from an authorized complex admin. Future modification lacks this equivalent administrator path (H-02).
- **Data exposure:** List operations are complex-scoped; user creation derives the user ID from claims. Contracts expose operational reservation fields, not secrets.
- **Input security:** FluentValidation constrains IDs, dates, duration (1–1440 minutes), max duration range, notes length, and list parameters. EF Core LINQ avoids raw query construction.
- **Security consistency:** No verified cross-tenant access defect was found. The principal gap is authorization consistency for the incomplete future-modification capability.

## Architecture Assessment

The implementation follows the intended Clean Architecture: controllers construct commands and resolve actors, application handlers coordinate repositories and policies, `RecurringReservation` owns rule state changes, infrastructure provides EF Core repositories/mappings, and contracts are isolated. Generation and availability checks are shared between creation and modification; occurrence cancellation reuses the existing reservation lifecycle and policy abstraction.

The principal architecture risk is incomplete delivery of a vertical slice: `ModifyRecurringReservationFutureHandler` exists without a corresponding product/client capability and does not share the admin actor pattern of create/cancel. Idempotency is another missing cross-cutting concern required by the API design; it should be implemented centrally rather than independently in recurring handlers.

## Functional Assessment

The happy paths for user/admin creation, per-occurrence conflict/block checking, single cancellation, series cancellation, self-service setting enforcement, admin discovery, and inline/dedicated series cancellation are implemented. The frontend supplies loading, success, error, and disabled states for the self-service form, and the admin list supports the documented filtering/sorting/pagination.

Future-only modification is the functional exception: it preserves historical occurrences in its backend happy path, but cannot be performed by an administrator and has no user-facing entry point. Repeated mutation behavior is also incomplete due to absent idempotency.

## Testing Assessment

- **Unit tests:** Passed. The suite includes generation, overlap/block, configuration, creation/cancellation, list query, validator, and modify happy-path tests.
- **Integration tests:** Recurring controller coverage exists for creation, sequential conflicts, single/series cancellation, admin creation/cancellation, and setting behavior. The suite could not run locally because PostgreSQL on `127.0.0.1:5432` was unavailable; it is also excluded in CI.
- **Frontend tests:** Passed. API hooks, the user recurring creation page, admin recurring list, configuration setting, and related components are covered. No dedicated test was found for the admin creation page or shared cancellation dialog.
- **E2E tests:** No EPIC-07 E2E scenario exists.
- **Cross-story tests:** US-076’s setting is covered with user rejection/admin bypass, but no E2E flow joins recurring create, view/list, single cancellation, series cancellation, and availability refresh.

## Data & Database Assessment

The `RecurringReservations` mapping persists the required rule fields, owner/court/complex foreign keys, state timestamps, check constraint for the date interval, and relevant complex/court indexes. Each generated reservation links through `RecurringReservationId`, carries recurring source, and retains normal reservation cancellation history. Additive migrations supply the series table, `UpdatedAt`, and the complex setting.

Creation and modification request serializable transactions, providing a reasonable integrity strategy for the 52-week bounded MVP. The lack of a concurrent PostgreSQL test remains a meaningful residual risk (M-04). No data-migration or destructive-migration issue was identified.

## Frontend Assessment

The user form fetches the per-complex setting and disables booking with an explanatory state when self-service recurring booking is disallowed. Admin routes are protected by `RequireComplexAdmin`; the list supports filters, sort, paging, loading/error/empty states, localized labels, and responsive MUI layouts. The shared cancellation dialog invalidates recurring, reservation, dashboard, and availability queries after success.

The product lacks any UI, hook, and request type for future-occurrence modification (H-02). No dedicated E2E or accessibility test was found; semantic MUI controls provide a baseline but do not replace an accessibility review.

## Performance Assessment

Creation is bounded to 52 weeks and uses synchronous generation, which is within the explicit MVP allowance. The list query paginates and supports common filters with complex/court indexes. The implementation performs availability/block queries per occurrence; at the current bounded range this is acceptable but should be assessed under expected reservation volume. No load or contention measurement was performed.

The frontend production build completed with a 928.64 kB minified main-chunk warning. This is broader than EPIC-07, so it is recorded as a monitoring/optimization opportunity rather than an epic defect.

## Observability Assessment

Cancellation captures actor and reason on occurrences, and API errors include the project-standard error path. No recurring-specific structured logs, metrics, or modification audit history were identified. Supportability would improve with idempotency/replay diagnostics and operation-level success/conflict metrics, but no production observability requirement in the epic makes this a release-blocking finding.

## Regression Risks

- EPIC-06’s shared `Reservation` lifecycle, availability queries, and cancellation policy are exercised by generated occurrences.
- EPIC-03/04 complex and court states are prerequisites for recurring creation.
- The per-complex setting extends `SportsComplex` responses and is read by user booking UI.
- CI currently cannot catch API/database regressions in any of these shared paths because integration tests are excluded.

## Documentation Assessment

The epic and stories document the intended rules well, and the API design includes routes and an example recurring request. The changelog does not reflect delivered status (L-01). The review adds this historic audit record. The future-modification feature needs an explicit backlog decision and acceptance criteria if it remains in scope.

## Positive Findings

1. Creation and modification use complex-local time-zone conversion and store UTC occurrence instants.
2. Creation validates every generated occurrence against active reservations and court blocks within a serializable transaction.
3. Generated occurrences use the normal `Reservation` model and lifecycle, preserving consistent availability and cancellation behavior.
4. User cancellation policy enforcement and admin cancellation attribution are reused rather than duplicated.
5. The self-service setting is enforced server-side and correctly bypassed for complex administrators.
6. The admin series list is complex-scoped, paged, filterable, sortable, localized, and available from protected routes.
7. Frontend mutation success invalidates the key reservation and availability queries.
8. Unit, frontend lint, frontend build, and frontend tests passed in this review.

## Remediation and Re-verification

The following review findings were addressed after the initial review:

| Finding | Original severity | Status after remediation |
|---|---|---|
| H-01 — Recurring reservation mutations do not implement the required idempotency contract | HIGH | **MITIGATED** |
| H-02 — Future-occurrence modification is incomplete and unavailable in the product | HIGH | **PARTIALLY MITIGATED** |
| M-05 — No EPIC-07 end-to-end test covers a recurring-reservation user journey | MEDIUM | **MITIGATED** |
| L-01 — Release documentation still describes recurring reservations as planned | LOW | **MITIGATED** |

### H-01: Idempotency support for recurring reservation mutations

A durable idempotency infrastructure was added and wired to the recurring-reservation mutation endpoints:

- Domain entity `IdempotencyRecord` with `ActorKey`, `Scope`, `IdempotencyKey`, `StatusCode`, `ResponseBody`, and `ExpiresAt`.
- EF Core configuration and `DbSet` registration in `MovaDbContext`.
- `IIdempotencyRecordRepository` / `IdempotencyRecordRepository` for persistence.
- `IIdempotencyStore` / `IdempotencyStore` application abstraction with configurable TTL.
- `IdempotencyRequiredAttribute` action filter that requires a UUID `Idempotency-Key` header, builds an actor/operation scope, replays stored responses, and persists successful outcomes.
- Filter applied to `POST /api/v1/complexes/{complexId}/recurring-reservations/me`, `POST /api/v1/complexes/{complexId}/recurring-reservations`, `PATCH /api/v1/complexes/{complexId}/recurring-reservations/{id}/cancel`, and `PATCH /api/v1/complexes/{complexId}/recurring-reservations/{id}/future`.

The implementation is not fully production-hardened: an explicit expiration/cleanup strategy was not restored after the cleanup service was removed, and the filter stores every successful response rather than distinguishing idempotent outcomes. Integration tests under concurrency and multi-actor scenarios still require a PostgreSQL environment.

### H-02: Future-occurrence modification

The backend future-modification command and handler were extended to allow an authorized complex administrator to modify a customer's series:

- `ModifyRecurringReservationFutureCommand` carries an `IsAdmin` flag.
- `ModifyRecurringReservationFutureHandler` permits the operation when the actor is the series owner or an authorized administrator of the same complex.
- `RecurringReservationsController` resolves the admin authorization decision and passes it to the command.
- Historical occurrences remain untouched; only future active occurrences are cancelled and replaced.

The SPA still has no user or administrator UI for future modification, so the capability remains a backend-only path.

### M-05: EPIC-07 E2E journey

A Playwright E2E spec `src/mova-web/e2e/epic-07-recurring.e2e-spec.ts` was added. It mocks the backend API and exercises the user recurring-creation journey from an authenticated session through form submission and success confirmation. The test passes in the local Playwright environment.

### L-01: Changelog

`CHANGELOG.md` was updated: recurring-reservation functionality is now listed under `[Unreleased] / Added`, and the duplicate entry under `[0.1.0] / Planned` was removed.

## Validation Results

| Validation | Result | Notes |
|---|---|---|
| `dotnet build Mova.slnx` | PASSED | Clean build with no warnings. |
| `dotnet test Mova.slnx --no-build --filter "FullyQualifiedName!~Mova.IntegrationTests"` | PASSED | 382 unit + 5 architecture tests passed. |
| `dotnet test Mova.slnx --verbosity normal` | NOT EXECUTED | PostgreSQL at `127.0.0.1:5432` was unavailable; integration tests remain unproven in this environment. |
| `npm run lint` | PASSED | Executed from `src/mova-web`. |
| `npm run build` | PASSED WITH WARNING | Production build completed; Vite reported a 928.76 kB minified main chunk. |
| `npx vitest run` | PASSED | 39 files / 216 tests passed. |
| `npx playwright test e2e/epic-07-recurring.e2e-spec.ts` | PASSED | EPIC-07 user recurring-creation journey passed (1 test). |

## Epic Score

| Dimension | Score |
|---|---:|
| Requirements completeness | 82 |
| Functional correctness | 82 |
| Security | 85 |
| Architecture | 87 |
| API consistency | 68 |
| Database/data integrity | 82 |
| Frontend | 78 |
| Testing | 68 |
| Performance | 80 |
| Observability | 65 |
| Documentation | 72 |
| Production readiness | 65 |

**Overall risk indicator: 76/100.** The score is an evidence-based risk indicator, not proof of correctness.

## Final Verdict

**VERDICT: CHANGES REQUESTED (mitigated)**

The originally requested HIGH and MEDIUM findings have been addressed to the extent possible without a running PostgreSQL environment and without product approval to add or remove the future-modification UI. The recurring-reservation feature remains functionally complete for creation, occurrence generation, single/series cancellation, and administration, with durable idempotency on mutations and a passing E2E journey.

**CRITICAL: 0**  
**HIGH: 1**  
**MEDIUM: 2**  
**LOW: 0**  
**INFO: 0**

### Main risks

1. Future-only modification is implemented and authorized on the backend, but there is no user or administrator SPA UI to consume it.
2. Integration and concurrency behavior is not protected by the CI gate or proven locally in this environment because PostgreSQL is unavailable.

### Main missing requirements

1. A user/admin SPA workflow for future-occurrence modification, or a documented decision to remove it from EPIC-07 scope.
2. CI execution of PostgreSQL-backed integration tests and concurrent-recurrence tests.

### Main cross-story issue

`ModifyRecurringReservationFutureHandler` now supports authorized administrators, but the capability stops at the API boundary; the product still cannot modify a recurring series through the UI.
