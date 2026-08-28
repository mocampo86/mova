# Epic Review

## Epic

EPIC-09 — Landing Page and Public Site

**Review date:** 2026-08-28

## Executive Summary

This audit reviewed the public landing and discovery experience described by US-048 through US-053. It covered the React public routes and layouts, landing-page content and calls to action, active-complex discovery API, Google-authentication handoff, SEO metadata, tests, CI, relevant architecture documentation, and Epic 09 history.

The delivered capability is functionally coherent. Visitors can understand Mova's value proposition, follow the three-step player journey, browse active complexes, see recently created active complexes, begin the player or complex-owner sign-in flow, and receive route-specific title, description, and Open Graph metadata. Public complex data is protected from inactive or pending records by the API rather than only by the UI. The frontend handles featured-content loading, empty, and error states and lazy-loads that below-the-fold section.

The main release-quality gaps are in evidence and documentation: three stories explicitly require E2E coverage, but the repository contains no dedicated E2E journey for the player discovery/booking guide, complex-owner registration/completion flow, or featured-complex behavior. The epic also links to story files that were moved to `docs/backlog/US-BACKLOG`, making two links broken; those source stories are still marked `Ready` even though their functionality was delivered through other Epic 09 changes. The epic remains `Ready` with every epic-level acceptance checkbox unchecked.

**Production readiness:** Functionally ready, but the missing critical-journey E2E coverage and inaccurate backlog record should be resolved before declaring the epic complete.

## Overall Verdict

**CHANGES REQUESTED** — two MEDIUM findings affect verification and delivery traceability. No security, data-integrity, or functional blocker was verified.

## Epic Completeness

**Implementation completeness: approximately 92%.**

All six intended public-site capabilities have implementation evidence. The remaining gap is end-to-end verification for US-049 through US-051 and backlog normalization for US-050/US-051 and the epic status/acceptance criteria.

## Scope Reconstruction

### Intended user journey

1. An unauthenticated visitor arrives at `/`, understands the platform and sees separate player and owner propositions.
2. A player reads the search → select → book guide, opens the public complex list, and selects an active complex to inspect availability and book after sign-in.
3. An owner selects the complex-oriented call to action, authenticates with Google, and is routed either to an existing complex dashboard or to the complex-admin completion flow.
4. The landing page loads up to three latest active complexes, with links to their public detail pages; empty, loading, and error states remain understandable.
5. Public routes supply index-document fallback metadata and client-side route-specific title, description, and Open Graph metadata.

### Dependency map

```text
EPIC-09 Landing Page and Public Site
├── US-048: value proposition and public home
│   └── HomePage + PublicLayout + AppHeader
├── US-049: player discovery / booking guide
│   ├── HomePage player steps and CTA
│   └── ComplexesPage -> ComplexDetailPage -> availability/reservation features
├── US-050: owner listing journey
│   └── HomePage owner CTA -> LoginPage -> useGoogleLogin -> CompleteComplexAdminPage
├── US-051: featured/recent complexes
│   └── HomeFeaturedSection -> useActiveComplexes -> GET /api/v1/complexes
├── US-052: visitor login and registration entry points
│   └── AppHeader/HomePage -> LoginPage -> Google auth flow
└── US-053: public SEO
    └── index.html fallback + Seo component on public pages
```

## Requirements Coverage

