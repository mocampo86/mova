# Epic Review

## Epic

EPIC-02 — Identity and Access

**Review date:** 2026-08-20

## Executive Summary

This review covered EPIC-02 and US-008 through US-012 across the ASP.NET Core API, React SPA, persistence configuration, tests, CI, architecture documentation, and relevant git history.

The epic implements the intended Google ID-token exchange, locally-issued JWTs, profile completion endpoint and form, role-based route guards, resource-scoped complex-admin authorization, and supporting tests. JWTs carry identity, role, and complex-association information; backend authorization rechecks complex associations from persistence rather than trusting the token alone.

The main release-blocking gap is that the mandatory-profile business rule is only enforced by the user-facing layout. An authenticated user with no phone number can bypass the UI and submit direct reservation or recurring-reservation API requests. This violates the stated rule that incomplete profiles must complete their profile before making reservations.

**Production readiness:** Not ready until the profile-completion rule is enforced server-side for user reservation operations. CI and E2E coverage also need improvement before release.

## Overall Verdict

**CHANGES REQUESTED**

## Epic Completeness

**Implementation completeness: approximately 85%.**

The core identity capability is implemented, but the mandatory-phone prerequisite is not enforced on the API paths that create reservations. The epic also lacks an end-to-end test of its principal flow and CI deliberately excludes integration tests.

## Scope Reconstruction

### Intended user journey

1. A visitor authenticates with Google in the React SPA.
2. The SPA sends the Google ID token to `POST /api/v1/auth/google`.
3. The API validates the Google token, creates or updates the local user, and returns a platform JWT with user identity, roles, and complex associations.
4. A user with no phone number is directed to `/complete-profile`, submits a valid international phone number to `PATCH /api/v1/users/me`, and can then use reservation functionality.
5. Complex admins and super admins are limited in both the SPA and API according to their roles; complex admins are also constrained to their assigned complex.

### Dependency map

```text
EPIC-02 Identity and Access
├── US-008 Google login
│   ├── AuthController -> GoogleLoginHandler -> GoogleTokenValidator
│   ├── JwtTokenService
│   └── GoogleLoginButton / useGoogleLogin
├── US-009 Profile completion
│   ├── UsersController -> CompleteProfileHandler -> User.PhoneNumber
│   └── CompleteProfilePage / UserLayout
├── US-010 Roles and complex scope
│   ├── JWT role and complex claims
│   └── ComplexAdminAuthorizationHandler -> ComplexAdministrator repository
├── US-011 React protected routes
│   ├── RequireRole
│   └── RequireComplexAdmin
└── US-012 Authorized API endpoints
    ├── Authorization policies
    └── Controller policy attributes and integration tests
```

## Requirements Coverage

| Requirement | Story | Implementation | Tests | Status | Finding |
|---|---|---|---|---|---|
| Google sign-in is available and completes an ID-token exchange | US-008 | Complete | Unit and integration | PASS | Google button posts the credential to the API; API validates configured Google audience. |
| Successful Google sign-in returns a valid platform JWT | US-008 | Complete | Integration | PASS | API returns an access token; JWT validation enforces issuer, audience, lifetime, and signing key. |
| JWT contains user ID, email, name, and roles | US-008, US-010 | Complete | Partial | PASS | Token service emits `sub`, `email`, `name`, and `roles`; authorization integration tests demonstrate role consumption. |
| Incomplete profiles are redirected to `/complete-profile` | US-009 | Partial | Unit/frontend | GAP | Login redirects correctly and `UserLayout` redirects after dashboard data loads, but direct reservation APIs do not enforce the prerequisite. See H-01. |
| Phone number has valid international format and is stored | US-009 | Complete | Unit and integration | PASS | Matching frontend and FluentValidation rules enforce `+` plus 7–15 digits (spaces permitted); persistence permits up to 50 characters. |
| User, ComplexAdmin, and SuperAdmin roles are enforced in UI and API | US-010, US-011, US-012 | Complete | Unit, frontend, integration | PASS | SPA guards protect user, super-admin, and complex-admin routes; API policies protect relevant controllers. |
| Complex-scoped endpoints reject administrators of other complexes | US-010, US-012 | Complete | Unit and integration | PASS | API authorization handler looks up an active complex-administrator association and tests cover assigned, other-complex, inactive, and super-admin cases. |
| Relevant unit, integration, and E2E tests pass | All stories | Partial | Unit/integration pass; E2E fails | GAP | No EPIC-02 E2E journey exists; current Playwright suite fails. See M-02. |

