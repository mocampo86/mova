# BUGFIX-008 — Restore backend integration tests in CI

## Status

Ready

## Priority

Medium

## Source

EPIC-06 review finding: **MEDIUM — Backend integration tests are excluded from the CI quality gate.**

## Problem

`.github/workflows/ci.yml` explicitly excludes `Mova.IntegrationTests` from the backend `Test` step, despite provisioning a PostgreSQL service and setting the test connection string. The excluded suite includes reservation-controller coverage for creation, authentication, overlap rejection, user lists/history, cancellations, administrative creation, and status updates.

Unit tests and static build checks cannot prove controller authorization, EF Core query behavior, transaction behavior, persistence mappings, or error-to-HTTP mappings. Reservation behavior can therefore regress while the required backend CI job remains green.

## Objective

Re-enable the backend integration suite as a required CI gate so that API, database, authorization, and concurrency behavior is validated on every build.

## Scope

- `.github/workflows/ci.yml` backend test step.
- `tests/Mova.IntegrationTests` determinism and reliability.
- Any infrastructure or test setup required to keep the suite green in GitHub Actions.

## Out of scope

- Fixing application defects discovered by the integration tests unless they are small, deterministic setup issues. Larger findings should be tracked as separate bugfix items.
- Re-enabling E2E tests in CI.
- Adding new integration tests (tracked in BUGFIX-007 and other items).

## Acceptance criteria

- [ ] The backend CI `Test` step no longer filters out `Mova.IntegrationTests`.
- [ ] The integration suite passes deterministically against the PostgreSQL service in GitHub Actions.
- [ ] Test database setup, migration, and teardown are reliable and do not leak data between tests.
- [ ] CI still fails fast on build errors and provides clear test output.
- [ ] The change is verified with at least one green CI run.

## Business rules

1. The Definition of Done requires automated tests that exercise the integrated API and database layers.
2. CI must be the authoritative quality gate for merges.
3. Excluding the integration suite to make CI green is not an acceptable fix.

## Validations

- Confirm the PostgreSQL service is healthy before tests run.
- Ensure the test connection string is available and correct.
- Run migrations or use a known test database setup.
- Prevent cross-test data leakage through transactions, database snapshots, or fixture isolation.
- Keep the existing `dotnet test` verbosity and failure reporting.

## Error cases

| Scenario | Expected behavior |
|---|---|
| Integration test fails due to a real product defect | CI fails; the defect is fixed or tracked as a separate bugfix. |
| Integration test is flaky due to ordering/timing | The flakiness is addressed through fixture isolation or explicit ordering. |
| PostgreSQL service is not ready | CI waits or fails clearly; the service configuration is fixed. |
| Test database already exists | The setup clears or recreates it deterministically. |

## Security considerations

- Do not log or expose the PostgreSQL connection string or credentials in CI output.
- Keep test secrets in GitHub Actions secrets or the environment file; do not commit them.
- Ensure the CI PostgreSQL user has no privileges beyond what integration tests require.

## Technical notes

### CI workflow

- Locate the backend `Test` step in `.github/workflows/ci.yml` and remove the `--filter "FullyQualifiedName!~Mova.IntegrationTests"` argument.
- Keep the PostgreSQL service and the connection string environment variable.
- Add a health check or wait step for PostgreSQL before running tests.
- Ensure `dotnet ef database update` or an equivalent migration step runs before tests if required by the integration setup.
- Consider using a custom test collection fixture that creates and drops the test database per run to avoid conflicts.

### Test reliability

- Review `tests/Mova.IntegrationTests/Reservations/ReservationsControllerTests.cs` and related fixtures for hard-coded IDs, ordering assumptions, and shared state.
- Use `WebApplicationFactory` with a PostgreSQL-backed `DbContext` and ensure each test or collection rolls back or re-creates data.
- Add `Collection` or `Assembly` fixtures if needed to serialize integration tests against the same database.

## Tests required

### CI

- A green GitHub Actions run that includes `Mova.IntegrationTests`.
- Multiple consecutive runs on the same commit to confirm determinism.

### Integration

- Existing `ReservationsControllerTests` must pass as part of the CI gate.
- New concurrent test from BUGFIX-007 must pass once the suite is re-enabled.

## Dependencies

- EPIC-06 — Reservations.
- BUGFIX-007 — Handle concurrent reservation creation conflicts (adds a new integration test that the CI gate must run).
- EPIC-02, EPIC-04, EPIC-05, EPIC-12 — Cross-epic integration coverage may reveal unrelated failures that should be fixed or tracked.

## Definition of Done

- [ ] The CI workflow no longer excludes integration tests.
- [ ] The integration suite is green in GitHub Actions on a feature branch and on `main` after merge.
- [ ] Any required determinism fixes are implemented and tested.
- [ ] No secrets or credentials are committed.
- [ ] The change is documented in the workflow if non-obvious.

## Project affected

- `.github/workflows/ci.yml`
- `tests/Mova.IntegrationTests`

## Suggested branch

`fix/BUGFIX-008-restore-integration-tests-in-ci`

## Instructions for Devin

Implement this change with minimal, focused workflow and test setup fixes. Do not modify source code to hide real failures; if a failure is outside the integration setup, track it as a separate bugfix. Prioritize deterministic database isolation and fast failure reporting.
