# API Design

## Conventions

- **Base path**: `/api/v1`
- **Resources** are expressed in plural English nouns: `/complexes`, `/courts`, `/reservations`.
- **Dates and times** are in ISO-8601 UTC in request/response bodies.
- **Business hours** and **court availability rules** represent local wall-clock values for the complex's configured time zone.
- **Availability** is derived from the complex's IANA `timeZoneId` and a complex-local `date` query parameter; `utcOffsetMinutes` is no longer used.
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
GET    /api/v1/complexes/{id}/admin
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

GET    /api/v1/complexes/{complexId}/availability?courtId=&date=   # date is local to the complex's TimeZoneId

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
GET    /api/v1/complexes/{complexId}/users/search
GET    /api/v1/complexes/{complexId}/users/{userId}/reservations

GET    /api/v1/complexes/{complexId}/recurring-reservations
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

### Create complex

```http
POST /api/v1/complexes HTTP/1.1
Content-Type: application/json
Authorization: Bearer <jwt>

{
  "name": "Club Padel",
  "description": "A premium padel club",
  "address": "Av. Libertador 1234",
  "city": "Buenos Aires",
  "latitude": -34.6,
  "longitude": -58.3,
  "phoneNumber": "+54 11 1234 5678",
  "email": "contact@clubpadel.com",
  "timeZoneId": "America/Argentina/Buenos_Aires"
}
```

`timeZoneId` is a required IANA identifier. A complex created or updated with a missing or invalid `timeZoneId` is rejected.

### Update complex

```http
PUT /api/v1/complexes/{complexId} HTTP/1.1
Content-Type: application/json
Authorization: Bearer <jwt>

{
  "name": "Club Padel",
  "description": "Updated description",
  "address": "Av. Libertador 1234",
  "city": "Buenos Aires",
  "latitude": -34.6,
  "longitude": -58.3,
  "phoneNumber": "+54 11 1234 5678",
  "email": "contact@clubpadel.com",
  "timeZoneId": "America/Argentina/Buenos_Aires"
}
```

### Get availability

The `date` query parameter is a local date in the complex's configured time zone. The response contains UTC slot instants.

```http
GET /api/v1/complexes/{complexId}/availability?courtId={courtId}&date=2026-08-10 HTTP/1.1
```

If the complex has no configured time zone, the endpoint returns `422 Unprocessable Entity` with the error code `TIMEZONE_NOT_CONFIGURED`.

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

### List recurring reservations

Complex administrators can list recurring reservations for their complex with optional filtering and pagination.

```http
GET /api/v1/complexes/{complexId}/recurring-reservations?page=1&pageSize=10&status=Active&sort=createdAt%3Adesc HTTP/1.1
Authorization: Bearer <jwt>
```

The response is a paginated list of recurring reservation series. Each item includes display-friendly court and user names.

```json
{
  "items": [
    {
      "id": "...",
      "complexId": "...",
      "courtId": "...",
      "courtName": "Court One",
      "userId": "...",
      "userName": "Test User",
      "dayOfWeek": 1,
      "startTime": "14:00:00",
      "durationMinutes": 60,
      "startDate": "2026-08-10",
      "endDate": "2026-08-31",
      "status": "Active",
      "createdAt": "2026-08-01T12:00:00Z",
      "updatedAt": "2026-08-01T12:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalItems": 1,
  "totalPages": 1
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
