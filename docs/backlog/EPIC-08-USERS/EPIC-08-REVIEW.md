# Epic Review

## Epic

EPIC-08 — User Management and Blocking

**Review date:** 2026-08-27

## Executive Summary

This audit reviewed the EPIC-08 user-management capability across US-042 through US-047: administrator user discovery and reservation history, block/unblock operations, optional block expiry, reservation prevention, and user-visible status. The review covered the React SPA, API, application/domain/infrastructure layers, contracts, migrations, CI configuration, existing tests, project architecture, and relevant history.

The principal journeys are implemented: administrators can list and search users who have reservations in their complex, inspect history, block/unblock with a reason and optional expiry, and see the resulting status; users receive a block warning when viewing an affected complex. Both single and recurring reservation creation enforce the block rule. Authorization is consistently complex-scoped for administrative paths and user-scoped for the status endpoint.

However, the data model treats expiry only as a query-time condition while its filtered unique index treats every record whose persisted status is `Active` as active indefinitely. After an expired record, the handler permits a new block but PostgreSQL rejects its insert. Block creation also does not implement the repository-wide required `Idempotency-Key` contract. Finally, the CI workflow explicitly excludes the PostgreSQL integration suite and there is no end-to-end administrator management journey.

**Production readiness:** Not ready without addressing the expired-block re-block failure and enabling the integration test gate. The idempotency contract and missing E2E journey should also be resolved before release.

## Overall Verdict

**CHANGES REQUESTED** — one HIGH cross-story data-integrity/functional finding and two MEDIUM release-quality findings require remediation.

## Epic Completeness

**Implementation completeness: approximately 85%.**

The core vertical slices are present and locally build/test successfully. The expiry requirement works for read-time eligibility, but a subsequent re-block cannot persist because expiry does not change the stored status. The user story source for US-046 is currently deleted in the working tree, so this review used the epic requirement and implementation evidence for expiry; the deletion was not modified.

## Scope Reconstruction

### Intended user journey

1. An active complex administrator opens the complex users page, filters users who have reserved at that complex, and views a selected user's paged reservation history.
2. The administrator blocks that user, recording a reason and optionally a future UTC expiry; the user list reflects status and the user can later be unblocked.
3. A blocked user cannot create either a single or recurring reservation in that complex; a block in another complex is irrelevant.
4. Once an expiry passes, the user is treated as unblocked for status, list, and reservation checks.
5. An authenticated user visiting a complex can retrieve and see only their own current block status without disclosure of `BlockedByUserId`.

### Dependency map

```text
EPIC-08 User Management and Blocking
├── US-042 / US-043: complex-scoped discovery and history
│   ├── ComplexUsersController + UserRepository
│   └── ComplexUsersPage + userAdminApi
├── US-044 / US-045 / US-046: block lifecycle
│   ├── BlockedUsersController -> BlockUserHandler / UnblockUserHandler
│   ├── BlockedUser + BlockedUserRepository + filtered unique index
│   └── block/unblock dialog and query invalidation
├── Reservation integration
│   ├── CreateReservationHandler
│   └── CreateRecurringReservationHandler
└── US-047: self-service status
    ├── UsersController -> GetMyBlockStatusHandler
    └── ComplexDetailPage -> useMyBlockStatus
```

## Requirements Coverage

