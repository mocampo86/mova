# API Conventions

## Base URL and versioning

- All API paths are prefixed with `/api/v1`.
- Breaking changes require a new version (`/api/v2`) and a documented deprecation plan.

## Resource naming

- Resources are plural nouns in English: `/complexes`, `/courts`, `/reservations`.
- Hierarchical relationships use nested paths: `/complexes/{complexId}/courts`.
- Avoid verbs in URLs; use HTTP methods to express actions.

## HTTP methods

| Method | Semantics |
|--------|-----------|
| GET | Retrieve a resource or collection |
| POST | Create a resource |
| PUT | Replace a resource completely |
| PATCH | Partially update a resource |
| DELETE | Delete, deactivate, or cancel a resource |

## Examples

```text
GET    /api/v1/complexes
GET    /api/v1/complexes/{id}
POST   /api/v1/complexes
PUT    /api/v1/complexes/{id}

GET    /api/v1/complexes/{complexId}/courts
POST   /api/v1/complexes/{complexId}/courts

GET    /api/v1/courts/{id}
PUT    /api/v1/courts/{id}
PATCH  /api/v1/courts/{id}

GET    /api/v1/complexes/{complexId}/availability?date=2026-08-10

POST   /api/v1/complexes/{complexId}/reservations
GET    /api/v1/complexes/{complexId}/reservations
GET    /api/v1/complexes/{complexId}/reservations/{id}
PATCH  /api/v1/complexes/{complexId}/reservations/{id}/cancel
PATCH  /api/v1/complexes/{complexId}/reservations/{id}/status

GET    /api/v1/users/me/reservations
GET    /api/v1/users/me/reservations/history
PATCH  /api/v1/users/me/reservations/{id}/cancel

POST   /api/v1/complexes/{complexId}/court-blocks
DELETE /api/v1/complexes/{complexId}/court-blocks/{id}

POST   /api/v1/complexes/{complexId}/blocked-users
DELETE /api/v1/complexes/{complexId}/blocked-users/{id}
```

## Content negotiation

- Requests and responses use JSON (`application/json`).
- The API must accept `application/json` and return `application/json`.
- UTF-8 encoding is required.

## Date and time

- All date and time values in request and response bodies are ISO 8601 UTC, e.g. `2026-08-10T20:00:00Z`.
- Frontend is responsible for converting UTC to local time for display.
- Query parameters that accept dates must follow `yyyy-MM-dd` for `DateOnly` and ISO 8601 for `DateTime`.

## Status codes

| Code | Usage |
|------|-------|
| 200 OK | Successful GET, PUT, PATCH |
| 201 Created | Successful POST with resource creation |
| 204 No Content | Successful DELETE or action with no body |
| 400 Bad Request | Validation or malformed request |
| 401 Unauthorized | Missing or invalid authentication |
| 403 Forbidden | Insufficient permissions or blocked user |
| 404 Not Found | Resource does not exist or is not accessible |
| 409 Conflict | Business conflict (e.g. overlapping reservation) |
| 422 Unprocessable Entity | Semantic validation failure (optional) |
| 429 Too Many Requests | Rate limit exceeded |
| 500 Internal Server Error | Unexpected server error |

## Query parameters

Common patterns:

- `page` and `pageSize` for pagination.
- `sort` for ordering, e.g. `sort=startAt:desc`.
- `filter` for search, e.g. `filter=sport:football`.
- `date` for availability and schedule queries.

## OpenAPI

- The API must expose an OpenAPI document at `/swagger/v1/swagger.json`.
- Swagger UI is available at `/swagger/index.html` in development.
- Controllers and DTOs must be documented with XML comments or attributes.

## Idempotency

Mutating endpoints support the `Idempotency-Key` header. See `IDEMPOTENCY.md` for details.

## Headers

| Header | Required | Description |
|--------|----------|-------------|
| `Authorization` | On protected endpoints | `Bearer <jwt>` |
| `Content-Type` | On POST/PUT/PATCH | `application/json` |
| `Idempotency-Key` | Recommended on mutating endpoints | UUID |
