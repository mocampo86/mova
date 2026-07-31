# Authentication & Authorization

## Overview

The platform uses **Google OpenID Connect** for user authentication and **JWT Bearer tokens** for API authorization. The backend validates tokens and enforces role-based and resource-based authorization.

## Authentication flow

```text
[User]
  │
  ├─ clicks "Sign in with Google" in the React SPA
  │
  ▼
[Google Identity]
  │
  ├─ returns ID token with claims (sub, email, name, picture)
  │
  ▼
[React SPA]
  │
  ├─ sends ID token to /api/v1/auth/google
  │
  ▼
[API]
  ├─ validates ID token signature and claims
  ├─ creates or updates user
  ├─ issues platform JWT (access + refresh optional)
  │
  ▼
[React SPA]
  ├─ stores access token securely (memory; refresh in httpOnly cookie if used)
  ├─ sends Bearer token on every authenticated request
```

## Token contents

The platform JWT contains:

```json
{
  "sub": "<user-id>",
  "email": "user@example.com",
  "name": "Juan Pérez",
  "roles": ["User"],
  "iss": "ReservaCanchas",
  "aud": "ReservaCanchas.Api",
  "iat": 1234567890,
  "exp": 1234571490
}
```

Administrators also receive:

```json
{
  "complexes": [
    { "complexId": "...", "role": "ComplexAdmin" }
  ]
}
```

## Roles

| Role | Description |
|------|-------------|
| `User` | Standard end user who makes reservations |
| `ComplexAdmin` | Administrator of one or more complexes |
| `SuperAdmin` | Global platform administrator |

A user may hold multiple roles. Complex-specific roles are stored in `ComplexAdministrators` and resolved at authentication or request time.

## Authorization policies

### User policy

Any authenticated user.

### ComplexAdmin policy

User must be an active administrator of the requested `SportsComplexId`. The API must validate that the user's associated `ComplexAdministrator` record exists and is active.

### SuperAdmin policy

User must have the `SuperAdmin` role.

### Complex access rule

For any endpoint scoped to a complex (`/api/v1/complexes/{complexId}/...`):

1. Parse `SportsComplexId` from route or body.
2. If user is `SuperAdmin`, allow.
3. If user is `ComplexAdmin` for that complex, allow.
4. Otherwise, return `403 Forbidden`.

## User profile completion

After the first Google login, if the user has not completed their profile (`PhoneNumber` missing), the frontend must redirect to `/complete-profile` before allowing reservations.

## Security requirements

- All tokens must be transmitted over HTTPS in production.
- Tokens must be short-lived (access token: 15 minutes; refresh token: 7 days if used).
- Refresh tokens must be stored in `HttpOnly`, `Secure`, `SameSite=Strict` cookies.
- Google client secrets must be stored in Azure Key Vault (production) or user secrets (local).
- No token values or phone numbers logged.
- CORS must restrict origin to the deployed frontend URL.

## Passwords

The platform does not manage user passwords. Complex administrators authenticate through Google as well; no local password store is required in the MVP.

## Phone verification (future)

The `PhoneVerified` flag is stored but not verified in the MVP. A future implementation may integrate SMS or WhatsApp OTP.

## Protected routes

| Area | Required role |
|------|---------------|
| Public landing, complex search | None |
| User portal, reservations | `User` |
| Complex admin panel | `ComplexAdmin` for the complex |
| Super admin panel | `SuperAdmin` |

## Implementation notes

- Use ASP.NET Core `AddJwtBearer` with Google `ClientId` and `ClientSecret` for the external validation step.
- Use custom `IAuthorizationHandler` for complex-scoped access.
- Consider `PolicyServer` or a static policy mapping only if authorization complexity grows beyond roles and resource ownership.