| Requirement | Story | Implementation | Tests | Status | Finding |
|---|---|---|---|---|---|
| List/search users who reserved in the complex, with paging/filtering | US-042 | Complete through `GetUsersByComplexHandler` and SPA list | Unit, integration, component | PASS | The separate `/search` endpoint intentionally searches all active users for manual reservation selection; the admin page correctly consumes the complex-scoped endpoint. |
| View a selected user's reservation history | US-043 | Complete | Integration, component | PASS | Complex-scoped reservations endpoint and paged dialog are wired. |
| Block/unblock with reason and actor | US-044, US-045 | Complete except idempotent replay | Unit, integration, component | GAP | M-02: the documented POST idempotency requirement is not implemented. |
| Optional future expiration | US-046 | Partial | Validator/unit, component/E2E status mock | FAIL | H-01: a re-block after expiry violates the active-record unique index. |
| Expired blocks treated as lifted | US-044, US-046 | Partial | Unit only | FAIL | Query-time filtering makes the user eligible, but status remains `Active`, causing H-01. |
| Block is complex-scoped | All | Complete | Unit/integration | PASS | All active checks include `SportsComplexId`; the unique index is likewise scoped. |
| Blocked users cannot create new reservations | US-044 | Complete | Unit | PASS | Both single and recurring creation call `IsUserBlockedAsync`. |
| Existing reservations are preserved | US-044 | Complete | Implementation review | PASS | Blocking creates only a `BlockedUser` record. |
| A user sees their own block status without blocker identity | US-047 | Complete | Unit, integration, E2E | PASS | User contract omits `BlockedByUserId`; UI displays reason/expiry. |
| Required integration coverage is part of CI | Epic DoD | Missing | Integration suite exists but is excluded | GAP | M-01. |

## Findings

### [HIGH] CROSS-STORY — An expired block prevents the same user from being blocked again

**Category:** Database / Functional / Reliability  
**Affected stories:** US-044, US-045, US-046, US-047  
**Location:** `src/Mova.Application/Users/Handlers/BlockUserHandler.cs:44-59`; `src/Mova.Infrastructure/Persistence/Repositories/BlockedUserRepository.cs:22-30`; `src/Mova.Infrastructure/Data/Configurations/BlockedUserConfiguration.cs:19-21`; `src/Mova.Infrastructure/Data/Migrations/20260827162151_FixBlockedUsersActiveFilteredIndex.cs:17-22`

**Problem:** `GetActiveByComplexAndUserAsync` excludes an expired `Active` block, so `BlockUserHandler` considers a new block valid. The database's filtered unique index, however, filters only on persisted `Status = 'Active'`; it does not account for `BlockedUntil`. Since expiry does not call `Lift()`, the expired row remains in that index and `SaveChangesAsync` violates the unique constraint.

**Why it matters:** The required lifecycle is inconsistent across stories: expiry lifts a block for reservation/status queries, but the administrator cannot re-block the user. The unhandled persistence conflict can surface as an unexpected server error rather than the documented business result.

**Scenario:** An administrator creates a one-day block. After it expires, the user can reserve. The administrator later needs to block that user again; the handler adds a second `Active` record and PostgreSQL rejects it because the expired first record is still `Active`.

**Recommendation:** Make persisted and effective state consistent before inserting a new block. For example, atomically lift expired records for the complex/user before the active-block check and insert, or revise the model/index so one invariant expresses effective active status. Add PostgreSQL integration coverage for expiry followed by re-block and concurrent block attempts.

**Confidence:** HIGH

### [MEDIUM] CROSS-STORY — Block creation violates the documented idempotency contract

**Category:** API / Reliability  
**Affected stories:** US-044, US-046  
**Location:** `.ai-kit/docs/architecture/API-DESIGN.md:11,304-308`; `src/Mova.Api/Controllers/BlockedUsersController.cs:34-57`; `src/mova-web/src/features/users/userAdminApi.ts:85-117`

**Problem:** API design requires an `Idempotency-Key` for every `POST`, `PUT`, and `PATCH` mutation and durable replay of the original response. The block endpoint neither accepts/enforces the header nor persists a replay record, and the SPA does not send one.

**Why it matters:** A client retry after a lost `201 Created` response returns a conflict instead of the original successful result. This breaks the published API contract and makes reliable mobile/retry behavior needlessly difficult.

**Scenario:** The block is committed but the mobile client loses the response. Retrying the same action produces `409 Conflict` rather than replaying the created block.

**Recommendation:** Use the existing central idempotency pattern for this POST: validate the key, scope it to actor/operation, persist the canonical result, replay it within the documented TTL, have the SPA supply a UUID, and test same-key retry/concurrency behavior.

