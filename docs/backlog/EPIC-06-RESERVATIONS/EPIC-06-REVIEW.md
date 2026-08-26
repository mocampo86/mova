# Epic Review

## Epic

EPIC-06 — Reservations

**Review date:** 2026-08-24

## Executive Summary

This review audited EPIC-06 across US-029 through US-036, US-073, and US-079. It covered the .NET API, application/domain/infrastructure layers, PostgreSQL mapping and migrations, React SPA, contracts, unit and integration tests, E2E coverage, CI, relevant architecture documentation, and git history.

The epic provides the core reservation lifecycle: users can make an auto-confirmed reservation, view upcoming and historic reservations, and cancel within a complex-scoped policy; complex administrators can list, manually create, cancel, and mark reservations completed or no-show. Creation checks active complex/court/user state, blocked users, court blocks, and overlapping reservations in a serializable transaction. The admin calendar and user-search UI are implemented with timezone-aware date filtering, reusable query hooks, and complex-scoped authorization.

The implementation is not production-ready as documented. The API design requires idempotency for reservation creation and mutations, but neither the API nor the frontend sends, validates, or persists an `Idempotency-Key`. In addition, the user-history UI omits cancellation reason and actor despite the API providing both fields and US-031 requiring them. Finally, the CI workflow intentionally excludes the integration suite that verifies API authorization, persistence, conflict handling, and cancellation policy behavior.

**Production readiness:** Changes are required before release. Address idempotency and reinstate the integration-test quality gate before treating the reservation lifecycle as release-ready.

## Overall Verdict

**CHANGES REQUESTED**

## Epic Completeness

**Implementation completeness: approximately 84%.**

The planned API and UI flows are broadly present, and the principal business rules are implemented. The documented idempotency requirement is absent across all reservation mutations; the history presentation does not meet its cancellation-detail criterion; and automated release confidence is reduced by CI excluding integration tests and by missing epic-wide E2E coverage.

## Scope Reconstruction

### Intended user journey

1. A signed-in user discovers an available court slot and creates a reservation for themselves.
2. The system validates the active complex, active court, active user, blocked-user state, court blocks, and overlapping active reservations; it confirms the MVP reservation.
3. The user views upcoming reservations, cancels an eligible one subject to the complex cancellation policy, and sees historical completed, cancelled, or past reservations.
4. A complex administrator lists reservations, creates an administrative reservation for a selected user, cancels a reservation with a reason, and records attendance as completed or no-show.
5. The administrator can switch the reservations page between list and daily-calendar views, select a date and court, and inspect free or reserved slots.
6. An administrator can search eligible users by name, email, or phone before creating a manual reservation.

### Dependency map

```text
EPIC-06 Reservations
├── US-029 User reservation creation
│   ├── ReservationsController -> CreateReservationHandler -> Reservation
│   ├── active complex/court/user + blocked-user validation
│   └── serializable overlap and court-block checks
├── US-030 / US-031 User lists
│   └── upcoming/history repository queries -> user reservation pages
├── US-032 User cancellation
│   └── ICancellationPolicy -> complex override / global fallback
├── US-033 / US-034 / US-035 / US-036 Administration
│   └── complex-scoped authorization -> list/create/cancel/status handlers
├── US-073 Daily calendar
│   ├── complex reservation list
│   └── public availability per court -> ReservationCalendar
└── US-079 User selection
    └── complex-scoped, rate-limited user search -> autocomplete/dialog
```

## Requirements Coverage