| Requirement | Story | Implementation | Tests | Status | Finding |
|---|---|---|---|---|---|
| Value proposition, public home, player/owner sections, and responsive CTAs | US-048 | Complete | Component | PASS | `HomePage` and `PublicLayout` provide the intended public shell and sections. |
| Explain finding and booking a court | US-049 | Complete | Component; related complex/availability tests | GAP | M-01: no dedicated browser journey validates the landing guide through public discovery. |
| Explain/list a complex for owners | US-050 | Complete | Component and completion-page tests | GAP | M-01: no browser journey validates owner CTA → authentication completion path. |
| Featured or recently added active complexes | US-051 | Complete | Component, unit, integration | GAP | M-01: no browser test validates active-data display, empty, or API-error behavior. |
| Landing-page login and registration entry points | US-052 | Complete | Component, targeted E2E | PASS | Targeted Playwright coverage verifies the login and owner links. |
| Static fallback and client-side SEO title, description, Open Graph metadata | US-053 | Complete | Unit, component, targeted E2E | PASS | SEO is applied to the reviewed public routes. |
| Active/public complex records only | US-049, US-051 | Complete | Unit, integration, component | PASS | The anonymous API filters by active status and maps the public contract. |
| Mobile responsiveness and below-fold lazy loading | Epic | Complete by implementation review | Build; component | PASS | MUI breakpoints are used and featured content is lazy-loaded. |
| Epic/story status and acceptance documentation reflects delivery | Epic, US-050, US-051 | Missing | N/A | FAIL | M-02: broken links and stale Ready/unchecked status obscure the delivered scope. |

## Findings

### [MEDIUM] CROSS-STORY — Critical landing journeys lack dedicated E2E coverage

**Category:** Testing / CI/CD  
**Affected stories:** US-049, US-050, US-051  
**Location:** `src/mova-web/e2e/`; `src/mova-web/playwright.config.ts:3-25`; `.github/workflows/ci.yml:83-112`

**Problem:** US-049, US-050, and US-051 each declare E2E testing as required, but the Epic 09 E2E files cover only landing login links (US-052) and SEO tags (US-053). No test follows the player guide into public complex discovery, asserts the featured section's active-complex/empty/error states in a browser, or validates the owner CTA through complex-admin completion. The CI frontend job runs lint, build, and Vitest, not Playwright.

**Why it matters:** Component tests with mocked hooks cannot establish browser routing, request interception, public API compatibility, query timing, or interactions across the landing, discovery, authentication, and owner-completion boundaries.

**Scenario:** A route, CTA query parameter, query key, public API response shape, or lazy-loaded featured section regresses while the mocked component tests remain green. Visitors can no longer complete a primary landing journey, but the required CI checks still pass.

**Recommendation:** Add stable Playwright coverage for (1) player CTA → complexes list → active complex detail, (2) owner CTA → login with `intent=complex` → complex-admin completion route using an authenticated/mocked identity, and (3) featured active data plus empty/error states. Add the relevant E2E invocation to the required release/CI workflow when its backend and identity dependencies can be made deterministic.

**Confidence:** HIGH

### [MEDIUM] CROSS-STORY — Epic source links and completion status do not match the delivered scope

**Category:** Documentation / Regression  
**Affected stories:** US-048–US-053  
**Location:** `docs/backlog/EPIC-09-LANDING/EPIC-09-LANDING.md:3-5,24-39`; `docs/backlog/US-BACKLOG/US-050.md:1-25`; `docs/backlog/US-BACKLOG/US-051.md:1-21`

**Problem:** The epic links US-050 and US-051 as sibling files, but they no longer exist in `docs/backlog/EPIC-09-LANDING`; their documents were moved to `docs/backlog/US-BACKLOG`. Both moved stories remain `Ready` with unchecked criteria, while their matching functionality exists in `HomePage`, `HomeFeaturedSection`, and the complex-admin completion flow. The epic itself remains `Ready` with all its acceptance criteria unchecked.

**Why it matters:** Reviewers and release owners cannot reliably reconstruct the scope from the canonical epic. Broken links and stale completion states can cause work to be duplicated, deferred work to be mistaken for completed work, or this review to be applied to the wrong requirements.

**Scenario:** A future engineer opens the epic to verify the owner and featured-complex requirements, receives broken links, and treats the still-Ready stories as unimplemented even though code has already been merged.

**Recommendation:** Restore US-050 and US-051 to the epic folder or update the epic links to their canonical location; reconcile their status and acceptance criteria with the verified delivered scope; then update the Epic 09 status and epic-level acceptance checkboxes after the E2E gap is closed.

**Confidence:** HIGH

## Security Assessment

