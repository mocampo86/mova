# BUGFIX-007 — Handle concurrent reservation creation conflicts

## Status

Ready

## Priority

Medium

## Source

EPIC-06 review finding: **MEDIUM — The serializable double-booking protection is not proven under concurrent requests.**

## Problem

Reservation creation in `CreateReservationHandler` correctly performs the overlap query, court-block query, and insert inside a serializable transaction. The available integration test proves a second sequential overlapping request returns `409`, but no test starts competing requests against separate contexts/connections. In addition, CI excludes the integration suite that would run such a test.

The key epic acceptance criterion is concurrency protection: two simultaneous attempts for the same slot must commit exactly one reservation and return a safe conflict response for the other. Without a real concurrent database test, the production race behavior is unproven, and a PostgreSQL serialization failure could be mapped to an unhandled `500` server error rather than the documented `409 RESERVATION_CONFLICT`.

## Objective

Prove and harden the concurrent reservation creation path so that two overlapping requests execute exactly one successful booking and the other receives a safe, deterministic `409 Conflict` response.

## Scope

- `CreateReservationHandler` in `src/Mova.Application/Reservations/Handlers/CreateReservationHandler.cs`.
- `ReservationRepository` overlap query in `src/Mova.Infrastructure/Persistence/Repositories/ReservationRepository.cs`.
- `ReservationsController` error mapping in `src/Mova.Api/Controllers/ReservationsController.cs`.
- `ReservationsControllerTests` in `tests/Mova.IntegrationTests/Reservations/ReservationsControllerTests.cs`.
- CI re-enablement for integration tests (tracked separately in BUGFIX-008).

## Out of scope

- Changing the serializable transaction isolation level unless the test proves it insufficient.
- Adding database-level exclusion constraints or triggers (consider as a separate hardening item if the concurrent test reveals gaps).
- Idempotency implementation (tracked in BUGFIX-006).

## Acceptance criteria

- [ ] A PostgreSQL-backed integration test submits two independent `POST` requests for the same court and overlapping time range concurrently.
- [ ] Exactly one request returns `201 Created` with a persisted active reservation.
- [ ] The other request returns `409 Conflict` with a documented error code such as `RESERVATION_CONFLICT`.
- [ ] The database contains exactly one active reservation for the overlapping range.
- [ ] A PostgreSQL `SerializationFailure` or `DbUpdateException` caused by the serializable transaction is consistently mapped to `409 Conflict`, not `500 Internal Server Error`.
- [ ] Existing sequential overlap tests continue to pass.

## Business rules

1. Active reservations cannot overlap on the same court.
2. Cancelled reservations do not occupy availability.
3. Court blocks also prevent reservations for the same court and overlapping time.
4. Concurrent creation attempts must resolve deterministically: exactly one succeeds; the other is rejected with a conflict.

## Validations

- The court exists, belongs to the requested complex, and is active.
- The user exists, is active, and is not blocked in the complex.
- No active reservation overlaps the requested time range on the same court.
- No court block overlaps the requested time range.
- Serial transaction isolation is used around the conflict checks and insert.
- Serialization/database concurrency failures are translated to a client-safe conflict response.

## Error cases

| Scenario | Expected behavior |
|---|---|
| Two simultaneous requests for the same court and overlapping time | One `201 Created`; the other `409 Conflict` with `RESERVATION_CONFLICT`. |
| Sequential second overlapping request | `409 Conflict` with `RESERVATION_CONFLICT`. |
| Concurrent request with a court block | `409 Conflict` with the appropriate block/court-block error code. |
| Database serialization failure | `409 Conflict` with `RESERVATION_CONFLICT`; no `500`. |

## Security considerations

- Do not expose internal database or transaction details in the conflict response.
- Ensure both requests are authenticated and authorized before the conflict check.
- Maintain the existing complex- and court-scoping rules under concurrency.

## Technical notes

### Backend

- In `CreateReservationHandler`, ensure the entire overlap/court-block/insert sequence runs inside a serializable transaction.
- Catch `DbUpdateException` (or `NpgsqlException`/`PostgresException` with `SerializationFailure`) inside the handler or a middleware and map it to the existing conflict response when the inner exception indicates a serialization/concurrency failure.
- Add a dedicated exception or error code such as `RESERVATION_CONFLICT` with status `409`.
- The integration test should use two separate `HttpClient` instances, separate `DbContext` instances, or direct parallel `POST` calls to avoid test-level synchronization.
- Use `Parallel.ForEach` or `Task.WhenAll` with a small, deterministic delay to increase the chance both requests reach the overlap query before either commits.

### Tests

- Add the test to `tests/Mova.IntegrationTests/Reservations/ReservationsControllerTests.cs`.
- Assert exactly one `201` response and one `409` response.
- Assert the database has exactly one active reservation for the target range.

## Tests required

### Integration

- Concurrent overlapping `POST /api/v1/complexes/{complexId}/reservations/me` results in one success and one `409 RESERVATION_CONFLICT`.
- Concurrent overlapping `POST /api/v1/complexes/{complexId}/reservations` (manual creation) shows the same behavior.
- A serialization failure is not returned as `500`.

## Dependencies

- EPIC-06 — Reservations, especially US-029 and US-034.
- BUGFIX-008 — Restore integration tests in CI (so the new test runs in the pipeline).

## Definition of Done

- [ ] All acceptance criteria are implemented and verifiable.
- [ ] The new concurrent integration test passes against PostgreSQL.
- [ ] Existing backend tests continue to pass.
- [ ] CI is capable of running the integration suite (final gate depends on BUGFIX-008).
- [ ] No sensitive data is logged or committed.

## Project affected

- `src/Mova.Application`
- `src/Mova.Infrastructure`
- `src/Mova.Api`
- `tests/Mova.IntegrationTests`

## Suggested branch

`fix/BUGFIX-007-handle-concurrent-reservation-creation-conflicts`

## Instructions for Devin

Implement this bugfix with minimal, focused changes. The primary deliverable is a deterministic concurrent integration test that proves the serializable transaction protects the court slot. If the test reveals a `500` from a serialization failure, add safe mapping to `409` without weakening the transaction isolation.
