# Epic Review

## Epic

EPIC-03 — Sports Complex Administration

**Review date:** 2026-08-20

## Executive Summary

This review covered EPIC-03 and US-013 through US-017 across the API, application/domain layers, EF Core persistence, React SPA, tests, CI, documentation, and relevant git history.

The epic implements core complex creation, first-administrator linkage, validation, active-only public listing and detail queries, complex-admin ownership checks, activation/deactivation, and super-admin listing/moderation APIs. The public read paths correctly filter inactive complexes; database-backed complex-administrator authorization prevents cross-complex updates.

The main functional gap is in the combined identity/onboarding flow: complex-admin onboarding creates a `Pending` complex and immediately routes the admin to the admin area, but the complex profile uses the public active-only API and therefore fails to load the pending complex. This blocks the new administrator from editing the complex profile before super-admin moderation.

**Production readiness:** Not ready until pending-complex administration is made coherent across the onboarding, admin-query, and profile flows. The CI integration-test exclusion and lack of EPIC-03 E2E coverage should also be addressed.

## Overall Verdict

**CHANGES REQUESTED**

## Epic Completeness

**Implementation completeness: approximately 82%.**

The CRUD-adjacent management paths and security boundaries are largely implemented. The intended pending moderation flow is not end-to-end usable for an administrator, and test/CI coverage does not prove the full administration journey.

## Scope Reconstruction

### Intended user journey

1. An authenticated user submits sports-complex details.
2. The system creates the complex, assigns the first `ComplexAdmin`, and grants the creator that role.
3. The complex administrator manages the complex’s public details and active/inactive state.
4. Public visitors can see and query only active complexes.
5. A super administrator can list all complexes, including inactive/pending states, and moderate their state.
6. The identity onboarding path creates a pending complex, assigns the administrator, and routes them to its administration area while awaiting moderation.

### Dependency map

```text
EPIC-03 Sports Complex Administration
├── US-013 Create complex
│   ├── ComplexesController -> CreateComplexHandler -> SportsComplex
│   ├── ComplexAdministrator first-admin link
│   └── Business-hours defaults
├── US-014 Edit complex
│   ├── ComplexesController -> UpdateComplexHandler -> SportsComplex.Update
│   └── ComplexProfilePage -> PUT /api/v1/complexes/{id}
├── US-015 Activate/deactivate
│   ├── PATCH /api/v1/complexes/{id}/status
│   └── active-only public repository queries
├── US-016 Super-admin moderation
│   ├── GET /api/v1/admin/complexes
│   └── SuperAdmin authorization + status endpoint override
└── US-017 First administrator
    ├── ComplexAdministrator record
    ├── ComplexAdmin user role
    └── database-backed complex-scope authorization
```

## Requirements Coverage

| Requirement | Story | Implementation | Tests | Status | Finding |
|---|---|---|---|---|---|
| Logged-in user creates a complex and becomes first administrator | US-013, US-017 | Complete | Unit and integration | PASS | Creation adds the complex, active `ComplexAdministrator` link, and `ComplexAdmin` role. |
| Required complex fields are server-validated and persisted | US-013 | Complete | Unit and integration | PASS | FluentValidation validates text, coordinates, international phone format, and email; domain also validates core invariants. |
| A complex admin can edit their own complex | US-014 | Partial | Unit, integration, frontend | GAP | Direct active-complex path works, but pending complexes created by onboarding cannot load in admin profile. See H-01. |
| Inactive complexes are excluded from public listing/detail/availability | US-015 | Complete | Integration | PASS | Active-only complex and court repository queries prevent public discovery and availability access after deactivation. |
| CreatedAt and UpdatedAt are tracked | US-013, US-014, US-015 | Complete | Unit and integration | PASS | Domain creation/update/status transition methods set the documented timestamps. |
| Super admins list and manage all complexes | US-016 | Complete API / Partial frontend | Integration | PASS | Super-admin API list is protected and includes inactive records; status policy permits super-admin access. No dedicated management UI was required by the story’s affected-project scope. |
| Complex creation and its operational defaults are consistent | US-013, US-017 | Partial | Partial | GAP | Core complex/admin records are committed separately from default business-hours creation. See M-02. |
| Required unit, integration, and E2E tests pass | All stories | Partial | Unit/integration pass; E2E absent for EPIC-03 | GAP | No E2E test covers this epic’s full flow. See M-03. |

## Findings