## Findings

### [HIGH] CROSS-STORY — Mandatory profile completion can be bypassed through reservation APIs

**Category:** Functional / Security

**Affected stories:** US-009, US-012; affects downstream reservation flows

**Location:** `src/Mova.Application/Reservations/Handlers/CreateReservationHandler.cs:62-72`; `src/Mova.Application/Reservations/Handlers/CreateRecurringReservationHandler.cs:95-104`; `src/mova-web/src/layouts/UserLayout.tsx:36-38`

**Problem:** The SPA redirects an incomplete user only after loading the user layout, but the backend reservation handlers accept any active user and never require a non-empty `PhoneNumber`. An authenticated newly registered user can call `POST /api/v1/complexes/{complexId}/reservations/me` or the corresponding recurring-reservation endpoint directly before profile completion.

**Why it matters:** The explicit EPIC-02 requirement is that phone completion is mandatory before a user can make reservations. UI-only enforcement is bypassable and results in reservations with identity data the business flow declares mandatory.

**Scenario:** A new user logs in, receives a valid JWT with `RequiresProfileCompletion=true`, skips `/complete-profile`, and posts a valid booking request with that token. The reservation handler validates account status, court availability, and blocks, then creates and confirms the reservation without checking the phone number.

**Recommendation:** Add a shared server-side profile-completion check for all user reservation creation paths (including recurring reservations) and return a consistent problem response until `PhoneNumber` is present. Retain the UI redirect as a usability guard. Add integration tests proving incomplete users receive the expected rejection and completed users can book.

**Confidence:** HIGH

### [MEDIUM] Integration tests are not enforced in CI

**Category:** CI/CD / Testing

**Affected stories:** US-008, US-009, US-010, US-012

**Location:** `.github/workflows/ci.yml:79-82`

**Problem:** The backend CI job explicitly filters out `Mova.IntegrationTests`, despite those tests covering Google login behavior, profile completion, role policies, and cross-complex authorization.

**Why it matters:** Regressions in authentication, authorization, persistence, or API behavior can merge without the integration suite running. The repository has a PostgreSQL service configured specifically for this suite, but the test command does not use it.

**Scenario:** A change breaks JWT claim mapping or complex-scoped authorization. Unit tests may remain green while the regression is only detected after merge or deployment.

**Recommendation:** Resolve the stated integration-test instability, remove the exclusion, and require integration tests in the backend CI gate. Preserve an explicit documented exception only if an unavoidable environment limitation remains.

**Confidence:** HIGH

### [MEDIUM] EPIC-02 has no passing end-to-end coverage for its required user journey

**Category:** Testing

**Affected stories:** US-008, US-009, US-011

**Location:** `src/mova-web/playwright.config.ts:4-5`; `src/mova-web/e2e/`

**Problem:** The Playwright configuration exists, but the available E2E tests cover US-079 rather than EPIC-02. Running `npm run test:e2e` failed: both existing tests timed out waiting for an authenticated-admin control. There is no automated browser test of Google-login exchange handling, incomplete-profile routing, completion, or post-completion protected navigation.

**Why it matters:** The epic definition of done calls for E2E coverage. Unit and integration tests do not prove the browser route guards, auth state, API client, and profile-completion handoff work together.

**Scenario:** A routing or auth-state regression permits navigation to `/user` without a completed profile, or traps a user at `/complete-profile`; the current automated suite would not detect it.

**Recommendation:** Stabilize the existing E2E setup and add deterministic EPIC-02 browser tests using a test authentication seam. Cover: incomplete login -> `/complete-profile`; valid phone completion -> `/user`; unauthorized admin route -> `/unauthorized`; and complex-admin access to own versus another complex.

**Confidence:** HIGH

## Security Assessment

