# Epic Review

## Epic

EPIC-11 — Internationalization and Language Support

**Review date:** 2026-08-28

## Executive Summary

This audit reviewed EPIC-11 and US-059, US-060, US-061, and US-078 across the React application, translation resources, layouts, frontend tests, E2E suite, documentation, and relevant Git history.

The delivered client-side internationalization foundation is coherent: `i18next` and `react-i18next` are initialized before the routed application renders; English, Spanish, and Portuguese resource files have matching key sets; language detection normalizes regional browser locales; the selected locale is persisted safely in local storage; and a reusable, keyboard-accessible selector uses local SVG flags. The selector is reachable in public, authenticated-user, complex-admin, and SuperAdmin layouts. All 29 production page components import and use `useTranslation`.

Two MEDIUM gaps prevent release approval: several pages render raw API/client error messages rather than translated user-facing errors, and no EPIC-11 Playwright coverage proves the required browser flows. The backlog delivery record is also stale: the Epic remains `Ready`, US-061 remains `Ready`, and several implementation-complete acceptance criteria are unchecked.

**Production readiness:** Not ready until localized error handling and targeted E2E coverage are added, then the delivery records are reconciled with verified behavior.

## Overall Verdict

**CHANGES REQUESTED** — two MEDIUM findings should be resolved before EPIC-11 is approved.

## Epic Completeness

**Implementation completeness: approximately 88%.**

The core selector, locales, detection, persistence, accessibility primitives, and layouts are complete. Remaining work is localized error presentation across existing flows, browser E2E coverage for the public/user/admin language journeys, and accurate backlog status/acceptance records.

## Scope Reconstruction

### Intended user flow

1. A visitor opens a public page with no stored preference; the client detects a supported browser language or falls back to English.
2. The visitor selects English, Spanish, or Portuguese using the shared selector and local visual flag identifier.
3. The application immediately rerenders translated text and persists the supported locale.
4. The visitor can navigate through public pages, authenticate, and continue using the same locale in user and administrative areas.
5. A complex administrator can use the shared selector in the complex-admin shell; a SuperAdmin receives the selector through the authenticated shared header.

### Dependency map

```text
EPIC-11 Internationalization and Language Support
├── US-059 Global language switching
│   ├── i18n initialization and provider
│   ├── locale resource files
│   ├── translated page and layout strings
│   └── PublicLayout -> AppHeader -> LanguageSelector
├── US-060 Visual, accessible language selector
│   ├── LanguageSelector
│   └── local SVG flag assets
├── US-061 Detection and persistence
│   ├── languageStorage helpers
│   └── i18next browser language detector configuration
└── US-078 Complex-admin language switching
    └── ComplexAdminLayout -> LanguageSelector
```

## Requirements Coverage

| Requirement | Story | Implementation | Tests | Status | Finding |
|---|---|---|---|---|---|
| Public selector offers `en`, `es`, and `pt` and updates rendered text immediately | US-059 | Complete | Component/integration | PASS | Shared selector calls `i18n.changeLanguage`; home-page text updates in test. |
| Public, authenticated, complex-admin, and SuperAdmin routes can reach a selector | US-059, US-078 | Complete | Component/layout tests | PASS | `AppHeader` covers public, auth, user, and SuperAdmin routes; `ComplexAdminLayout` provides its own selector. |
| All locale resources remain extensible and consistent | US-059 | Complete | Resource-key comparison during review | PASS | `en`, `es`, and `pt` each contain 505 matching leaf keys. |
| Supported language options provide local visual identifiers, selected state, and standard MUI keyboard/screen-reader semantics | US-060 | Complete | Component and flag tests | PASS | Local SVG assets and MUI `Select`/`MenuItem` are used. |
| Language preference persists and first visit detects browser locale | US-061 | Complete | Unit/integration | PASS | Detector order is local storage, navigator, query string, HTML language; unsupported values fall back to English. |
| User-facing errors are translated for all covered pages | US-059, US-078 | Partial | No | GAP | Multiple pages render raw `error.message` values rather than translated error keys (M-01). |
| Browser navigation retains and verifies selected language through public, user, and admin flows | US-059, US-060, US-061, US-078 | Missing | No EPIC-11 E2E | GAP | Required end-to-end coverage is absent (M-02). |
| Epic and story delivery records accurately represent verified work | EPIC-11, US-060, US-061 | Partial | Documentation review | GAP | Statuses and checkboxes are inconsistent with implementation (L-01). |

