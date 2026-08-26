# BUGFIX-006 — Implement reservation mutation idempotency

## Status

Ready

## Priority

High

## Source

EPIC-06 review finding: **HIGH — Reservation mutations do not implement the required idempotency contract.**

## Problem

The API design requires an `Idempotency-Key` UUID for `POST`, `PUT`, and `PATCH` mutation endpoints and requires the server to replay the stored response for the same key within a configurable TTL. The reservation controller accepts no idempotency header and has no persistence/replay mechanism. The frontend mutation calls also send no such header. A repository-wide source and test search found no idempotency implementation or tests.

This means retried browser requests, mobile reconnects, proxies, or duplicate clicks can re-execute reservation mutation logic. Duplicate creation for an identical active time range normally conflicts, but duplicate status/cancellation processing and retries after a lost response are not guaranteed to return the original response. The public API behavior contradicts the documented contract.

## Objective

Implement the documented `Idempotency-Key` contract for reservation creation and mutation endpoints so that repeated requests with the same key return the original stored response without re-executing business logic.

## Scope

- `ReservationsController` mutation endpoints in `src/Mova.Api/Controllers/ReservationsController.cs`:
  - `POST /api/v1/complexes/{complexId}/reservations/me`
  - `POST /api/v1/complexes/{complexId}/reservations`
  - `PATCH /api/v1/complexes/{complexId}/reservations/{reservationId}/cancel`
  - `PATCH /api/v1/complexes/{complexId}/reservations/{reservationId}/status`
- Frontend reservation mutation calls in `src/mova-web/src/features/reservations/reservationApi.ts`.
- Idempotency storage mechanism (cross-cutting or reservation-scoped).
- Reservation integration and frontend tests.

## Out of scope

- Requiring `Idempotency-Key` for read endpoints or for non-reservation epics. This bugfix implements the contract for reservation mutations; a future platform-wide middleware can generalize the pattern.
- Database schema changes outside the reservation/idempotency store.
- Audit logging of idempotency replay.

## Acceptance criteria

- [ ] The reservation endpoints above require a valid `Idempotency-Key` UUID header and reject requests with an invalid or missing header.
- [ ] The server stores the canonical response keyed by authenticated actor, route/operation, and `Idempotency-Key` for the documented TTL (default 24 hours).
- [ ] Replayed requests with the same key, same actor, and same operation return the stored response without re-executing business logic.
- [ ] Replayed requests with the same key but a different actor are treated as a new request.
- [ ] The frontend sends a generated UUID as `Idempotency-Key` for each reservation mutation.
- [ ] Idempotency records are cleaned up after the configured TTL.
- [ ] Integration tests cover same-key replay, different-key behavior, invalid keys, concurrent duplicate requests, and expiry.
- [ ] The implementation does not cache error responses in a way that masks legitimate retries of a previously failed request.

## Business rules

1. `POST`, `PUT`, and `PATCH` reservation mutations must accept and validate an `Idempotency-Key` header.
2. The server is the authoritative replay point for idempotent mutations.
3. Idempotency keys are scoped to the authenticated actor and the operation; one actor cannot replay another actor's mutation.
4. A stored response may be returned for the configured TTL, after which the key is eligible for reuse.
5. A failed request may be retried with the same key or a new key; the server must not store and replay a final error response as a success.

## Validations

- Validate that `Idempotency-Key` is a non-empty, well-formed UUID.
- Return `400 Bad Request` with a stable error code when the header is missing or invalid.
- Return the stored response for a known key; execute the request normally for a new key.
- Scope the idempotency record to the authenticated user or complex administrator and the route/operation.

## Error cases

| Scenario | Expected behavior |
|---|---|
| Mutation request without `Idempotency-Key` | `400 Bad Request` with a clear error code such as `IDEMPOTENCY_KEY_REQUIRED`. |
| Mutation request with an invalid `Idempotency-Key` | `400 Bad Request` with validation details. |
| First valid mutation with a new key | Operation executes and the response is stored. |
| Retry with the same key, same actor, within TTL | Stored response is returned without re-execution. |
| Retry with a used key but a different actor | Treated as a new request or rejected with `403 Forbidden` per the chosen design. |
| Retry after TTL | Operation executes again with the same key. |
| Concurrent retries with the same key | One request completes and stores the response; others await or receive the stored response. |

## Security considerations

- Scope idempotency records to the authenticated actor so one user cannot replay another user's mutation.
- Do not store or return sensitive data (phone numbers, JWT values, internal identifiers) in the idempotency record.
- Store idempotency records durably and clean them up after the TTL to avoid data leakage and storage growth.
- Ensure idempotency replay cannot be used to bypass authorization or ownership checks.

## Technical notes

### Backend

- Introduce an `IIdempotencyRecord` entity or a dedicated table keyed by `ActorId`, `Operation`, `IdempotencyKey`, storing the status code, headers, and body.
- Implement an idempotency middleware, filter, or handler decorator that runs before business logic and after the response is produced.
- For the reservation controller, apply the header requirement and replay logic consistently to all `POST` and `PATCH` endpoints.
- Use the existing transaction/UnitOfWork pattern so idempotency record creation is part of the mutation transaction where appropriate.
- Return the original HTTP status code and body on replay.

### Frontend

- Generate a UUID for each mutation call in `reservationApi.ts`.
- Send the `Idempotency-Key` header with `POST` and `PATCH` requests.
- Preserve the same key when retrying after a network error, and allow a new key for explicit user re-submission.

## Tests required

### Unit

- Reservation command handlers do not depend on idempotency logic directly; the controller or middleware manages the key.
- Idempotency storage/replay service unit tests for key lookup, TTL, and actor scoping.

### Integration

- Same `Idempotency-Key` replay returns the original `201 Created` or `409 Conflict` response.
- Different key for the same payload executes a second time.
- Missing/invalid key returns `400`.
- Concurrent duplicate-key requests result in one execution and one replayed response.
- Expired key allows a new execution.

### Frontend

- Reservation creation/cancellation mutations include an `Idempotency-Key` header.
- A network retry reuses the same key.

## Dependencies

- EPIC-06 — Reservations, especially US-029, US-032, US-034, US-035, and US-036.
- `API-DESIGN.md` idempotency section.

## Definition of Done

- [ ] All acceptance criteria are implemented and verifiable.
- [ ] Backend build and the full backend test suite pass, including integration tests.
- [ ] Frontend lint, build, and tests pass.
- [ ] API documentation/OpenAPI describes the header requirement and replay semantics.
- [ ] No idempotency keys, responses, or JWT values are logged or committed.

## Project affected

- `src/Mova.Api`
- `src/Mova.Application`
- `src/Mova.Infrastructure`
- `src/Mova.Domain`
- `src/mova-web`
- `tests/Mova.UnitTests`
- `tests/Mova.IntegrationTests`
- `src/mova-web/src/pages` and `src/mova-web/src/features/reservations`

## Suggested branch

`fix/BUGFIX-006-implement-reservation-mutation-idempotency`

## Instructions for Devin

Implement this bugfix with minimal, focused changes. Prefer a reusable application/infrastructure concern over duplicating idempotency logic in each reservation handler. Ensure the frontend generates and sends the key, and that replay truly returns the stored response without re-executing business logic.
