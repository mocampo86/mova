# API Design

## Conventions

- **Base path**: `/api/v1`
- **Resources** are expressed in plural English nouns: `/complexes`, `/courts`, `/reservations`.
- **Dates and times** are in ISO-8601 UTC in request/response bodies.
- **Pagination** is applied to all list endpoints that may return more than one page of results.
- **Idempotency** must be supported for reservation creation and mutation endpoints.

## HTTP methods

| Method | Usage |
|--------|-------|
| GET | Read, list, search |
| POST | Create a resource |
| PUT | Full update of an existing resource |
| PATCH | Partial update where supported |
| DELETE | Soft delete or cancellation where appropriate |

## URL patterns

```text
GET    /api/v1/complexes
GET    /api/v1/complexes/{id}
POST   /api/v1/complexes
PUT    /api/v1/complexes/{id}
PATCH  /api/v1/complexes/{id}/status

GET    /api/v1/admin/complexes

GET    /api/v1/complexes/{complexId}/courts
POST   /api/v1/complexes/{complexId}/courts
GET    /api/v1/courts/{id}
GET    /api/v1/complexes/{complexId}/courts/{courtId}
PUT    /api/v1/complexes/{complexId}/courts/{courtId}
PATCH  /api/v1/complexes/{complexId}/courts/{courtId}/status

GET    /api/v1/complexes/{complexId}/availability?courtId=&date=

PUT    /api/v1/complexes/{complexId}/courts/{courtId}/availability
GET    /api/v1/complexes/{complexId}/courts/{courtId}/availability

PUT    /api/v1/complexes/{complexId}/business-hours
GET    /api/v1/complexes/{complexId}/business-hours

GET    /api/v1/complexes/{complexId}/reservations
GET    /api/v1/complexes/{complexId}/reservations/{id}
POST   /api/v1/complexes/{complexId}/reservations
POST   /api/v1/complexes/{complexId}/reservations/me
PATCH  /api/v1/complexes/{complexId}/reservations/{id}/cancel
PATCH  /api/v1/complexes/{complexId}/reservations/{id}/status

GET    /api/v1/users/me/reservations
GET    /api/v1/users/me/reservations/history
PATCH  /api/v1/users/me/reservations/{id}/cancel

GET    /api/v1/complexes/{complexId}/users
GET    /api/v1/complexes/{complexId}/users/{userId}/reservations

POST   /api/v1/complexes/{complexId}/recurring-reservations/me
POST   /api/v1/complexes/{complexId}/recurring-reservations
PATCH  /api/v1/complexes/{complexId}/recurring-reservations/{id}/cancel
PATCH  /api/v1/complexes/{complexId}/recurring-reservations/{id}/future

PUT    /api/v1/complexes/{complexId}/configuration/recurring-reservations

POST   /api/v1/complexes/{complexId}/blocked-users
DELETE /api/v1/complexes/{complexId}/blocked-users/{id}

POST   /api/v1/complexes/{complexId}/court-blocks
```

## Request / response examples

### Create reservation

```http
POST /api/v1/complexes/{complexId}/reservations/me HTTP/1.1
Content-Type: application/json
Authorization: Bearer <jwt>
Idempotency-Key: <uuid>

{
  "courtId": "...",
  "startAt": "2026-08-10T20:00:00Z",
  "endAt": "2026-08-10T21:00:00Z",
  "notes": "Traer pelota"
}
```

### Create recurring reservation

```http
POST /api/v1/complexes/{complexId}/recurring-reservations/me HTTP/1.1
Content-Type: application/json
Authorization: Bearer <jwt>
Idempotency-Key: <uuid>

{
  "courtId": "...",
  "dayOfWeek": 1,
  "startTime": "14:00:00",
  "durationMinutes": 60,
  "startDate": "2026-08-10",
  "endDate": "2026-08-31",
  "notes": "Fixed weekly slot"
}
```

The response contains the recurring reservation rule plus the generated confirmed `Reservation` occurrences. Each occurrence has `source = "Recurring"` and `recurringReservationId` populated.

```json
{
  "id": "...",
  "complexId": "...",
  "courtId": "...",
  "userId": "...",
  "startAt": "2026-08-10T20:00:00Z",
  "endAt": "2026-08-10T21:00:00Z",
  "status": "Confirmed",
  "source": "Web",
  "createdAt": "2026-08-01T12:00:00Z",
  "updatedAt": "2026-08-01T12:00:00Z",
  "cancelledAt": null,
  "cancellationReason": null,
  "cancelledByUserId": null,
  "cancelledByUserName": null
}
```

### Update recurring reservation settings

Complex administrators can enable or disable regular user-created recurring reservations for their complex. The setting does not affect administrators, who can always create recurring reservations.

```http
PUT /api/v1/complexes/{complexId}/configuration/recurring-reservations HTTP/1.1
Content-Type: application/json
Authorization: Bearer <jwt>

{
  "allowUserRecurringReservations": false
}
```

The response returns the updated `SportsComplex` settings including the new value.

```json
{
  "id": "...",
  "name": "...",
  "allowUserRecurringReservations": false,
  "status": "Active"
}
```

## Error contract

All errors follow a consistent envelope:

```json
{
  "error": {
    "code": "RESERVATION_CONFLICT",
    "message": "The selected time is no longer available.",
    "details": {
      "courtId": "...",
      "requestedStart": "2026-08-10T20:00:00Z",
      "requestedEnd": "2026-08-10T21:00:00Z"
    },
    "traceId": "00-..."
  }
}
```

Common error codes:

| Code | HTTP Status | Meaning |
|------|-------------|---------|
| VALIDATION_ERROR | 400 | Request fails FluentValidation or Zod validation |
| RESERVATION_CONFLICT | 409 | Overlapping reservation or block |
|| RECURRING_RESERVATIONS_DISABLED | 409 | User recurring reservations are disabled for the complex |
| USER_BLOCKED | 403 | User is blocked in the complex |
| UNAUTHORIZED | 401 | Missing or invalid JWT |
| FORBIDDEN | 403 | Insufficient role or complex access |
| NOT_FOUND | 404 | Resource does not exist or is not accessible |
| CONCURRENCY_ERROR | 409 | Optimistic concurrency conflict |

## Pagination contract

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalItems": 0,
  "totalPages": 0
}
```

Query parameters:

- `page` (int, default 1)
- `pageSize` (int, default 20, max 100)
- `sort` (string, e.g. `startAt:desc`)
- `filter` (feature-specific)

## Idempotency

- Use an `Idempotency-Key` header with a UUID for `POST`, `PUT`, and `PATCH` mutation endpoints.
- The server stores the key and response for a configurable TTL (default 24 hours).
- Replayed requests with the same key return the stored response without re-executing business logic.

## Versioning

- All API paths start with `/api/v1`.
- Breaking changes in the future require a new version (`/api/v2`) and a documented deprecation plan.

## OpenAPI

- The API exposes an OpenAPI/Swagger document generated from controller metadata and XML comments.
- Available at `/swagger/v1/swagger.json` and `/swagger/index.html` in development.
- Contract DTOs are located in `Mova.Contracts`.

## Security headers

- All endpoints except authentication and health require a valid JWT.
- `Authorization: Bearer <jwt>` is mandatory for authenticated routes.
- Admin endpoints additionally check `SportsComplexId` authorization.
