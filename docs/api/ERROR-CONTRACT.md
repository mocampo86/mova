# Error Contract

## Envelope

All API errors use the following JSON envelope:

```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable description.",
    "details": {},
    "traceId": "00-..."
  }
}
```

## Fields

| Field | Type | Description |
|-------|------|-------------|
| `code` | string | Machine-readable error code, uppercase with underscores |
| `message` | string | Human-readable explanation, safe to show to end users |
| `details` | object | Optional structured context (field names, ids, etc.) |
| `traceId` | string | Correlation identifier from `Activity.Current.Id` or header |

## Common error codes

| Code | HTTP Status | Meaning |
|------|-------------|---------|
| `VALIDATION_ERROR` | 400 | Request body or query parameters failed validation |
| `INVALID_JSON` | 400 | Request body is not valid JSON |
| `MISSING_FIELD` | 400 | A required field is missing |
| `INVALID_FIELD_VALUE` | 400 | A field value is not valid |
| `UNAUTHORIZED` | 401 | Missing or invalid JWT |
| `TOKEN_EXPIRED` | 401 | JWT has expired |
| `FORBIDDEN` | 403 | User lacks permission for the operation |
| `USER_BLOCKED` | 403 | User is blocked in the requested complex |
| `NOT_FOUND` | 404 | Resource does not exist or is inaccessible |
| `RESERVATION_CONFLICT` | 409 | Requested time slot is no longer available |
| `COURT_BLOCK_CONFLICT` | 409 | Time range overlaps with a court block |
| `CONCURRENCY_ERROR` | 409 | Optimistic concurrency conflict |
| `RECURRING_CONFLICT` | 409 | One or more occurrences conflict with existing reservations |
| `RATE_LIMIT_EXCEEDED` | 429 | Too many requests |
| `INTERNAL_SERVER_ERROR` | 500 | Unexpected server error |

## Validation error details example

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred.",
    "details": {
      "PhoneNumber": ["Phone number is required."],
      "Email": ["Invalid email format."]
    },
    "traceId": "00-1234567890abcdef-1234567890abcdef-00"
  }
}
```

## Business error example

```json
{
  "error": {
    "code": "RESERVATION_CONFLICT",
    "message": "The selected time is no longer available.",
    "details": {
      "courtId": "11111111-1111-1111-1111-111111111111",
      "requestedStart": "2026-08-10T20:00:00Z",
      "requestedEnd": "2026-08-10T21:00:00Z"
    },
    "traceId": "00-abcdef1234567890-abcdef1234567890-00"
  }
}
```

## Implementation guidelines

- Use problem details (`Microsoft.AspNetCore.Mvc.ProblemDetails`) as a base when possible.
- Do not leak internal exceptions or stack traces to the client in production.
- Do not include tokens, passwords, phone numbers, or other sensitive data in `details`.
- Log the full exception server-side with correlation IDs.
- Keep `message` concise and user-friendly.