| Requirement | Story | Implementation | Tests | Status | Finding |
|---|---|---|---|---|---|
| User creates an auto-confirmed reservation for an available slot | US-029 | Complete | Unit and integration | PASS | Active complex/court/user, blocked-user, overlap, and court-block checks are implemented. |
| Active reservations do not overlap and conflicts are protected transactionally | US-029, US-034 | Complete implementation / partial proof | Unit and sequential integration | GAP | Serializable transaction and overlap query exist, but no concurrent integration test proves the production behavior. See M-03. |
| User views upcoming reservations and can begin cancellation | US-030 | Complete | Unit/integration for API; no dedicated page test | PASS | Query limits records to the current user, future start time, and pending/confirmed status. |
| User reservation history includes cancellation reason and actor | US-031 | Complete | API and page tests | PASS | `UserHistoryPage` now renders cancellation actor and reason for cancelled reservations with safe fallbacks; tests pass. |
| User cancellation applies complex policy/global fallback | US-032 | Complete | Unit and integration | PASS | The handler evaluates `ICancellationPolicy` before cancelling the user's own pending/confirmed reservation. |
| Admin list/manual create/cancel/status operations are complex-scoped | US-033–US-036 | Complete | Unit and integration | PASS | `ComplexAdmin` authorization and handler ownership checks protect the administrative paths. |
| Daily calendar shows court/date-filtered free and reserved slots with a legend | US-073 | Complete | Component and utility tests | PASS | Calendar combines availability and reservation data, excludes cancelled reservations from occupied slots, and renders the legend. |
| Admin can search and select an eligible user for manual creation | US-079 | Complete | Frontend unit tests; E2E currently failing | PASS / test gap | Search is complex-scoped, rate-limited, validated, debounced, and supports autocomplete/dialog selection. |
| Reservation creation and mutations support API idempotency | Epic technical requirement | Missing | No tests | FAIL | API design requires `Idempotency-Key` for POST, PUT, and PATCH mutations, but no implementation exists. See H-01. |
| Backend integration behavior is part of the CI gate | Epic Definition of Done | Missing | Integration tests exist locally but are excluded in CI | GAP | CI explicitly filters `Mova.IntegrationTests`. See M-01. |

## Findings

### [HIGH] CROSS-STORY — Reservation mutations do not implement the required idempotency contract

**Category:** API / Reliability / Functional

**Affected stories:** US-029, US-032, US-034, US-035, US-036

**Location:** `.ai-kit/docs/architecture/API-DESIGN.md:11,303-307`; `src/Mova.Api/Controllers/ReservationsController.cs:74-165,182-228`; `src/mova-web/src/features/reservations/reservationApi.ts:94-160,319-424`

**Problem:** The API design requires an `Idempotency-Key` UUID for POST, PUT, and PATCH mutation endpoints and requires the server to replay the stored response for the same key. The reservation controller accepts no idempotency header and has no persistence/replay mechanism. The frontend mutation calls also send no such header. A repository-wide source and test search found no idempotency implementation or tests.

**Why it matters:** Retried browser requests, mobile reconnects, proxies, or duplicate clicks can re-execute mutation logic. A duplicate creation for an identical active time range normally conflicts, but duplicate status/cancellation processing and retries after a response is lost are not guaranteed to return the original response. More generally, the public API behavior contradicts the documented contract.

**Scenario:** The server commits a `PATCH` cancellation but the browser loses the response. The client retries the same operation; instead of replaying the first response as specified, it executes the endpoint again against changed state. The consumer cannot safely distinguish a completed first request from a new operation.

**Recommendation:** Implement a durable idempotency record keyed by authenticated actor, route/operation, and `Idempotency-Key`, storing the canonical response for the documented TTL. Require and validate the header on reservation POST/PATCH endpoints, send a generated UUID from each frontend mutation, and add integration tests for same-key replay, different-key behavior, invalid keys, concurrent duplicate requests, and expiry.

**Confidence:** HIGH

### [MEDIUM] Backend integration tests are excluded from the CI quality gate

**Category:** CI/CD / Testing

**Affected stories:** US-029–US-036, US-079

**Location:** `.github/workflows/ci.yml:78-82`

**Problem:** The backend CI `Test` step explicitly excludes `Mova.IntegrationTests`, despite provisioning PostgreSQL and setting the test connection string. The excluded suite includes reservation-controller coverage for creation, authentication, overlap rejection, user lists/history, cancellations, administrative creation, and status updates.

**Why it matters:** Unit tests and static build checks cannot prove controller authorization, EF Core query behavior, transaction behavior, persistence mappings, or error-to-HTTP mappings. Reservation behavior can regress while the required backend CI job remains green.

**Scenario:** A change to complex authorization or a repository query breaks an administrative reservation operation. Local unit tests pass, and CI still passes because the affected API/database test is filtered out.

**Recommendation:** Investigate the currently failing integration tests, make them deterministic against the configured PostgreSQL service, and remove the filter. Keep the integration suite as a required backend gate. Add a focused concurrent-create integration test to validate the serializable reservation conflict path.

**Confidence:** HIGH

### [MEDIUM] User history does not display the required cancellation details

**Category:** Functional / Frontend / Frontend-Backend Contract

**Affected stories:** US-031, US-032, US-035

**Location:** `docs/backlog/EPIC-06-RESERVATIONS/US-031.md:21-23`; `src/Mova.Contracts/Reservations/ReservationInfo.cs:49-59`; `src/mova-web/src/features/reservations/reservationTypes.ts:25-28`; `src/mova-web/src/pages/UserHistoryPage.tsx:59-90`