**Confidence:** HIGH

### [MEDIUM] PostgreSQL integration tests are excluded from the required CI gate

**Category:** CI/CD / Testing  
**Affected stories:** US-042–US-047  
**Location:** `.github/workflows/ci.yml:78-82`; `tests/Mova.IntegrationTests/Users/UsersControllerTests.cs:114-365`

**Problem:** CI provisions PostgreSQL but runs `dotnet test` with `FullyQualifiedName!~Mova.IntegrationTests`. The suite contains the controller, authorization, persistence, and cross-story block/unblock coverage that unit tests cannot establish.

**Why it matters:** Route, authorization, EF mapping, migration, and persistence regressions can pass the mandatory CI gate. It would also fail to catch H-01.

**Scenario:** A change to the filtered index or an endpoint route breaks blocking in PostgreSQL. Unit and architecture tests pass, leaving CI green.

**Recommendation:** Repair the test environment and remove the exclusion; keep focused PostgreSQL integration tests for complex isolation, expiry/re-block, reservation prevention (single and recurring), and concurrent block attempts in the required gate.

**Confidence:** HIGH

### [LOW] No E2E test covers the administrator's full user-management journey

**Category:** Testing  
**Affected stories:** US-042–US-046  
**Location:** `src/mova-web/e2e/us-047.e2e-spec.ts:81-128`; `src/mova-web/src/pages/ComplexUsersPage.tsx:101-343`

**Problem:** US-047 has Playwright coverage for the user-facing warning, but no E2E scenario traverses administrator search/list, reservation-history dialog, block with optional expiry, refreshed state, and unblock.

**Why it matters:** Mocked component/API-hook tests do not prove route protection, browser payloads, contract compatibility, or query invalidation across the complete administration flow.

**Recommendation:** Add a stable admin E2E scenario for search/list → history → block with reason/expiry → visible status → unblock, including the appropriate authorization setup.

**Confidence:** HIGH

## Security Assessment

- **Authentication and authorization:** Administrative endpoints consistently use `ComplexAdmin`; the user status endpoint uses `User`. Complex-scoped authorization is applied at the controller boundary.
- **Object/tenant access:** Block lookups, list annotations, and reservation checks include `SportsComplexId`. No verified cross-tenant bypass was found.
- **Data exposure:** `MyBlockStatusInfo` excludes `BlockedByUserId`; administrators receive the operational block data they require. No secrets were found in reviewed paths.
- **Input security:** FluentValidation rejects missing IDs and past expiries. LINQ-based repository queries avoid raw SQL. Search validation/paging constrains input.
- **Security finding:** None independently identified. The idempotency gap is a reliability/API-contract issue rather than an authorization bypass.

## Architecture Assessment

The implementation respects the documented Clean Architecture boundaries: controllers translate HTTP and claims into commands, handlers coordinate repository abstractions, `BlockedUser` owns creation/lifting state, EF Core remains in Infrastructure, and public DTOs remain in Contracts. The same block repository abstraction is reused by discovery, status, dashboard, and reservation creation paths.

The principal architecture concern is a split definition of "active": repositories incorporate time while the persistence uniqueness constraint incorporates status only. That split produces H-01. No layer-direction or controller-business-logic violation was found.

## Functional Assessment

The principal flows are present: paging/sorting/filtering, history display, blocking/unblocking, optional future dates, user warning, and enforcement for both reservation types. Loading, empty, error, and mutation-pending states exist on the administration page; mutation success invalidates complex-user and dashboard queries.

Expiry behavior is consistent for normal reads because repository methods filter out passed timestamps. It is incomplete as a lifecycle transition because it does not permit the valid subsequent re-block path. Existing reservations are not changed by blocking, as required.

## Testing Assessment