- **Authentication and authorization:** Public complex reads and availability are explicitly anonymous. Mutating complex creation remains user-authorized; existing complex management remains complex-admin authorized. The landing's player and owner actions only establish login intent and do not grant roles.
- **Data exposure:** Public listing uses `SportsComplexInfoMapper.ToPublicInfo`; integration tests assert public records omit `Status`. The query filters active records, and the public detail handler returns `404` for inaccessible non-active records.
- **Input security:** The public list constrains `page`, `pageSize`, and search length. Repository search is LINQ/parameterized `ILike`, with no raw SQL or client-supplied sort expression in the reviewed flow.
- **Security consistency:** No verified cross-user, cross-tenant, authentication, or data-exposure finding was identified for Epic 09.

## Architecture Assessment

The epic keeps the public UI in the React presentation layer, obtains server data through the existing TanStack Query API hook, and retains active-status enforcement in the application/infrastructure backend path. `ComplexesController` remains a thin HTTP boundary and delegates retrieval to a handler and repository. `Seo` centralizes browser metadata behavior, and `PublicLayout` provides a separate public shell as required.

No layer-boundary violation or duplicate implementation of active-public filtering was found. The shared `useActiveComplexes` query intentionally serves both full discovery and the three-item landing subset; changing its contract or cache key is the principal shared-component regression risk.

## Functional Assessment

The home page provides the value proposition, player steps, audience-specific CTAs, and a final call to action. The owner link selects `intent=complex`; the auth mutation then routes existing complex administrators to their dashboard and other authenticated owners to `/complete-complex-admin`. The featured section uses the active-list endpoint with page size three, which orders results by most-recent creation and only returns active complexes. It provides skeletons, an error alert, and an empty alert.

Public discovery includes search, pagination, loading, error, empty, and detail-link behavior. The public complex endpoint validates list bounds and only serves active data. No inconsistent state transition or broken primary flow was confirmed, but M-01 leaves the primary multi-page workflows unproven in a browser.

## Testing Assessment

- **Unit/component:** `HomePage.test.tsx` covers proposition content, calls to action, metadata, and rendered featured links. `Seo.test.tsx` verifies creation and updates of SEO tags. Existing complex handler and API tests verify active filtering, search, paging, and public status omission.
- **Integration:** `ComplexesControllerTests` covers complex creation and anonymous active-list filtering, including an attempt to bypass it with query parameters. The full integration suite was run successfully during this review.
- **E2E:** Targeted Playwright tests for US-052 and US-053 passed. The missing US-049–US-051 flows are M-01.
- **Cross-story:** The landing components connect to the existing public discovery and complex-admin completion capabilities, but that connection has no end-to-end automated proof beyond the entry-link test.

## Data & Database Assessment

Epic 09 adds no schema or migration. The featured query uses the existing `SportsComplexes` model and orders active records by `CreatedAt`, then applies bounded pagination. Public-list filtering occurs before count and page selection, avoiding exposure of inactive records and avoiding an obvious N+1 pattern. No database-integrity or migration risk was identified.

## Frontend Assessment

`PublicLayout` provides the shared header and one primary-content landmark. The landing's MUI grids, stacks, spacing, and breakpoint-specific action layout are responsive by implementation review. The features have semantic section headings, and recent SEO/layout work addresses public-page landmarks. The featured section is code-split with `lazy` and has loading, error, and empty representations.

Keyboard and screen-reader behavior was not manually tested. The production build reports the main JavaScript bundle at 949.37 kB minified / 270.67 kB gzip, exceeding Vite's advisory 500 kB chunk limit. Featured content is separately code-split; no Epic 09-specific performance defect is confirmed, but full-bundle splitting should remain a delivery-performance consideration.

## Performance Assessment

- **Backend/database:** The active list is bounded to 100 items per request and uses server-side filtering, count, ordering, and pagination. The landing requests only three records.
- **Frontend:** Below-the-fold featured content is lazy-loaded and uses one TanStack Query request. No repeated-request or rendering defect was found.
- **End-to-end:** The build passes, but Vite emits the noted main-chunk advisory. No measured mobile performance budget or production telemetry was available.

## Observability Assessment