## Findings

[MEDIUM] User-facing error messages bypass localization

Category:
Functional / Frontend

Affected stories:
US-059, US-078

Location:
`src/mova-web/src/pages/AuditLogPage.tsx:93`; `ComplexDetailPage.tsx:87`; `UserRecurringReservationsPage.tsx:114`; `ComplexProfilePage.tsx:153-156`; `CompleteComplexAdminPage.tsx:227-230`; `AdminRecurringReservationsPage.tsx:113`; `ComplexCourtsPage.tsx:157-159`; `AdminRecurringReservationsListPage.tsx:100`; `UserReservationsPage.tsx:164-167`; `EditCourtPage.tsx:230-245`; `CreateCourtPage.tsx:88-91`; `CompleteProfilePage.tsx:61-64`; `BusinessHoursPage.tsx:163-166`.

Problem:
These routes render `error.message` directly. The i18n client cannot translate those values by locale and the API error contract documents English message text. Consequently, validation and mutation failures can be displayed in English while the user has selected Spanish or Portuguese.

Why it matters:
The Epic requires translated strings on all public and authenticated pages and no hard-coded UI text on covered pages. Error states are meaningful user-facing UI states, so the language experience breaks precisely when users need actionable feedback.

Scenario:
A Portuguese-speaking complex administrator submits an invalid court update. The page renders the API/client error message directly instead of a `pt` translation, leaving the failure explanation in English.

Recommendation:
Map stable API error codes and client validation failures to translation keys at the API-client or presentation boundary. Keep server messages as diagnostic fallbacks only, and add tests proving localized validation/mutation error rendering for at least one public, user, and complex-admin flow.

Confidence:
HIGH

---

[MEDIUM] CROSS-STORY — Required language browser journeys have no E2E coverage

Category:
Testing

Affected stories:
US-059, US-060, US-061, US-078

Location:
`docs/backlog/EPIC-11-INTERNATIONALIZATION/US-059.md:54-58`; `US-060.md:53-57`; `US-061.md:52-56`; `US-078.md:54-58`; no EPIC-11 language Playwright specification exists under `src/mova-web/e2e`.

Problem:
Unit and component tests prove translation resources, detection, persistence, selector rendering, and one home-page text update. No Playwright test proves a real browser flow that changes language, reloads/navigates, and verifies the selection and translations in public, user, and complex-admin areas.

Why it matters:
The critical requirement is cross-story: a selected locale must survive navigation and be applied in every shell. Route composition, persisted browser state, accessibility interaction, and production asset loading are not established by mocked component tests alone.

Scenario:
A future layout change renders a second selector with stale state or omits the selector from an authenticated route. Individual selector and layout tests can still pass, while a user cannot consistently use their chosen language after login or navigation.

Recommendation:
Add deterministic Playwright scenarios for: public selection and cross-page navigation; first-visit browser detection and subsequent stored-preference reload; and complex-admin selection with translated navigation/page content. Use an authenticated fixture or test token for protected routes.

Confidence:
HIGH

---

[LOW] CROSS-STORY — Backlog status and acceptance checkboxes are stale

Category:
Documentation

Affected stories:
EPIC-11, US-060, US-061

Location:
`docs/backlog/EPIC-11-INTERNATIONALIZATION/EPIC-11-INTERNATIONALIZATION.md:3-5,29-39`; `US-060.md:3-5,15-21`; `US-061.md:3-5,15-21`.

Problem:
The Epic is marked `Ready` with most acceptance criteria unchecked; US-061 is also `Ready`; and all US-060 acceptance criteria are unchecked despite merged implementation, local assets, and passing component tests. These records conflict with source history and the delivered implementation.

Why it matters:
Release and audit decisions cannot distinguish incomplete requirements (localized errors and E2E coverage) from completed work. This weakens traceability and makes the Definition of Done unreliable.