- **Authentication:** Google ID tokens are validated with the configured audience before a local JWT is issued. JWT bearer validation checks issuer, audience, lifetime, and signing key.
- **Authorization:** API controller policies protect user, complex-admin, and super-admin operations. The complex-admin handler uses an active persisted association for the requested route complex and allows super-admin override.
- **Data exposure:** Auth responses expose only the access token and documented user-profile fields. No refresh token is implemented; no token or phone logging was identified in the reviewed identity flow.
- **Input security:** Login and profile inputs use FluentValidation; profile phone validation agrees with the frontend schema. EF Core repositories and typed contracts reduce injection risk in reviewed paths.
- **Security consistency:** Complex ownership checks are strong in the reviewed API paths. The profile prerequisite is inconsistent because it is enforced in UI but absent from user booking APIs (H-01).

## Architecture Assessment

The epic follows the documented layered architecture well. Controllers stay thin; authentication orchestration resides in application handlers; external Google and JWT concerns stay in infrastructure; and contracts are kept in `Mova.Contracts`. The authorization handler is correctly placed in the API layer because it operates on `HttpContext` route data and an application persistence abstraction.

The strongest architectural choice is resolving complex-admin authorization from the repository on each authorization decision rather than relying only on JWT complex claims. This prevents stale association claims from granting cross-complex access. No dependency-direction violation was identified; architecture tests pass.

The primary architectural improvement is to make profile-completion eligibility a shared domain/application concern, not a layout-only check, so all booking entry points uphold the same rule.

## Functional Assessment

Google login creates or updates the local user and reports whether phone completion is required. The profile form and API validate and persist a phone number. The login hook redirects an incomplete standard user to `/complete-profile`; `UserLayout` also redirects if its dashboard shows no phone number. Role and complex route guards provide expected UI behavior.

The end-to-end business flow is incomplete because the backend accepts direct booking requests before profile completion. The complete-complex-admin onboarding flow supplies a phone number and issues a replacement token, so it does not share this particular gap.

## Testing Assessment

- **Unit tests:** Present for Google-login handling, JWT role handoff, profile validation and profile persistence, and complex-admin authorization.
- **Integration tests:** Present for Google login, invalid token handling, profile completion, role protection, super-admin access, assigned/different/inactive complex-admin access, and more. The full current backend test run passed.
- **Frontend tests:** Route guards and the Google-login redirect behavior are tested; the current frontend suite passed.
- **E2E tests:** No EPIC-02 E2E journey was found. The existing US-079 Playwright tests failed during this review; see M-02.
- **Missing coverage:** Direct API attempts to book before profile completion; profile-completion route guard behavior; and the full login -> complete profile -> book/portal transition.

## Data & Database Assessment

`Users` has unique indexes for both `GoogleSubjectId` and `Email`, preventing duplicate identity records. The phone number is nullable to represent incomplete profiles and has a 50-character limit. Role collections are persisted as a required PostgreSQL text array.

No EPIC-02 migration issue, orphaning risk, or unsafe schema change was identified. Profile completion is a simple update. Concurrent first-time login for the same Google account could still race at the application level and rely on the unique constraint to reject the duplicate; the review found no user-friendly conflict recovery for that rare case, but insufficient evidence to classify it as a release finding.

## Frontend Assessment

The SPA uses typed auth state, parses JWT identity/role/complex data, and protects public, user, super-admin, and complex-admin routes. `RequireComplexAdmin` verifies the requested complex association in the JWT, while the API independently enforces the authoritative association.

Phone completion has client-side Zod validation, error rendering, pending state, and a post-success redirect. The layout-level phone check improves direct-route behavior. It must not be the sole enforcement point for booking eligibility; see H-01.

## Performance Assessment

No significant performance concern was identified in the identity flow. JWT construction and Google validation occur once per login, and profile completion is a single user update. Complex-admin authorization performs a targeted repository lookup per scoped request, an appropriate security trade-off. The frontend build emitted a 921 kB minified main bundle warning, but it is a general application concern and not attributable specifically to this epic.

## Observability Assessment

Request logging and global exception handling are configured. Identity handlers do not log raw Google tokens or phone numbers, which is positive. The reviewed epic does not add dedicated audit events or metrics for login, profile completion, or authorization denials; this is a useful operational enhancement but is not classified as a defect based on the available requirements.

## Regression Risks

