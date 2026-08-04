# Pagination

## Default behavior

All list endpoints that may return more than one page must support pagination with:

- `page` (int, default `1`)
- `pageSize` (int, default `20`, maximum `100`)

## Response contract

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalItems": 150,
  "totalPages": 8
}
```

## Fields

| Field | Type | Description |
|-------|------|-------------|
| `items` | array | The current page of results |
| `page` | int | Current page number (1-based) |
| `pageSize` | int | Number of items per page |
| `totalItems` | int | Total number of items matching the query |
| `totalPages` | int | `ceil(totalItems / pageSize)` |

## Request examples

```http
GET /api/v1/complexes?page=1&pageSize=20
GET /api/v1/complexes/{complexId}/reservations?page=2&pageSize=50&sort=startAt:desc
GET /api/v1/users/me/reservations?page=1&pageSize=10&status=Confirmed
```

## Sorting

Use a `sort` query parameter with field and direction separated by a colon:

```text
sort=createdAt:desc
sort=name:asc
```

Multiple sort fields can be comma-separated:

```text
sort=startAt:asc,createdAt:desc
```

## Filtering

Use query parameters named after the field or a `filter` parameter:

```text
GET /api/v1/complexes/{complexId}/reservations?status=Confirmed&date=2026-08-10
GET /api/v1/complexes?city=Buenos+Aires
```

Recommended patterns:

- Exact match: `?status=Confirmed`
- Date range: `?from=2026-08-01&to=2026-08-31`
- Contains (case-insensitive): `?name=search-term` (document the behavior explicitly)

## Empty pages

When `page` exceeds `totalPages`, the API returns an empty `items` array:

```json
{
  "items": [],
  "page": 100,
  "pageSize": 20,
  "totalItems": 50,
  "totalPages": 3
}
```

## Implementation notes

- Use IQueryable deferred execution and `Skip`/`Take` in the repository or handler.
- Always calculate `totalItems` with the same filters as the paged query.
- Avoid returning the full entity graph; project to DTOs defined in `Mova.Contracts`.
- For very large lists, consider keyset pagination as a future optimization.
