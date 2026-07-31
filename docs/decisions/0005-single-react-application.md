# ADR-0005: Single React Application with Role-Based Layouts

## Status

Accepted

## Context

The platform needs a public landing page, a user portal, and an administrative panel. We had to decide whether to split these into separate applications or keep them in one.

## Decision

For the MVP, we will build a **single React application** with separate layouts for public, user, and administrator views. Routes and components will be protected by role-based access checks.

## Consequences

### Positive

- Simpler build, deployment, and asset management.
- Shared components, hooks, and theme across all user types.
- Faster iteration and lower overhead in the early stage.

### Negative

- The application bundle contains code for all roles; lazy loading and code splitting should be used for admin-only views.
- A larger bundle may impact initial load time if not optimized.
- Admin-only features could accidentally be exposed if route guards are not correctly implemented.

## Implementation notes

- Use `PublicLayout`, `UserLayout`, and `AdminLayout`.
- Protect routes using role checks derived from the JWT or user context.
- Lazy load admin and complex panels to reduce initial bundle size.

## Alternatives considered

- **Separate admin and user applications**: rejected because it increases build and deployment complexity for the MVP.
- **Separate landing site**: rejected because the landing page can be a public layout within the same application.

## Related decisions

- ADR-0004: React with TypeScript and Vite.

## References

- `mova-project-overview.md` section 7.3.
- `.ai-kit/docs/architecture/SOLUTION-ARCHITECTURE.md`.