- **Unit and architecture:** Passed locally. The unit suite includes active/expired single-reservation checks, block validators, handlers, search/list, and authorization; architecture tests verify dependency direction.
- **Integration:** Existing controller tests cover list/search/history, block/unblock cycles, status, and some authorization states. They were **not executed** in this review because the factory creates/manages a PostgreSQL test database and no disposable target was confirmed. There is no test for expiry followed by re-block, nor end-to-end reservation prevention through HTTP.
- **Frontend:** Passed locally with 39 test files / 216 tests. Hook and component coverage exists for the management page and block-status behavior.
- **E2E:** US-047 warning behavior has Playwright coverage, including an expiration display. There is no administrator end-to-end flow.
- **Cross-story:** The integration suite links block/unblock to user-list state, but does not cover expiry → re-block or block → attempted single/recurring reservation.

## Data & Database Assessment

`BlockedUsers` persists the required IDs, reason, timestamps, optional expiry, and status. The filtered unique index prevents duplicate persisted active records per complex/user, and the repository efficiently scopes active reads by complex/user/time.

The migration is additive and no destructive migration risk was found. The index and query semantics disagree for expired rows (H-01); correcting that invariant and validating it against PostgreSQL are required. The existing serializable reservation transaction does not make block creation itself atomic with a reservation attempt, so a concurrent block/reservation race remains unproven and should be covered during integration testing.

## Frontend Assessment

The admin route renders `ComplexUsersPage`, which offers search, sorting, pagination, a block dialog with reason/expiry controls, an unblock action, and a paged history dialog. The user-facing `ComplexDetailPage` queries block status only when authenticated and renders a warning with optional expiry. TypeScript property names and nullable state align with the reviewed .NET contracts.

Material UI provides a responsive foundation, but detailed keyboard/screen-reader evaluation was not performed. The production build passed with a pre-existing Vite warning that the main minified bundle is 930.42 kB (266.44 kB gzip), above the 500 kB advisory threshold.

## Performance Assessment

List and history endpoints paginate, and block annotations are batch-loaded for the current result page rather than issuing one query per user. No confirmed N+1 query was found. Search uses case-normalized contains expressions and a regex phone match; these should be profiled with production-scale user data, but no evidence supports reporting a defect now.

## Observability Assessment

The block record itself retains actor, reason, and timestamp, supporting basic investigation. No dedicated structured audit event, metric, or expiry-lifecycle diagnostic was found for block/unblock operations. This is an **INFO** operational improvement, not a release blocker given the persisted audit fields.

## Regression Risks

- Reservation creation now depends on `BlockedUserRepository`; its behavior affects both single and recurring reservation flows.
- The filtered-index change affects all future re-block operations and is the highest regression risk.
- The complex users page shares user-search/list contracts with manual reservation selection; preserve the distinction between all-active-user search and complex-reservation list semantics.
- The monolithic frontend bundle warning is unrelated to EPIC-08 functionality but remains a delivery-performance concern.

## Documentation Assessment

The domain, API, database, authentication, and multi-tenancy documents describe the expected block model and `USER_BLOCKED` response. The epic itself remains marked `Ready` and its acceptance boxes are unchecked despite individual implementation stories being marked done. The US-046 document is also deleted in the current working tree (`git status`); this review did not restore or alter that user-owned deletion. Resolve the intended story location/status before merging the review.

## Positive Findings

1. Complex scoping is applied consistently for storage, block checks, and administrative endpoints.
2. Both single and recurring reservation creation enforce the same block rule.
3. The user-facing status contract deliberately omits the administrator identifier.
4. The UI provides loading, error, empty, block, unblock, expiry-display, and query-invalidation behavior.
5. Block/unblock lifecycle and user-list reflection have integration coverage.
6. Pagination and batch block lookup avoid obvious list-flow scalability mistakes.
7. Build, lint, unit/architecture tests, and frontend tests are currently green.

## Validation Results

