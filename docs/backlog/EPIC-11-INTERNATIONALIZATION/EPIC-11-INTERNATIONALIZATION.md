# EPIC-11 — Internationalization and Language Support

## Status

Done

## Objective

Enable visitors and users to view the application in their preferred language (Spanish, English, or Portuguese) through an accessible language selector, with a foundation that supports adding more languages later.

## Scope

- Language selector on the main/public screen and globally available in the application shell.
- Spanish (`es`), English (`en`), and Portuguese (`pt`) translations for all public and authenticated pages.
- Persist user language preference.
- Browser language detection on first visit.
- Visual language indicator using a flag or language icon.
- Extensible translation structure for future languages.

## User stories

| ID | Story |
|----|-------|
| [US-059](US-059.md) | As a visitor, I want to change the application language from the main screen so all pages display in Spanish, English, or Portuguese. |
| [US-060](US-060.md) | As a visitor, I want the language selector to display a flag or language icon so I can identify languages visually. |
| [US-061](US-061.md) | As a user, I want my language preference to persist and be detected from my browser so I don't have to reselect it. |
| [US-078](US-078.md) | As an admin, I want the language selector to be available in the admin panel so I can use the admin pages in my preferred language. |

## Acceptance criteria

- [x] A language selector is visible on the main/public screen.
- [x] Selecting Spanish, English, or Portuguese updates the language of all pages immediately.
- [x] The selector includes a visual indicator (flag or language icon).
- [x] The selected language persists across browser sessions.
- [x] The application detects the browser language on first visit when no preference is stored.
- [x] Translation files are structured to make adding new languages straightforward.
- [x] All existing public and authenticated pages use translated strings.
- [x] No hard-coded UI text remains on pages covered by this epic.
- [x] The language selector is available in the admin panel and all admin pages use translated strings.

## Dependencies

- EPIC-01 — Technical Foundation.
- EPIC-09 — Landing Page and Public Site.
- EPIC-08 — Users (for authenticated pages if relevant).
- EPIC-12 — Complex Admin Dashboard (for the admin panel layout).

## Technical notes

- Recommended library: `react-i18next` with `i18next`, `i18next-browser-languagedetector`, and `i18next-localstorage-backend` (or localStorage).
- Store translation files under `src/mova-web/src/locales/{lang}.json` or `src/mova-web/public/locales/{lang}/translation.json`.
- Use standard locale codes: `es`, `en`, `pt`.
- Flag icons: emoji flags, local SVG icons, or an approved icon set. Prefer local assets over external CDNs.
- Wrap the app with `I18nextProvider` in `providers.tsx`.
- Add `LanguageSelector` to `PublicLayout` and any future shared layout (`UserLayout`, `AdminLayout`).
- Consider `Accept-Language` header or user profile preference for future server-side support.
- This epic introduces a new frontend dependency; follow the new-dependency approval path defined in `AGENTS.md`.

## Definition of Done

- All acceptance criteria are implemented and verifiable.
- Relevant unit, integration, and E2E tests pass.
- Code review is approved.
- No secrets or sensitive data are committed.
- Documentation is updated if the change affects setup or API contracts.