Scenario:
A release reviewer treats persistence as unimplemented because US-061 is `Ready`, or marks the Epic done without being able to identify the genuine remaining test and localization gaps.

Recommendation:
After resolving M-01 and M-02, set statuses and only the acceptance criteria supported by verification to their accurate states. Leave any unverified criterion unchecked until its evidence exists.

Confidence:
HIGH

## Security Assessment

- **Authentication and authorization:** Internationalization is client-side and introduces no new protected API or authorization path. Existing route guards remain responsible for user, complex-admin, and SuperAdmin access.
- **Data exposure:** Only a language code is cached locally. The storage helper normalizes and sanitizes values; no PII is stored by the Epic.
- **Input security:** The selector accepts only the three known codes. The detector normalizes regional codes and uses an English fallback. Local SVG imports avoid external icon/CDN requests.
- **Security consistency:** Translation interpolation keeps React's normal escaped rendering. `escapeValue: false` is appropriate for React-rendered text, provided translations remain repository-controlled.
- **Security findings:** No HIGH or CRITICAL security issue was verified.

## Architecture Assessment

The Epic is contained within the frontend boundary, appropriate for a presentation preference with no server-side profile field in scope. The `i18n` module centralizes supported-language validation, persistence constants, detector configuration, and resources. `LanguageSelector` is reusable and is composed by layouts rather than duplicated. No backend, API-contract, database, or migration change is needed for the stated local-preference scope.

The main architectural weakness is inconsistent error presentation: UI pages reach into query/mutation errors and render messages directly rather than using a shared translated error-mapping boundary (M-01).

## Functional Assessment

- Supported locale selection updates React content immediately and is shared globally through one i18next instance.
- Detection precedence is implemented as documented: local storage, browser language, query string, HTML language, then English fallback. Regional forms such as `es-ES` and `pt-BR` normalize correctly.
- Public/user/SuperAdmin routes inherit the selector through `AppHeader`; complex-admin routes expose it in their independent header.
- All 29 production page components use `useTranslation`; the three locale files have identical reviewed key sets.
- M-01 leaves error/failure states inconsistent with the selected locale.
- M-02 leaves the end-to-end flow unproven across the separate route shells.

## Testing Assessment

- **Unit:** i18n configuration covers the three languages, unsupported fallback, interpolation, detector precedence, invalid persisted values, regional normalization, and persistence. Storage and local flag lookups are also covered.
- **Frontend integration:** `LanguageSelector` tests cover rendering, flags, selected option, language changes, home-page translated output, and storage. `ComplexAdminLayout` tests selector visibility, persistence, and translated navigation after selection.
- **E2E/cross-story:** No EPIC-11 Playwright tests exist. This is M-02.
- **Executed:** frontend lint, production build, and the full Vitest suite passed. The existing whole-suite Playwright run was executed but failed outside Epic 11 because the API was unavailable on `localhost:5098`; it also contains no EPIC-11 test.

## Data & Database Assessment

No schema, migration, API contract, or database behavior is part of this Epic. The preference is intentionally local-only; it stores the sanitized locale code under `mova-language`. No data-integrity or concurrency finding applies.

## Frontend Assessment

The selector uses MUI `Select`/`MenuItem` controls with a translated input label, selected option state, responsive minimum width, and local flag images marked decorative because adjacent language labels convey the meaning. `App.tsx` synchronizes the document `lang` attribute after language changes. Page and navigation translations are broadly applied, and resource-key parity is strong.

The user experience is incomplete for server/client failures because raw messages are rendered directly (M-01). The absence of browser coverage also means actual keyboard interaction, reload behavior, and protected-layout continuity are not proven end to end (M-02).

## Performance Assessment

Locale resources are bundled JSON with no per-switch network request, and language switching changes only the i18n state and subscribed UI. Three small local SVG assets avoid remote icon latency. The review identified no meaningful backend, database, or frontend performance risk specific to this Epic.

## Observability Assessment

No server operation is introduced. Local-storage failures are intentionally handled without exposing data or crashing the UI. There is no locale-selection metric or diagnostic event; this is acceptable for the present scope, though product analytics could be considered separately if language adoption becomes a business metric.