The reviewed public API path participates in request logging and error responses include a trace identifier for list validation failures. The landing itself has no product metrics or dedicated diagnostics for CTA conversion, featured-query failure, or client navigation. This is an **INFO** operational improvement; no required observability contract was identified for the MVP scope.

## Regression Risks

- `useActiveComplexes` is shared by landing discovery, public complex browsing, and other booking flows; preserve its active-only contract, cache key, and paged response shape.
- The login `intent=complex` parameter controls the owner journey; changes to auth redirect logic can break landing CTAs without changing the landing itself.
- `Seo` mutates document head state shared by all public routes; new pages must use it consistently and must not introduce duplicate meta tags.
- The large main bundle can affect the landing's mobile performance objective as frontend features accumulate.

## Documentation Assessment

`src/mova-web/README.md` accurately describes the public landing route, complex discovery, login route, and complex-admin completion capability. The architecture and API design documents are consistent with the reviewed public list and frontend architecture.

M-02 remains: Epic 09's broken US-050/US-051 links and stale status/acceptance records need correction. No new configuration, migration, environment variable, or deployment documentation is required by this epic.

## Positive Findings

1. Active-public complex filtering is enforced by the server and validated through unit and PostgreSQL integration coverage.
2. The landing cleanly separates player and owner actions while reusing the existing authenticated onboarding flows.
3. Featured content has intentional loading, empty, error, and link states and is code-split below the fold.
4. SEO support is reusable, has static fallback tags, uses route-specific metadata, and is tested in both unit/component and browser tests.
5. The public shell separates public, auth, user, and complex-admin layouts, preserving the documented frontend structure.
6. All local build, test, lint, and targeted Epic 09 E2E checks executed in this review passed.

## Validation Results

| Validation | Result |
|---|---|
| `dotnet build Mova.slnx --no-restore` | **PASSED** — 0 warnings, 0 errors |
| `dotnet test Mova.slnx --no-build --verbosity normal` | **PASSED** — 146 tests, including PostgreSQL integration suite |
| `cmd /c "cd /d C:\source\mova\src\mova-web && npm run lint"` | **PASSED** |
| `cmd /c "cd /d C:\source\mova\src\mova-web && npm run build"` | **PASSED** — Vite main-bundle advisory (949.37 kB minified) |
| `cmd /c "cd /d C:\source\mova\src\mova-web && npx vitest run --pool=threads"` | **PASSED** — 43 files, 233 tests |
| `cmd /c "cd /d C:\source\mova\src\mova-web && npx playwright test e2e/us-052.e2e-spec.ts e2e/us-053.e2e-spec.ts"` | **PASSED** — 4 tests |
| Full Playwright suite | **NOT EXECUTED** — targeted Epic 09 E2E specs were run; no full-suite result is claimed. |

## Epic Score

| Dimension | Score |
|---|---:|
| Requirements completeness | 92 |
| Functional correctness | 91 |
| Security | 93 |
| Architecture | 91 |
| API consistency | 92 |
| Database/data integrity | 94 |
| Frontend | 90 |
| Testing | 78 |
| Performance | 82 |
| Observability | 76 |
| Documentation | 74 |
| Production readiness | 84 |

**Overall risk indicator: approximately 87/100.** This is a risk indicator, not proof of correctness; complete browser coverage for the central landing flows is still absent.

## Final Verdict

**VERDICT: CHANGES REQUESTED**

**CRITICAL: 0**  
**HIGH: 0**  
**MEDIUM: 2**  
**LOW: 0**  
**INFO: 1**

**Main risks:**
1. Regression in player discovery, owner onboarding, or featured-complex behavior is not caught end-to-end.
2. Broken story links and stale Ready/unchecked states make the epic scope and completion status unreliable.
3. The shared frontend bundle exceeds Vite's advisory chunk-size threshold.

**Main missing requirements:**
1. Required E2E coverage for US-049, US-050, and US-051.
2. Accurate canonical story links and completion status for Epic 09.

**Main cross-story issue:**
The landing is the handoff point between public discovery, featured active data, and authentication/onboarding, yet only the entry links—not the three resulting user journeys—are covered in browser automation.
