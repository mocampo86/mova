# ADR-0004: React with TypeScript and Vite

## Status

Accepted

## Context

The frontend must be a responsive, modern web application with a mobile-first experience. We needed a framework, language, and build tool that supports rapid development and maintainability.

## Decision

We will build the frontend with **React**, **TypeScript**, and **Vite**.

## Consequences

### Positive

- React provides a large ecosystem and component model.
- TypeScript catches errors at compile time and improves developer experience.
- Vite offers fast development builds and optimized production bundles.
- The stack is widely adopted and easy to hire for.

### Negative

- React ecosystem changes frequently; dependencies must be kept up to date.
- Vite requires modern browser support; polyfills may be needed for very old browsers.

## Supporting libraries

- React Router for routing.
- TanStack Query for server state.
- React Hook Form and Zod for forms and validation.
- Material UI for components and theming.
- Vitest and React Testing Library for unit tests.
- Playwright for end-to-end tests.

## Alternatives considered

- **Angular**: rejected because React with Vite is lighter and more flexible for the MVP.
- **Next.js**: deferred to keep deployment simple (Static Web Apps target) and avoid server-side rendering complexity in the MVP.
- **Vue.js**: considered but the team preferred the React ecosystem.

## Related decisions

- ADR-0005: Single React Application with Role-Based Layouts.

## References

- `mova-project-overview.md` section 8.2.
- `.ai-kit/docs/architecture/SOLUTION-ARCHITECTURE.md`.