| Validation | Result |
|---|---|
| `dotnet build Mova.slnx --no-restore` | **PASSED** — 0 warnings, 0 errors |
| `dotnet test Mova.slnx --no-build --verbosity normal --filter "FullyQualifiedName!~Mova.IntegrationTests"` | **PASSED** — 5 architecture + 386 unit tests |
| `cmd /c "cd /d C:\source\mova\src\mova-web && npm run lint"` | **PASSED** |
| `cmd /c "cd /d C:\source\mova\src\mova-web && npm run build"` | **PASSED** — bundle-size advisory warning |
| `cmd /c "cd /d C:\source\mova\src\mova-web && npx vitest run --pool=threads"` | **PASSED** — 39 files, 216 tests |
| PostgreSQL integration suite | **NOT EXECUTED** — requires a confirmed disposable PostgreSQL test database; CI currently excludes it |
| Playwright E2E suite | **NOT EXECUTED** — no configured/running browser application was established for this review |

## Epic Score

| Dimension | Score |
|---|---:|
| Requirements completeness | 85 |
| Functional correctness | 78 |
| Security | 92 |
| Architecture | 82 |
| API consistency | 80 |
| Database/data integrity | 70 |
| Frontend | 86 |
| Testing | 76 |
| Performance | 82 |
| Observability | 72 |
| Documentation | 76 |
| Production readiness | 72 |

**Overall risk indicator: approximately 79/100.** The score is not proof of correctness; PostgreSQL integration behavior remains unverified.

## Final Verdict

**VERDICT: CHANGES REQUESTED**

**CRITICAL: 0**  
**HIGH: 1**  
**MEDIUM: 2**  
**LOW: 1**  
**INFO: 1**

**Main risks:**
1. An expired but persisted `Active` record prevents a valid re-block and can produce an unexpected database failure.
2. Required API idempotency is absent for block creation.
3. PostgreSQL integration tests are excluded from CI, leaving persistence and HTTP behavior unprotected.

**Main missing requirements:**
1. A consistent effective-active persistence invariant for expired/re-blocked users, backed by PostgreSQL tests.
2. Idempotent replay for the block POST contract.
3. A required integration-test gate and an administrator E2E journey.

**Main cross-story issue:**
Expiry is considered lifted by the repository but remains `Active` for the filtered unique index. This incompatibility affects block creation, user status, user management, and reservation eligibility together.

---

## Remediation Update — 2026-08-27

The HIGH and MEDIUM findings have been remediated in the working tree:

1. **H-01 — expired-block re-block lifecycle:** `BlockUserHandler` now runs the lifecycle inside a transaction. It finds a matching expired active record, calls `Lift()`, saves that state, verifies that no effective active block remains, and then creates the new block. The regression is covered by both a unit test and a PostgreSQL integration test.
2. **M-02 — block idempotency:** `BlockedUsersController.Block` now requires the existing `IdempotencyRequired` filter. The SPA sends a UUID `Idempotency-Key`; the idempotency store now persists successful response records with `IUnitOfWork.SaveChangesAsync`; and the integration suite includes same-key replay coverage.
3. **M-01 — CI integration gate:** CI now runs the complete `dotnet test Mova.slnx --no-build --verbosity normal` command against its provisioned PostgreSQL service rather than filtering out `Mova.IntegrationTests`.

### Remediation Validation

| Validation | Result |
|---|---|
| `dotnet build Mova.slnx --no-restore` | **PASSED** — 0 warnings, 0 errors |
| `dotnet test tests/Mova.UnitTests/Mova.UnitTests.csproj --no-build --verbosity normal` | **PASSED** — 387 tests |
| Focused frontend Vitest (`userAdminApi`, `ComplexUsersPage`) | **PASSED** — 10 tests |
| `npm run lint` | **PASSED** |
| `npm run build` | **PASSED** — existing bundle-size advisory warning |
| PostgreSQL integration suite | **NOT EXECUTED LOCALLY** — PostgreSQL is unavailable on localhost; CI will run it with the configured service |

### Updated Verdict

**APPROVED WITH COMMENTS (remediation pending CI confirmation).** The original HIGH and MEDIUM findings are addressed in code and targeted tests. The remaining LOW E2E coverage recommendation and the local PostgreSQL validation limitation should be tracked separately.