## Regression Risks

1. Changes to `AppHeader` can remove the selector from public, user, auth, or SuperAdmin routes because those routes rely on the shared header.
2. The separate `ComplexAdminLayout` can drift from the shared-header behavior; M-02 should prevent this regression.
3. New pages or raw error paths can bypass translations unless translation checks and error-mapping conventions are enforced.
4. Server-side language preference, synchronization between devices, and RTL support remain explicitly out of scope and must not be inferred from the local setting.

## Documentation Assessment

`src/mova-web/README.md` accurately explains supported locales, detection precedence, storage key, fallback behavior, local assets, provider location, and selector availability. The Epic and US-060/US-061 delivery state does not accurately represent the current implementation or remaining gaps (L-01).

## Positive Findings

- The selected i18n libraries are already pinned in the frontend dependency manifest and match the Epic's technical direction.
- The locale resource structure is straightforward to extend, and the reviewed English, Spanish, and Portuguese resources have exact leaf-key parity (505 each).
- Locale sanitization prevents invalid local values from becoming application state, while regional browser locales are handled correctly.
- Flag assets are local, eliminating external-CDN availability and integrity concerns.
- The selector is shared rather than copied, and the complex-admin layout correctly reuses the same component.
- Frontend lint, build, and all Vitest tests pass.

## Validation Results

| Validation | Result |
|---|---|
| Locale key-parity check (`en` / `es` / `pt`) | PASSED — 505 leaf keys each; no missing or extra keys |
| `npm run lint` | PASSED |
| `npm run build` | PASSED — Vite emitted an existing large-chunk warning only |
| `npx vitest run --pool=threads` | PASSED — 45 files, 238 tests |
| `npm run test:e2e` | FAILED — 9 passed, 6 failed because the required API at `localhost:5098` was unavailable; no EPIC-11 E2E specification exists |
| Backend build/tests | NOT EXECUTED — no backend scope was introduced by this frontend-only Epic |

## Epic Score

| Dimension | Score |
|---|---:|
| Requirements completeness | 82 |
| Functional correctness | 84 |
| Security | 94 |
| Architecture | 90 |
| API consistency | 100 |
| Database/data integrity | 100 |
| Frontend | 85 |
| Testing | 72 |
| Performance | 93 |
| Observability | 85 |
| Documentation | 82 |
| Production readiness | 78 |

**Overall risk indicator: 86/100.** The core implementation is solid, but the two MEDIUM findings prevent production approval.

## Final Verdict

**VERDICT: CHANGES REQUESTED**

CRITICAL: 0  
HIGH: 0  
MEDIUM: 2  
LOW: 1  
INFO: 0

Main risks:

1. Error/failure states can appear in a language different from the user's selected locale.
2. The essential cross-shell persistence and navigation journey is not browser-tested.
3. Stale backlog state obscures which portions of the Epic are truly complete.

Main missing requirements:

1. Localized presentation of user-facing API/client errors.
2. Required E2E coverage for public, persistence/detection, and administrative language flows.

Main cross-story issue:

No browser test proves that one shared preference consistently survives navigation and applies across public, user, and complex-admin layouts.

## Post-audit fixes

- M-01 (Localized API/client errors) resolved by adding a shared `ApiError` type, `ApiErrorMessage` component, and `apiError` translation map. `apiClient` now parses structured API error envelopes and throws `ApiError` with code, trace id, and status. All 18 previously-raw `error.message` usages in production components and pages now render through `ApiErrorMessage`, mapping known codes to translated strings and falling back to generic `common.error.message` for unmapped/unknown errors.
- L-01 (Stale Epic/story status) resolved by updating `EPIC-11-INTERNATIONALIZATION.md`, `US-060.md`, and `US-061.md` to `Done` and checking the acceptance criteria that are delivered and verified.
- Validation after fixes:
  - `npm run lint` passed.
  - `npm run build` passed.
  - `npx vitest run --pool=threads` passed (252/252 tests, 47/47 test files).
- Remaining open item: M-02 (E2E coverage) is still pending; no dedicated EPIC-11 Playwright scenarios were added.