**Problem:** US-031 requires a cancelled reservation in history to show the cancellation reason and actor. The API contract and frontend type expose `cancellationReason`, `cancelledByUserId`, and `cancelledByUserName`, but `UserHistoryPage` renders only court, start, end, and status. Its test fixture contains a cancellation reason, but the test only asserts the status.

**Why it matters:** A user cannot determine why a reservation was cancelled or who performed an administrative cancellation, which fails the stated history requirement and weakens support transparency.

**Scenario:** An administrator cancels a user's future booking with a reason. The user opens reservation history and sees only “Cancelled by admin,” without the recorded reason or cancelling actor.

**Recommendation:** Add a cancellation-details column, accessible expandable row, or details view for cancelled entries. Render the reason and actor name with a clear fallback for older records, then add UI tests for user- and admin-cancelled reservations.

**Resolution:** Implemented. `UserHistoryPage` now includes a `Details` column that renders `Cancelled by: {{actor}}` and `Reason: {{reason}}` for `CancelledByUser` and `CancelledByAdmin` reservations, with `common.emptyValue` and `dashboard.noCancellationReason` fallbacks. English, Spanish, and Portuguese locale keys were added, and `UserHistoryPage.test.tsx` covers both populated and missing cancellation metadata. Frontend lint, build, and Vitest pass.

**Confidence:** HIGH

### [MEDIUM] CROSS-STORY — The serializable double-booking protection is not proven under concurrent requests

**Category:** Testing / Database / Reliability

**Affected stories:** US-029, US-034, US-073

**Location:** `src/Mova.Application/Reservations/Handlers/CreateReservationHandler.cs:74-106`; `tests/Mova.IntegrationTests/Reservations/ReservationsControllerTests.cs:92-119`; `.github/workflows/ci.yml:78-82`

**Problem:** Reservation creation correctly performs the overlap query, court-block query, and insert inside a serializable transaction. The available integration test proves a second *sequential* overlapping request returns `409`, but no test starts competing requests against separate contexts/connections. CI also excludes this integration suite.

**Why it matters:** The key epic acceptance criterion is concurrency protection. A fake or sequential test cannot demonstrate that PostgreSQL serialization failures are translated to the documented conflict outcome or that exactly one competing request succeeds.

**Scenario:** Two players submit the same last available slot almost simultaneously. Both requests reach the overlap query before either commits. The system must commit exactly one reservation and return a safe conflict response for the other.

**Recommendation:** Add a PostgreSQL integration test with two independent clients/contexts submitting the same range concurrently. Assert exactly one `201 Created`, one conflict response, and one persisted active reservation. Ensure a serializable transaction abort/`DbUpdateException` is consistently mapped to `409 RESERVATION_CONFLICT` rather than an unhandled server error.

**Confidence:** HIGH

## Security Assessment

- **Authentication:** All reviewed reservation endpoints require an authenticated caller. The `User` policy intentionally means any authenticated platform user; user-specific operations derive identity from JWT claims and query by reservation ownership.
- **Authorization:** Administrative routes use the database-backed `ComplexAdmin` requirement, which resolves the caller's association for the requested route `complexId`. Handlers also verify a reservation belongs to the target complex before admin cancellation or status updates.
- **Data exposure:** User list/history queries are scoped to the authenticated user. Administrative reservation and user-search routes are scoped to the authorized complex. The search endpoint returns selection-focused fields and has a fixed-window rate-limit policy.
- **Input security:** FluentValidation validates reservation commands and search query length/characters. EF Core LINQ query construction avoids SQL string concatenation.
- **Security consistency:** No verified privilege-escalation finding was identified. The absence of idempotency is primarily a reliability/API-contract defect, but safe retry behavior is also relevant to abuse-resistant mutation handling.

## Architecture Assessment

The implementation follows the expected layered design. Controllers translate HTTP requests to commands/queries and derive caller IDs. Application handlers orchestrate repositories and policy abstractions; `Reservation` encapsulates state transitions; infrastructure provides EF Core repositories and transaction management; contracts are isolated in `Mova.Contracts`.

The strongest architectural choices are the `ICancellationPolicy` abstraction with per-complex resolution/fallback, clear user/admin creation routes, and transactional conflict validation. The main architectural gap is the missing idempotency cross-cutting capability required by the API design. The proposed implementation should be shared infrastructure or an API/application concern rather than duplicated across individual reservation handlers.

## Functional Assessment

