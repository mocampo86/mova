# Idempotency

## Overview

Mutating endpoints must be idempotent so that retrying a request due to network issues does not produce duplicate side effects.

## Mechanism

Clients include an `Idempotency-Key` header with a UUID:

```http
POST /api/v1/reservations HTTP/1.1
Content-Type: application/json
Idempotency-Key: 11111111-1111-1111-1111-111111111111
Authorization: Bearer <jwt>

{
  "courtId": "...",
  "startAt": "2026-08-10T20:00:00Z",
  "endAt": "2026-08-10T21:00:00Z"
}
```

## Server behavior

1. The server stores the `Idempotency-Key` and the response for a configured TTL (default 24 hours).
2. If the same key is replayed with the same request payload, the server returns the stored response without re-executing business logic.
3. If the same key is reused with a different payload, the server returns `409 Conflict` with code `IDEMPOTENCY_KEY_REUSED`.

## Scope

Required on:

- `POST /api/v1/reservations`
- `POST /api/v1/recurring-reservations`
- `POST /api/v1/complexes/{complexId}/court-blocks`
- `POST /api/v1/complexes/{complexId}/blocked-users`
- `PATCH /api/v1/reservations/{id}/cancel`
- `PUT /api/v1/complexes/{id}`

Recommended on all other mutating endpoints.

## Idempotency key generation

- The client is responsible for generating and storing the key during a request attempt.
- Use UUID v4 or any sufficiently random 128-bit identifier.
- Reuse the same key for retries of the same logical operation.

## Storage

- Keys are stored in a short-lived cache or database table.
- In the MVP this can be an in-memory cache (e.g. IMemoryCache) or a Redis-backed cache if available.
- In production, prefer a distributed cache to survive process restarts and horizontal scaling.

## Error code

| Code | HTTP Status | Meaning |
|------|-------------|---------|
| `IDEMPOTENCY_KEY_REUSED` | 409 | The key was already used with a different request body |
| `IDEMPOTENCY_KEY_REQUIRED` | 400 | The endpoint requires the header and it is missing |

## Implementation guidelines

- Validate the key format if stored.
- Store the response body and status code, not just the key.
- Do not replay a stored response if it was a `5xx` or `409` business conflict; allow the client to retry with a new key.
- TTL should be long enough for client retries (minimum 5 minutes, default 24 hours).
