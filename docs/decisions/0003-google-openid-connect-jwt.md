# ADR-0003: Google OpenID Connect and JWT

## Status

Accepted

## Context

Users must authenticate securely without the platform managing passwords. We needed a simple and trusted identity provider and a stateless mechanism for API authorization.

## Decision

We will use **Google OpenID Connect** for user authentication and **JWT Bearer tokens** for API authorization.

## Consequences

### Positive

- No need to store or manage user passwords.
- Reduced login friction because users can use their existing Google accounts.
- JWT enables stateless, scalable API authorization.
- Easy integration with ASP.NET Core authentication middleware.

### Negative

- The platform depends on Google as an external identity provider.
- Users without Google accounts cannot log in unless another provider is added later.
- Token revocation requires short-lived access tokens and refresh token handling.

## Alternatives considered

- **Local username/password with ASP.NET Core Identity**: rejected to avoid password management, breach risks, and recovery flows in the MVP.
- **Microsoft or Auth0**: deferred; Google is widely used by the target audience and easy to configure.

## Implementation notes

- The API validates Google ID tokens and issues platform JWTs.
- JWT claims include `sub` (user id), `email`, `name`, `roles`, and authorized `complexes` for administrators.
- Refresh tokens, if used, are stored in `HttpOnly`, `Secure`, `SameSite=Strict` cookies.

## Related decisions

- ADR-0001: Modular Monolith.

## References

- `mova-project-overview.md` section 12.
- `.ai-kit/docs/architecture/AUTHENTICATION.md`.