The happy paths are complete: user/manual creation, current/history views, user/admin cancellation, administrator status updates, calendar rendering, and user selection all have corresponding routes and UI. Cancelled reservations cease to block availability, and the calendar deliberately excludes them from occupied slots. User cancellation and admin cancellation apply distinct documented rules.

The user-history flow is incomplete because it does not surface cancellation details. Repeated-operation behavior is also incomplete because no idempotency mechanism exists. The implementation contains strong conflict logic, but concurrency behavior needs a real database test rather than inference from transaction configuration.

## Testing Assessment

- **Unit tests:** Domain entities, validators, creation/cancellation/status handlers, query handlers, cancellation-policy behavior, calendar utilities/components, search components, and API hooks have focused coverage.
- **Integration tests:** `ReservationsControllerTests` covers major creation, sequential conflict, query, cancellation, and administrative status paths. Important missing cases include concurrent creation, blocked-user/court-block API behavior, and a real serializable-conflict response.
- **Frontend tests:** Reservation API hooks, calendar utilities/component, admin reservation page, history page, autocomplete, and search dialog are covered. No dedicated `UserReservationsPage` test was found; the existing history page test does not assert cancellation details.
- **E2E tests:** Only US-079 has E2E coverage in the repository. The review run failed both US-079 tests before the reservation page became usable; the same run also failed two EPIC-05 tests and skipped two others. The failures must be investigated before a release claim is made.
- **Cross-story tests:** No E2E test covers discover slot -> create -> user upcoming -> cancel -> history -> admin calendar, and no concurrent API test proves the reservation conflict guarantee.

## Data & Database Assessment

The reservation table stores the documented ownership, time, status, source, cancellation, and recurrence fields. The mapping enforces `StartAt < EndAt` and includes an index on `(CourtId, StartAt, EndAt, Status)`, plus complex and user lookup indexes. Cancellation is soft-state based and preserves historical records and cancellation actor data.

Creation performs overlap checks under serializable isolation and checks blocks in the same transaction, providing a sound base for integrity. However, application-level overlap checks must be verified with concurrent PostgreSQL requests. No database-level exclusion constraint/trigger is present as an additional last-line defense; this is not reported as a defect because the documented design permits serializable/advisory-lock protection, but it increases the importance of concurrency integration coverage.

## Frontend Assessment

The reservation SPA uses TanStack Query hooks with appropriate query invalidation after create/cancel/status actions. The admin page preserves date/court filters while switching views, builds a calendar from reservation and availability data, supplies loading/error/empty states, and uses MUI’s responsive horizontal calendar layout. US-079 provides debounced autocomplete, a search dialog, blocked-user disabling, server-side search validation, and backend rate limiting.

User history omits cancellation reason and actor despite receiving both. Reservation pages have semantic inputs, dialog titles, and navigation structure, but no dedicated automated accessibility audit was found. The main tested frontend validation command passed; the E2E suite did not.

## Performance Assessment

Reservation lists are paginated and clamp page size to 100. Reservation lookup paths are filtered by tenant/user/court and use an overlap-friendly composite index. User autocomplete is debounced, paged, validated, and rate-limited on the server.

The frontend production build passed but emitted a 927.75 kB minified main-bundle warning. This is broader than EPIC-06 and is recorded as a monitoring/optimization opportunity rather than an epic finding. No load or contention test currently measures concurrent reservation creation.

## Observability Assessment

The API has centralized request/error logging and returns trace IDs through the documented error envelope. No explicit reservation audit log was identified for manual creation, cancellation, or attendance changes; cancellation itself retains an actor and reason in the reservation record. This is an operational improvement opportunity, not an explicit EPIC-06 acceptance-criteria failure.

## Regression Risks

1. Adding idempotency must not cache error responses incorrectly or allow one actor to replay another actor’s mutation; scope keys to the authenticated actor and operation.
2. Re-enabling integration tests must keep database isolation deterministic in CI and must not hide failures with test ordering.
3. Rendering cancellation details must tolerate older records with missing actor names/reasons and must not expose cancellation data outside the reservation owner or authorized complex administration scope.
4. Changes to the serializable transaction path must preserve court-block and blocked-user behavior, and must map database serialization failures to a client-safe conflict response.
5. Calendar/list changes must preserve complex-local date/time-zone semantics introduced by the availability epic.

## Documentation Assessment

The domain, database, API, authorization, and user-story documentation describe the reservation model, cancellation behavior, conflict rules, and time-zone requirements well. The API-design idempotency section is not reflected in the current implementation, creating a material documentation/behavior mismatch. Update API/OpenAPI documentation when idempotency is implemented to describe header requirements, replay semantics, error behavior, and TTL.