1. Changes to JWT claim names or claim mapping can affect every role-protected API and SPA route.
2. Changes to `ComplexAdministrator` persistence or handler lookup can impact all complex-scoped admin functions in later epics.
3. The profile-completion fix must consistently cover single and recurring reservation creation without blocking complex-admin onboarding.
4. Adding server-side completion enforcement can affect existing users with legacy null phone data; release notes or a remediation approach may be needed if such data exists.

## Documentation Assessment

The authentication architecture documentation accurately describes Google ID-token exchange, platform JWT contents, roles, complex authorization, phone-completion rules, secret handling, and token lifetime. The frontend README documents environment variables and routes. Documentation should be updated alongside the H-01 remediation to identify the authoritative server-side profile-completion enforcement and resulting response semantics.

## Positive Findings

- Google ID tokens are validated against the configured client ID before platform JWT issuance.
- JWT validation is configured to validate issuer, audience, lifetime, and signing key.
- Identity, authorization, and persistence responsibilities respect the intended clean architecture layers.
- Complex-admin authorization checks an active database association for the requested complex, rather than relying solely on client-controlled data or stale token claims.
- The SPA has both role-based and complex-association route guards, with API authorization as the security boundary.
- Phone validation is consistent between the React form and FluentValidation.
- The current full backend, frontend lint, frontend build, and frontend unit suites pass.

## Validation Results

| Validation | Result | Notes |
|---|---|---|
| `dotnet test Mova.sln --no-restore` | FAILED | Not a repository solution file; `Mova.slnx` is the actual solution referenced by CI. |
| `dotnet test Mova.slnx --no-restore` | PASSED | 5 architecture, 341 unit, and 123 integration tests passed (469 total). |
| `npm run lint` | PASSED | Executed in `src/mova-web`. |
| `npm run build` | PASSED WITH WARNING | Production build passed; Vite reported a main bundle larger than 500 kB. |
| `npx vitest run --pool=threads` | PASSED | 37 test files and 202 tests passed. Threads pool used per Windows project guidance. |
| `npm run test:e2e` | FAILED | 2 US-079 tests timed out waiting for the authenticated admin reservation UI; no EPIC-02 E2E test ran. |

## Epic Score

| Dimension | Score | Evidence |
|---|---:|---|
| Requirements completeness | 85 | Core features exist; mandatory profile prerequisite is bypassable. |
| Functional correctness | 80 | Happy paths are covered; booking eligibility is incomplete. |
| Security | 82 | Strong token and complex-scope controls; UI-only completion rule is bypassable. |
| Architecture | 90 | Clear layering and authoritative complex ownership checks. |
| API consistency | 88 | Typed contracts and consistent policies; eligibility rule needs centralization. |
| Database/data integrity | 90 | Unique identity indexes and suitable nullable profile state. |
| Frontend | 86 | Strong guards and form state; browser flow lacks E2E proof. |
| Testing | 72 | Unit/integration/frontend suites pass, but no EPIC-02 E2E and CI skips integration tests. |
| Performance | 85 | No material identity-flow concern; general bundle-size warning remains. |
| Observability | 75 | Request/error logging exists; identity audit/metrics are absent. |
| Documentation | 88 | Architecture and frontend route documentation are current. |
| Production readiness | 75 | Server-side prerequisite enforcement and test gates are needed. |

These are approximate risk indicators, not proof of correctness.

## Final Verdict

**VERDICT: CHANGES REQUESTED**

- **CRITICAL:** 0
- **HIGH:** 1
- **MEDIUM:** 2
- **LOW:** 0
- **INFO:** 0

### Main risks

1. Incomplete users can bypass the browser redirect and create reservations directly through authenticated APIs.
2. CI does not execute the integration tests that validate the identity and authorization contracts.
3. The required end-to-end identity journey is not automated, and the current Playwright suite fails.

### Main missing requirements

1. Authoritative server-side enforcement that phone/profile completion precedes reservation creation.
2. Passing EPIC-02 E2E coverage for login, completion, and protected-navigation behavior.

### Main cross-story issue

**CROSS-STORY:** US-009 supplies only UI-level profile-completion enforcement, while US-012-protected reservation APIs accept active users without a completed profile. The combined flow permits the business rule to be bypassed.