### [HIGH] CROSS-STORY — Pending complex onboarding routes administrators to a profile that can only load active complexes

**Category:** Functional / Frontend / API

**Affected stories:** US-013, US-014, US-016, US-017; integration with EPIC-02 complex-admin onboarding

**Location:** `src/Mova.Application/Authentication/Handlers/CompleteComplexAdminHandler.cs:45-60`; `src/mova-web/src/features/users/useCompleteComplexAdminProfile.ts:21-24`; `src/mova-web/src/pages/ComplexProfilePage.tsx:68-72`; `src/Mova.Application/Complexes/Handlers/GetActiveComplexByIdHandler.cs:17-26`

**Problem:** The complex-admin onboarding flow creates the complex with `ComplexStatus.Pending`, grants the creator complex-admin access, and routes them to `/admin/complex/{complexId}`. The profile page then calls `useActiveComplex`, which requests the public `GET /api/v1/complexes/{id}` endpoint. That endpoint intentionally returns `404` unless the complex is `Active`; the profile page renders its load-error state for the pending complex.

**Why it matters:** The documented onboarding design requires a pending complex to be administered before a super administrator activates it. A newly onboarded administrator cannot load or edit their own profile through the normal admin route, so the cross-story journey breaks before moderation is complete.

**Scenario:** A visitor completes the complex-admin form. The API persists a pending complex and an active administrator association, returns a JWT, and the SPA navigates to `/admin/complex/{id}`. Selecting the profile page invokes the active-only public API and receives `404`, despite the caller being the authorized administrator of that complex.

**Recommendation:** Add an authenticated complex-admin read endpoint/query that returns the caller’s assigned complex regardless of public state, and use it for admin pages. Keep the current public endpoint active-only. Add integration and frontend tests for a pending complex admin loading/editing their own complex, a different-complex admin receiving `403`, and a public caller receiving `404`.

**Confidence:** HIGH

### [MEDIUM] Complex creation commits core records before default business-hours provisioning

**Category:** Database / Reliability

**Affected stories:** US-013, US-017

**Location:** `src/Mova.Application/Complexes/Handlers/CreateComplexHandler.cs:47-61`

**Problem:** `CreateComplexHandler` saves the sports complex, administrator association, and user role before it creates and saves seven default business-hours records. These operations are not wrapped in the available unit-of-work transaction abstraction.

**Why it matters:** A failure in the second save leaves a successfully created active complex and administrator association without the operational defaults that creation attempts to establish. This produces partial setup that can surface unpredictably in later court/availability functionality and is difficult for callers to distinguish from complete creation.

**Scenario:** The first database save succeeds, but the second save fails due to a transient database error or cancellation. The caller receives an error, retries creation, and may create a duplicate complex while the original complex already has active public visibility but lacks persisted business hours.

**Recommendation:** Perform complex, administrator, user-role, and default business-hours creation in one transaction and save once where EF change tracking permits. Add a test that forces default-hours persistence to fail and proves no complex or administrator association remains committed.

**Confidence:** HIGH

### [MEDIUM] EPIC-03 lacks end-to-end coverage for the administrator lifecycle

**Category:** Testing

**Affected stories:** US-013, US-014, US-015, US-016, US-017

**Location:** `src/mova-web/playwright.config.ts:4-5`; `src/mova-web/e2e/`

**Problem:** The available Playwright coverage is for US-079 and does not cover complex creation/onboarding, administrator profile management, visibility transitions, or super-admin moderation. The current frontend unit tests mock the active-complex query and do not expose the pending-complex integration defect.

**Why it matters:** The most important interactions in this epic cross browser routing, JWT role/complex association, backend authorization, public visibility, and data state. Unit and isolated integration tests do not prove those components work together.

**Scenario:** A regression changes an admin page to call a public endpoint, as in H-01, and all existing unit/integration tests remain green because neither executes the full onboarding-to-profile route.

**Recommendation:** Add deterministic Playwright tests using a test-auth mechanism for: create/onboard pending complex -> admin profile; pending -> active moderation -> public listing; deactivation -> public removal; and cross-complex admin denial.

**Confidence:** HIGH

### [MEDIUM] Backend CI excludes the integration suite that validates complex authorization and public visibility

**Category:** CI/CD / Testing

**Affected stories:** US-013, US-014, US-015, US-016, US-017

**Location:** `.github/workflows/ci.yml:79-82`

**Problem:** Backend CI filters out `Mova.IntegrationTests`. Those tests contain the primary proofs for complex creation, active-only public listing/detail behavior, cross-complex admin denial, and super-admin access.