## Positive Findings

- Reservation creation validates active complex, court, user, blocked-user state, conflicts, and court blocks before persistence.
- Conflict and block checks execute with the insert inside a serializable transaction.
- User and administrator cancellation semantics are clearly separated, with a complex-scoped policy and global fallback for user cancellation.
- Reservation ownership and complex scoping are enforced in both policy and handler layers.
- Contracts consistently carry cancellation metadata and frontend types match them.
- Admin calendar logic excludes cancelled reservations from occupied slots and provides an always-visible legend.
- User search is scoped, validated, debounced, rate-limited, and avoids a raw free-text user-ID field.
- The frontend lint, production build, and full Vitest suite passed in this review.

## Validation Results

| Validation | Result | Notes |
|---|---|---|
| `dotnet build Mova.slnx --no-restore` | BLOCKED | Existing `Mova.Api` process (PID 329972) locked API output assemblies. The process was not stopped because it was not started by this review. |
| `dotnet test Mova.slnx --no-build --no-restore` | NOT EXECUTED | Skipped because the required build failed due to the external file lock. |
| `npm run lint` | PASSED | Executed in `src/mova-web`. |
| `npm run build` | PASSED WITH WARNING | Build passed; Vite warned that the main bundle is 928.64 kB minified (above 500 kB). |
| `npx vitest run --pool=threads` | PASSED | 39 test files and 216 tests passed. One initial run showed a timeout in `ComplexReservationsPage.test.tsx`; the test passed on re-run and in isolation, indicating parallel-test flakiness rather than a product defect. Threads pool used per Windows project guidance. |
|| `npx vitest run --pool=threads src/pages/UserHistoryPage.test.tsx` | PASSED | 4 tests passed, including cancellation actor/reason and fallback cases. |
| `npm run test:e2e` | FAILED | 1 passed, 4 failed, 2 skipped. Both US-079 E2E tests timed out waiting for the reservation page’s create button; two EPIC-05 tests timed out waiting for complex responses. |
| CI workflow inspection | FAILED QUALITY GATE | `.github/workflows/ci.yml` excludes `Mova.IntegrationTests`. |

## Epic Score

| Dimension | Score | Evidence |
|---|---:|---|
| Requirements completeness | 84 | Major flows exist; idempotency is missing and US-031 presentation is incomplete. |
| Functional correctness | 84 | Core handlers and UI flows are sound; retry semantics and cancellation history are incomplete. |
| Security | 88 | Authenticated and complex-scoped authorization is solid; user search is validated/rate-limited. |
| Architecture | 86 | Clean layering and policy abstraction are strong; cross-cutting idempotency is absent. |
| API consistency | 68 | Routes/contracts are conventional, but documented mutation idempotency is not implemented. |
| Database/data integrity | 84 | Constraints, indexes, and serializable transactions are present; concurrent behavior lacks direct proof. |
| Frontend | 82 | Calendar/search implementations are strong; user-history cancellation details are missing. |
| Testing | 68 | Good unit coverage, but CI excludes integration tests and E2E/concurrent-flow coverage is insufficient. |
| Performance | 82 | Pagination, indexes, debounce, and rate limiting exist; no contention/load testing and bundle warning remain. |
| Observability | 74 | Request/error diagnostics exist; explicit reservation audit events are limited. |
| Documentation | 78 | Core behavior is documented, but idempotency documentation and implementation disagree. |
| Production readiness | 65 | CI integration exclusion, failed E2E validation, and missing idempotency require remediation. |

## Final Verdict

**VERDICT: CHANGES REQUESTED**

**CRITICAL:** 0  
**HIGH:** 1  
**MEDIUM:** 3  
**LOW:** 0  
**INFO:** 2

### Main risks

1. Retried reservation mutations do not satisfy the documented idempotency contract.
2. CI can merge API/database regressions because it excludes the integration suite.
3. The conflict guarantee is not validated under concurrent PostgreSQL requests.

### Main missing requirements

1. `Idempotency-Key` support and replay behavior for reservation POST/PATCH mutations.
2. ~~Cancellation reason and actor display in user reservation history.~~ **Resolved:** `UserHistoryPage` now renders these values with fallbacks.
3. A green, required integration-test CI gate and a working reservation E2E path.

### Main cross-story issue

The user creation, cancellation, administrative mutation, and availability flows all rely on safe repeated operations, but the required shared idempotency mechanism is absent across them.