**Why it matters:** A regression in endpoint policy, EF persistence, or query filtering can merge without CI exercising the API/database boundary.

**Recommendation:** Fix the stated integration-test instability and remove the test filter so the existing PostgreSQL service container is used by the backend test gate.

**Confidence:** HIGH

## Security Assessment

- **Authentication:** Complex creation requires an authenticated user policy. Admin and moderation endpoints require the expected role policies.
- **Authorization:** Complex update and status APIs use `ComplexAdmin` policy. The authorization handler resolves the active `ComplexAdministrator` association from persistence and rejects administrators of another complex; super admins are explicitly permitted.
- **Data exposure:** Public list/detail APIs are active-only. Public availability uses an active-court query that also requires the complex to be active. Admin responses expose documented complex contact data to authorized users.
- **Input security:** Create and update validators enforce field presence, maximum lengths, coordinate ranges, phone format, and email format. EF Core LINQ queries parameterize public search.
- **Security consistency:** Authorization is strong for the reviewed mutation paths. The main inconsistency is not a privilege escalation but an authorized administrator being unable to read a pending resource through the admin UI (H-01).

## Architecture Assessment

The implementation generally follows the documented layering: controllers coordinate HTTP concerns, validators are in application, complex state is encapsulated in the domain entity, and EF queries are isolated in infrastructure repositories. `ComplexAdminAuthorizationHandler` correctly stays in the API layer because it consumes route data and authorization context while using application persistence abstractions.

The principal architectural concern is state-specific read modeling. `useActiveComplex` is correctly built for public reads, but it is reused by the administration page, collapsing public visibility and admin ownership into one query. Separate public and authorized-admin queries are required for coherent boundaries.

## Functional Assessment

Direct complex creation persists and returns a complex, links the creator as first administrator, grants the appropriate role, and seeds default business hours. Edit, active/inactive toggling, public exclusion, and super-admin listing are covered by current integration tests. `CreatedAt` and `UpdatedAt` behavior is also covered.

The complete administrator journey has a broken pending state. The direct API creation path defaults to `Active`, while the intended onboarding path defaults to `Pending`. The latter is legitimate for moderation, but the frontend fails to represent that state for an authorized administrator. The direct and onboarding creation paths also differ in their provisioning of default business hours.

## Testing Assessment

- **Unit:** Creation, update, status transitions, validators, domain state methods, and authorization-handler behavior have focused coverage.
- **Integration:** The full suite includes complex creation, invalid/unauthenticated requests, public active-only list/detail queries, update/status authorization, dashboard access, and super-admin access. The full backend suite passed in this review.
- **Frontend:** Complex profile form loading, validation, submission, error, and loading states are covered. These tests mock `useActiveComplex`, so they do not test the pending state or endpoint selection.
- **E2E:** No EPIC-03 lifecycle test was found. The review did not rerun Playwright because its prior run in this session failed on unrelated US-079 tests; the existing E2E suite does not cover EPIC-03.
- **Missing coverage:** pending-admin profile query/edit; onboarding-created complex visibility; rollback on default business-hours persistence failure; and full moderation/public-visibility lifecycle.

## Data & Database Assessment

`SportsComplexes` fields and precision align with the epic’s core data model. The EF configuration makes the expected creation fields required, stores status as a string, tracks timestamps, and indexes city. `ComplexAdministrators` has a unique `(SportsComplexId, UserId)` constraint and a cascade relationship to the complex, supporting first-admin integrity.

No schema migration concern was identified for the existing complex model. The main data-integrity risk is partial persistence in the two-save creation sequence (M-02). The public active-only query and active-court query correctly avoid exposing inactive complex data.

## Frontend Assessment

The complex profile form uses React Hook Form and Zod, provides loading/error/success states, validates complex contact and coordinate data, and invalidates relevant TanStack Query caches after updates. Complex-admin routes are guarded by role and complex association.

The form uses a public active-only data hook for an administrative screen. It cannot represent the legitimate `Pending` state created by complex-admin onboarding, which causes the onboarding flow to fail at the profile screen (H-01). The super-admin page is a placeholder; this is not classified as a requirement defect because US-016 specifies backend API as the affected project, but it limits operational usability.

## Performance Assessment

Public complex listing paginates and caps page size at 100. Public search is applied in the database using `ILIKE` across name, city, and address; city is indexed, but name/address searches can become expensive at scale. This is acceptable for current MVP evidence but should be monitored as the complex catalog grows.

The frontend production build passed but emitted a general 921 kB minified main-bundle warning. This is broader than EPIC-03 and not classified as an epic defect.

## Observability Assessment

Request logging and centralized exception handling are configured. The reviewed complex lifecycle does not create explicit audit events for creation, update, or public-visibility status changes despite `AuditLogs` existing in the architecture. This is an operational improvement opportunity; no explicit EPIC-03 requirement made it a release finding.

## Regression Risks

1. Correcting the pending-admin query must not expose pending or inactive complex data to public callers.
2. Consolidating creation into one transaction must preserve first-admin role assignment and existing default business-hours behavior.
3. Changing status handling must continue to allow super-admin moderation while denying administrators of other complexes.
4. The pending-profile activation bugfix planned from EPIC-02 may affect direct complex creation, which presently accepts the general authenticated-user policy.

## Documentation Assessment

The architecture documents the complex entity, public active-only behavior, and complex-administrator association. The frontend README lists the active admin-profile route but does not describe that pending complexes require an authorized admin data path. Update documentation and API contracts when H-01 is fixed so public and admin complex reads are explicit.

## Positive Findings

- Server-side validation and frontend form validation align for phone number, email, fields, and coordinate ranges.
- First-administrator linkage is created automatically and persisted with a unique association.
- Complex-admin authorization checks the persisted association for the requested complex instead of trusting only a client-provided ID.
- Public complex list/detail and active-court availability paths exclude inactive complexes.
- Status transitions update `UpdatedAt` and have direct unit/integration coverage.
- Super-admin list and status-management paths are API-protected and integration-tested.

## Validation Results

| Validation | Result | Notes |
|---|---|---|
| `dotnet test Mova.slnx --no-restore` | PASSED | 5 architecture, 341 unit, and 123 integration tests passed (469 total). |
| `npm run lint` | PASSED | Executed in `src/mova-web`. |
| `npm run build` | PASSED WITH WARNING | Production build passed; Vite reported a main bundle larger than 500 kB. |
| `npx vitest run --pool=threads` | PASSED | 37 test files and 202 tests passed. Threads pool used per Windows project guidance. |
| `npm run test:e2e` | NOT EXECUTED FOR THIS REVIEW | The earlier session run failed on two unrelated US-079 tests; no EPIC-03 E2E journey exists. |

## Epic Score

| Dimension | Score | Evidence |
|---|---:|---|
| Requirements completeness | 82 | Core APIs exist; pending onboarding cannot complete its admin-profile flow. |
| Functional correctness | 78 | Direct path works; moderation/onboarding state is broken cross-story. |
| Security | 90 | Strong API policies and database-backed complex scope enforcement. |
| Architecture | 84 | Clean layering; public and admin read models are improperly shared. |
| API consistency | 85 | Conventional routes/validation; pending admin read API is missing. |
| Database/data integrity | 82 | Constraints and timestamps are sound; create provisioning is non-atomic. |
| Frontend | 76 | Form/guards are good; pending state cannot load in admin profile. |
| Testing | 72 | Unit/integration pass; key cross-story path and E2E coverage are absent. |
| Performance | 85 | Pagination/caps present; monitor catalog search and main bundle. |
| Observability | 75 | Request/error logs exist; lifecycle audit events are absent. |
| Documentation | 84 | Core architecture is documented; admin/public pending-state behavior is unclear. |
| Production readiness | 75 | Fix H-01 and strengthen transaction/test gates before release. |

These are approximate risk indicators, not proof of correctness.

## Final Verdict

**VERDICT: CHANGES REQUESTED**

- **CRITICAL:** 0
- **HIGH:** 1
- **MEDIUM:** 3
- **LOW:** 0
- **INFO:** 0

### Main risks

1. Pending complex administrators cannot load/edit their complex profile immediately after onboarding.
2. Partial complex creation can persist public/admin records without default business-hours provisioning.
3. CI and E2E coverage do not protect the end-to-end complex-administration lifecycle.

### Main missing requirements

1. An authorized administration read path for a pending complex, distinct from public active-only reads.
2. Transactional persistence and failure coverage for complete complex setup.

### Main cross-story issue

**CROSS-STORY:** EPIC-02 onboarding creates a pending complex and grants its administrator access, while EPIC-03’s profile page reuses the active-only public read endpoint. The resulting `404` prevents the authorized new administrator from administering the pending complex.
